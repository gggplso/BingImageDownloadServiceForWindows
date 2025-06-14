using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClassLibrary.Classes;
using System.Security.Cryptography;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace ClassLibrary.Services
{
    public class BingImageDownloadService
    {
        private static readonly object LockObject = new object();
        private static bool IsInitialized = false;
        private static ClassLibrary.Classes.BingImageSettingClass _bingImageSetting;
        public ClassLibrary.Classes.BingImageSettingClass BingImageSetting { get { return _bingImageSetting; } }
        /// <summary>
        /// 静态构造函数
        /// 静态构造函数只在类首次被使用时调用一次。
        /// 静态构造函数会在类首次被 加载到应用程序域 时调用一次。这意味着在整个应用程序域中，静态构造函数只会被调用一次。
        /// </summary>
        static BingImageDownloadService()
        {
            try
            {
                InitializeConfig();
            }
            catch (Exception ex)
            {
                string strTemp = string.Empty;
                // 处理配置文件读取错误
                strTemp = $"配置文件初始化失败，请检查异常原因。\r\n{ex.Message}\r\nError reading configuration.";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "必应每日壁纸下载配置参数初始化", strTemp, ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
            }
        }
        /// <summary>
        /// 处理配置文件读取
        /// 通过锁对象确保只能被一个线程执行
        /// </summary>
        private static void InitializeConfig()
        {
            lock (LockObject)
            {
                if (!IsInitialized)
                {
                    _bingImageSetting = ClassLibrary.ShareClass.GetBingImageSettingClass();
                    IsInitialized = true;
                }
            }
        }
        #region 锁对象
        /*
            lock 是 C# 中用来实现互斥（mutex）的一种关键字，用于保证在多线程环境下对共享资源的安全访问。下面详细解释它的含义和作用：

        1. 什么是 lock
            lock 是一种同步原语（synchronization primitive），用于确保一段代码在同一时刻只能被一个线程执行。它通过锁定一个对象来实现这一点。

        2. 语法
                    lock (object lockObject)
                    {
                        // 临界区代码
                    }
        3. 作用
            互斥访问：确保同一时刻只有一个线程能够进入临界区（critical section）。
            防止数据竞争：避免多个线程同时修改共享资源导致的数据不一致问题。
        4. 示例
            （略）
        5. 详细解释
            锁对象：_lockObject 是一个用于锁定的对象。通常建议使用私有字段来作为锁对象，以避免外部访问。
            临界区：lock (_lockObject) 语句块内的代码被称为临界区。这段代码在同一时刻只能被一个线程执行。
            线程安全：通过 lock 确保 _counter 的修改是线程安全的。
        6. 注意事项
            锁对象的选择：锁对象应该是不可变的，且不应被外部访问。通常使用私有字段作为锁对象。
            死锁风险：避免在 lock 内部调用其他可能锁定相同对象的方法，以防死锁。
            性能影响：频繁使用 lock 可能会影响性能，尤其是在高并发场景下。
         */
        #endregion

        /// <summary>
        /// 根据配置参数，整合下载地址等一些必要元素到字典中
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetDownloadUrlFromResultList()
        {
            Dictionary<string, List<string>> dicBingDownloadUrl = new Dictionary<string, List<string>>();

            // 全量下载
            if (BingImageSetting.BingImageApi.BingApiRequestParams.IsFullDownload)
            {
                // 先下载：今天往前的8张(idx=0 n=8)
                string strJson1 = GetJsonFromBingAPI(BingImageSetting, 0, 8);
                Dictionary<string, List<string>> dicSource1 = GetDownloadUrlFromBingJson(strJson1);

                //// 使用 Union 合并
                //// 异常消息：无法将类型为“<UnionIterator>d__67`1[System.Collections.Generic.KeyValuePair`2[System.String,System.Collections.Generic.List`1[System.String]]]”的对象强制转换为类型“System.Collections.Generic.Dictionary`2[System.String,System.Collections.Generic.List`1[System.String]]”
                //dicBingDownloadUrl = (Dictionary<string, List<string>>)dicBingDownloadUrl.Union(dicSource1);
                dicBingDownloadUrl = dicBingDownloadUrl.Concat(dicSource1)
                                                       .ToLookup(pair => pair.Key, pair => pair.Value)
                                                       .ToDictionary(group => group.Key, group => group.First());

                // 再下载：7天前往前的8张(idx=7 n=8)
                string strJson2 = GetJsonFromBingAPI(BingImageSetting, 7, 8);
                Dictionary<string, List<string>> dicSource2 = GetDownloadUrlFromBingJson(strJson2);
                // 使用 LINQ 合并两个字典
                dicBingDownloadUrl = dicBingDownloadUrl.Concat(dicSource2)
                                                       .ToLookup(pair => pair.Key, pair => pair.Value)
                                                       .ToDictionary(group => group.Key, group => group.First());
            }
            // 按指定下载
            else
            {
                string strJson3 = GetJsonFromBingAPI(BingImageSetting, BingImageSetting.BingImageApi.BingApiRequestParams.Query3Value, BingImageSetting.BingImageApi.BingApiRequestParams.Query4Value);
                Dictionary<string, List<string>> dicSource3 = GetDownloadUrlFromBingJson(strJson3);
                // 使用循环方法合并
                MergeDictionaries(dicBingDownloadUrl, dicSource3);
            }

            return dicBingDownloadUrl;
        }
        /// <summary>
        /// 从Bing的API中获取Json结果
        /// </summary>
        /// <param name="bingImageSetting"></param>
        /// <returns></returns>
        private string GetJsonFromBingAPI(ClassLibrary.Classes.BingImageSettingClass bingImageSetting, int intDaysAgo, int intAFewDays)
        {
            string strJson = string.Empty;

            UriBuilder uriBuilder = new UriBuilder(bingImageSetting.BingImageApi.BingApiRequestParams.Scheme, bingImageSetting.BingImageApi.BingApiRequestParams.Host, bingImageSetting.BingImageApi.BingApiRequestParams.Port, bingImageSetting.BingImageApi.BingApiRequestParams.Path);
            NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            queryParameters.Add(bingImageSetting.BingImageApi.BingApiRequestParams.Query1, bingImageSetting.BingImageApi.BingApiRequestParams.Query1Value1);
            queryParameters.Add(bingImageSetting.BingImageApi.BingApiRequestParams.Query2, bingImageSetting.BingImageApi.BingApiRequestParams.Query2Value);
            queryParameters.Add(bingImageSetting.BingImageApi.BingApiRequestParams.Query3, intDaysAgo.ToString());
            queryParameters.Add(bingImageSetting.BingImageApi.BingApiRequestParams.Query4, intAFewDays.ToString());
            queryParameters.Add(bingImageSetting.BingImageApi.BingApiRequestParams.Query5, bingImageSetting.BingImageApi.BingApiRequestParams.Query5Value);
            uriBuilder.Query = queryParameters.ToString();
            string strUrl = uriBuilder.Uri.ToString();

            //string strUrl = bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl1
            //			  + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl2
            //			  + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl3
            //			  + intDaysAgo
            //			  + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl4
            //			  + intAFewDays
            //			  + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl5;
            try
            {
                /*
				 注意事项：
						避免混合同步和异步代码: 尽量避免在异步方法中使用同步调用（如 GetResult 或 Result），以防止潜在的死锁问题。
						异常处理: 使用 try-catch 块来捕获和处理可能的异常。
				 */
                strJson = MyHttpServiceHelper.GetClient().GetStringAsync(strUrl).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
            }
            finally
            {
                #region 不需要在这里调用 DisposeClient
                //// 确保在 finally 块中调用 DisposeClient 方法
                //MyHttpServiceHelper.DisposeClient();
                /*
                 解释
                        单例模式管理 HttpClient：

                        使用单例模式管理 HttpClient，确保在整个应用程序生命周期内只有一个实例。
                        定期刷新 HttpClient 实例以保持其有效性。
                        在适当的位置调用 DisposeClient 方法：

                        在每个请求完成后不需要立即调用 DisposeClient，而是将其放在应用程序退出时调用。
                        这样可以避免频繁创建和销毁 HttpClient 对象带来的性能开销。
                        通过这种方式，你可以确保 HttpClient 的生命周期得到合理管理，同时避免不必要的性能损失。如果有其他问题，请随时提问。
                 */
                #endregion
            }
            return strJson;
        }
        /// <summary>
        /// 从必应Bing的Json数据中获取下载地址等一些必要元素
        /// </summary>
        /// <param name="strJson">从访问必应API地址中获得的Json数据</param>
        /// <returns>返回一个键值对集合</returns>
        /// 之所以要用Dictionary而不是List，主要是为了通过必应每日壁纸图片的日期enddate作为唯一键来记录，避免重复
        private Dictionary<string, List<string>> GetDownloadUrlFromBingJson(string strJson)
        {
            Dictionary<string, List<string>> dicEveryDayInformation = new Dictionary<string, List<string>>();
            try
            {
                ClassLibrary.Classes.BingClass bingclass = JsonConvert.DeserializeObject<ClassLibrary.Classes.BingClass>(strJson);
                if (!(bingclass is null))
                {
                    for (int i = 0; i < bingclass.images.Count; i++)
                    {
                        List<string> listInformation = new List<string>();
                        string strStart = "/th?id=OHR.";
                        string strEnd = "&rf=";

                        // [0]：原下载地址
                        string strUrl = string.Concat("https://www.bing.com", bingclass.images[i].url.Substring(0, bingclass.images[i].url.IndexOf(strEnd)));
                        listInformation.Add(strUrl);

                        // 必应每日壁纸下载地址： https://www.bing.com/th?id=OHR.MonsterDoor_ZH-CN6613337019_UHD.jpg&qlt=100
                        // [1]：高清电脑壁纸的下载地址 添加高清像素的分辨率(横版：landscape)
                        string strUrlIsUHD = strUrl.Substring(0, strUrl.LastIndexOf('_')) + "_UHD.jpg&qlt=100";
                        listInformation.Add(strUrlIsUHD);

                        // 必应每日壁纸下载地址： https://www.bing.com/th?id=OHR.MonsterDoor_ZH-CN6613337019_1080x1920.jpg&qlt=100
                        // [2]：手机壁纸的下载地址 添加手机壁纸(竖版：portrait)
                        string strUrlIsMobile = strUrl.Substring(0, strUrl.LastIndexOf('_')) + "_1080x1920.jpg&qlt=100";
                        listInformation.Add(strUrlIsMobile);

                        // [3]：原英文名称
                        string strEnglish = bingclass.images[i].url.Substring(strStart.Length, bingclass.images[i].url.IndexOf(strEnd) - strStart.Length);
                        listInformation.Add(strEnglish);

                        // [4]：加UHD作为高清电脑壁纸的英文名称
                        string strEnglishIsUHD = strEnglish.Substring(0, strEnglish.LastIndexOf('_')) + "_UHD.jpg";
                        listInformation.Add(strEnglishIsUHD);

                        // [5]：加分辨率作为手机壁纸的英文名称
                        string strEnglishIsMobile = strEnglish.Substring(0, strEnglish.LastIndexOf('_')) + "_1080x1920.jpg";
                        listInformation.Add(strEnglishIsMobile);

                        // [6]：提取版权信息copyright作为中文名称
                        string strChinese = bingclass.images[i].copyright.Substring(0, bingclass.images[i].copyright.IndexOf(" (© "));
                        listInformation.Add(strChinese);

                        // [7]：提取标题，看参数是否加入中文名称 因为说明也会加入到文件名中，所以也需要去除非法字符
                        listInformation.Add(bingclass.images[i].title);

                        // [8]：加UHD作为高清电脑壁纸的中文名称
                        string strChineseIsUHD = strChinese + "_UHD.jpg";
                        listInformation.Add(strChineseIsUHD);

                        // [9]：加分辨率作为手机壁纸的中文名称
                        string strChineseIsMobile = strChinese + "_1080x1920.jpg";
                        listInformation.Add(strChineseIsMobile);

                        // []：


                        dicEveryDayInformation.Add(bingclass.images[i].enddate, listInformation);
                    }
                }
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
            }
            return dicEveryDayInformation;
        }
        /// <summary>
        /// 使用循环方法合并
        /// </summary>
        /// <param name="target">目标字典</param>
        /// <param name="source">源字典</param>
        private void MergeDictionaries(Dictionary<string, List<string>> target, Dictionary<string, List<string>> source)
        {
            foreach (var pair in source)
            {
                if (!target.ContainsKey(pair.Key))
                {
                    // 不存在则添加
                    target.Add(pair.Key, pair.Value);
                }
                else
                {
                    // 更新已存在的键
                    target[pair.Key] = pair.Value;
                }
            }
        }

        #region 网络状态测试
        public bool NetworkStateTest()
        {
            // 定义配置文件路径和相关日志信息
            var logTitle = "网络状态测试(方法)";
            var logType = "NetworkStateTestService";
            var logCycle = "yyyyMMdd";

            bool result = false;
            DateTime dateTimeBingBegin = DateTime.Now;
            string strTemp = $"{dateTimeBingBegin}：开始网络状态测试";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            try
            {
                string dnsAddress = BingImageSetting.NetworkInformation.DnsServerAddress;
                string ipAddress = BingImageSetting.NetworkInformation.PingReplyDomain;

                // 网络是否通畅测试一
                IPAddress iPAddress = null;
                if (IPAddress.TryParse(dnsAddress, out iPAddress))
                {
                    if (ClassLibrary.ShareClass.IsNetworkAvailable(iPAddress))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                    strTemp = $"指定的DNS服务器地址是：{dnsAddress}\r\n连接结果：{result}";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "一", strTemp, logType, logCycle);
                }
                else
                {
                    strTemp = $"指定的DNS服务器地址{dnsAddress}非法";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "一", strTemp, logType, logCycle);
                }

                // 网络是否通畅测试二
                bool? netStatus;
                netStatus = ClassLibrary.ShareClass.IsNetworkAvailable(dnsAddress, BingImageSetting.NetworkInformation.SocketSendTimeout, BingImageSetting.NetworkInformation.SocketReceiveTimeout);
                if (netStatus is null)
                {
                    strTemp = $"传入地址{dnsAddress}有误。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "二", strTemp, logType, logCycle);
                }
                else if (netStatus == true)
                {
                    result = true;
                    strTemp = $"{dnsAddress}网络通畅。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "二", strTemp, logType, logCycle);
                }
                else if (netStatus == false)
                {
                    strTemp = $"{dnsAddress}网络不通。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "二", strTemp, logType, logCycle);
                }
                else
                {
                    strTemp = $"{dnsAddress}网络未知……";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "二", strTemp, logType, logCycle);
                }

                // 网络是否通畅测试三
                strTemp = $"通过发送ping指定地址({ipAddress})地址命令来测试网络是否通畅";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "三", strTemp, logType, logCycle);

                if (ShareClass.NetworkTest(ipAddress, BingImageSetting.NetworkInformation.NetRetryCount, BingImageSetting.NetworkInformation.NetWaitTime))
                {
                    result = true;
                    strTemp = $"通过发送ping指定地址({ipAddress})地址命令来测试网络的结果是：通畅";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "三", strTemp, logType, logCycle);
                }
                else
                {
                    strTemp = $"通过发送ping指定地址({ipAddress})地址命令来测试网络的结果是：失败";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle + "三", strTemp, logType, logCycle);
                }

            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                strTemp = $"程序发生错误：{ex.Message}";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                result = false;
            }

            strTemp = $"{DateTime.Now}：网络状态测试结束，耗时：{ClassLibrary.ShareClass.ToMinutesAgo(dateTimeBingBegin)}";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            return result;
        }
        #endregion

        #region 必应每日壁纸
        /// <summary>
        /// 通过传入必应Bing的下载元素，按配置参数下载相应的文件
        /// </summary>
        /// <param name="dicBingDownloadUrl"></param>
        /// <returns></returns>
        public bool BingImageDownloads(Dictionary<string, List<string>> dicBingDownloadUrl)
        {
            // 定义配置文件路径和相关日志信息
            var logTitle = "必应每日壁纸下载(方法)";
            var logType = "BingImageDownloadService";
            var logCycle = "yyyyMMdd";

            bool result = false;
            int intBingUHDCount = 0;
            int intBingMobileCount = 0;
            bool isDownloadExists = false;
            int intBingExistsCount = 0;
            DateTime dateTimeBingBegin = DateTime.Now;
            string strTemp = $"{dateTimeBingBegin}：开始下载必应每日壁纸";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            try
            {
                // 文件分类归档后存放的目录若不存在，则创建
                Directory.CreateDirectory(BingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath);
                Directory.CreateDirectory(BingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath);
                Directory.CreateDirectory(BingImageSetting.CategorizeAndMoveSetting.RejectedPath);
                Directory.CreateDirectory(BingImageSetting.ImageFileSavePath);

                foreach (KeyValuePair<string, List<string>> item in dicBingDownloadUrl)
                {
                    //string strUrl = item.Value[0];
                    string strUrlIsUHD = item.Value[1];
                    string strUrlISMobile = item.Value[2];
                    // 获取文件的Hash值
                    string strUHDHash = ClassLibrary.MyHashHelper.GetFileHashAsync_Url(strUrlIsUHD, SHA256.Create()).GetAwaiter().GetResult();
                    string strMobileHash = ClassLibrary.MyHashHelper.GetFileHashAsync_Url(strUrlISMobile, SHA256.Create()).GetAwaiter().GetResult();
                    //string strEnglish = item.Value[3];
                    string strEnglishIsUHD = ClassLibrary.ShareClass.RemoveInvalidChars(item.Value[4]);
                    string strEnglishIsMobile = ClassLibrary.ShareClass.RemoveInvalidChars(item.Value[5]);
                    //string strChinese = item.Value[6];
                    string strChineseIsUHD = ClassLibrary.ShareClass.RemoveInvalidChars(item.Value[8]);
                    string strChineseIsMobile = ClassLibrary.ShareClass.RemoveInvalidChars(item.Value[9]);

                    string strFileName = string.Empty;
                    if (BingImageSetting.BingImageApi.BingApiRequestParams.FileNameAddDate)
                    {
                        strFileName += ClassLibrary.ShareClass.RemoveInvalidChars(item.Key) + "_";
                    }
                    if (BingImageSetting.BingImageApi.BingApiRequestParams.FileNameAddTitle)
                    {
                        strFileName += ClassLibrary.ShareClass.RemoveInvalidChars(item.Value[7]) + "_";
                    }


                    if (BingImageSetting.BingImageApi.BingApiRequestParams.FileNameLanguageIsChinese)
                    {
                        if (BingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsUHD)
                        {
                            string strFileNameNew = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath, strFileName + strChineseIsUHD);

                            if (ClassLibrary.MyMethodExtension.FileExistsByHash(strUHDHash) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"SHA256：{strUHDHash}\r\n{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlIsUHD, strFileNameNew);
                                ClassLibrary.MyMethodExtension.InsertHashData(strFileNameNew, strUHDHash, 0);
                                strTemp = $"{strUrlIsUHD}\r\nSHA256：{strUHDHash}\r\n{strFileNameNew}\r\n文件下载结果：{result}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                intBingUHDCount++;
                            }
                        }
                        if (BingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsMobile)
                        {
                            string strFileNameNew = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath, strFileName + strChineseIsMobile);

                            if (ClassLibrary.MyMethodExtension.FileExistsByHash(strMobileHash) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"SHA256：{strMobileHash}\r\n{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlISMobile, strFileNameNew);
                                ClassLibrary.MyMethodExtension.InsertHashData(strFileNameNew, strMobileHash, 1);
                                strTemp = $"{strUrlISMobile}\r\nSHA256：{strMobileHash}\r\n{strFileNameNew}\r\n文件下载结果：{result}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                intBingMobileCount++;
                            }
                        }
                    }
                    else
                    {
                        if (BingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsUHD)
                        {
                            string strFileNameNew = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath, strFileName + strEnglishIsUHD);
                            if (ClassLibrary.MyMethodExtension.FileExistsByHash(strUHDHash) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"SHA256：{strUHDHash}\r\n{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlIsUHD, strFileNameNew);
                                ClassLibrary.MyMethodExtension.InsertHashData(strFileNameNew, strUHDHash, 0);
                                strTemp = $"{strUrlIsUHD}\r\nSHA256：{strUHDHash}\r\n{strFileNameNew}\r\n文件下载结果：{result}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                intBingUHDCount++;
                            }
                        }
                        if (BingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsMobile)
                        {
                            string strFileNameNew = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath, strFileName + strEnglishIsMobile);
                            if (ClassLibrary.MyMethodExtension.FileExistsByHash(strMobileHash) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"SHA256：{strMobileHash}\r\n{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlISMobile, strFileNameNew);
                                ClassLibrary.MyMethodExtension.InsertHashData(strFileNameNew, strMobileHash, 1);
                                strTemp = $"{strUrlISMobile}\r\nSHA256：{strMobileHash}\r\n{strFileNameNew}\r\n文件下载结果：{result}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                intBingMobileCount++;
                            }
                        }
                    }
                }

                if ((intBingUHDCount + intBingMobileCount) > 0)
                {
                    strTemp = $"{intBingUHDCount + intBingMobileCount} 个文件下载完毕，可到你指定的文件夹中查看。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                }
                else if (isDownloadExists)
                {
                    strTemp = $" {intBingExistsCount} 个文件已存在，根据参数配置不重新下载。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                }
                else
                {
                    strTemp = $"文件下载出错，请到程序目录的Log文件中查看日志文件，分析出错原因。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                }

                strTemp = string.Format($"共下载Bing每日图片：电脑壁纸 {intBingUHDCount} 个文件，手机壁纸 {intBingMobileCount} 个文件。");
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                strTemp = $"程序发生错误：{ex.Message}";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
            }

            strTemp = $"{DateTime.Now}：下载必应每日壁纸结束，耗时：{ClassLibrary.ShareClass.ToMinutesAgo(dateTimeBingBegin)}";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            strTemp = $"\r\n【版权】仅限于壁纸使用。它们是受版权保护的图像，因此您不应将其用于其他目的，但可以将其用作桌面壁纸。\r\n ";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(strTemp);
            Console.ResetColor();
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            return result;
        }
        #endregion

        #region 复制Windows聚焦图片
        /// <summary>
        /// Windows聚焦图片复制
        /// </summary>
        /// <returns></returns>
        public bool WindowsSpotlightCopy()
        {
            // 定义配置文件路径和相关日志信息
            var logTitle = "Windows聚焦图片复制(方法)";
            var logType = "WindowsSpotlightCopyService";
            var logCycle = "yyyyMMdd";

            bool result = false;
            int intSpotlightCount = 0;
            DateTime dateTimeBingBegin = DateTime.Now;
            string strTemp = $"{dateTimeBingBegin}：开始Windows聚焦图片复制";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            try
            {
                string strPath = BingImageSetting.ImageFileSavePath;
                string strSourcePath = BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightPath;
                int intSpotlightCopyCount = 0;
                bool isExists = false;

                Stopwatch stopwatchSpotlight = new Stopwatch();
                stopwatchSpotlight.Start();
                if (Directory.Exists(strSourcePath))
                {
                    Directory.GetFiles(strSourcePath, "*", SearchOption.TopDirectoryOnly).ToList().ForEach(x =>
                    {
                        string strFileHash = ClassLibrary.MyHashHelper.GetFileHashAsync_Local(x, SHA256.Create()).GetAwaiter().GetResult();
                        string strFileName = Path.GetFileName(x) + ".png";
                        string strDownloadFile = Path.Combine(strPath, strFileName);
                        if (!BingImageSetting.Overwrite && ClassLibrary.MyMethodExtension.FileExistsByHash(strFileHash))
                        {
                            // 因为现在改为通过Hash值来判断，这里的文件路径strDownloadFile是旧的以文件名来判断时的留存取值，所以可能并不准确。
                            strTemp = $"SHA256：{strFileHash}\r\n{strDownloadFile}\r\n文件已存在，根据参数配置不重新复制。";
                            Console.WriteLine(strTemp);
                            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                            intSpotlightCopyCount--;
                            isExists = true;
                        }
                        else
                        {
                            if (new FileInfo(x).Length > (1024 * 50))
                            {
                                using (FileStream fsRead = new FileStream(x, FileMode.Open))
                                {
                                    byte[] buffer = new byte[1024 * 1024 * 1];
                                    using (FileStream fsWrite = new FileStream(strDownloadFile, FileMode.Create))
                                    {
                                        while (true)
                                        {
                                            int i = fsRead.Read(buffer, 0, buffer.Length);
                                            if (i > 0)
                                            {
                                                fsWrite.Write(buffer, 0, i);
                                            }
                                            else
                                            {
                                                //// 复制聚焦图片时，不做记录，不然分类归档时就被拒了。
                                                //ClassLibrary.MyMethodExtension.InsertHashData(strDownloadFile, strFileHash);
                                                strTemp = $"SHA256：{strFileHash}\r\n{strDownloadFile}\r\n复制完成。";
                                                Console.WriteLine(strTemp);
                                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        intSpotlightCount++;
                        intSpotlightCopyCount++;
                    });
                }
                else
                {
                    strTemp = $"目录{strSourcePath}不存在，请检查配置。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                }
                stopwatchSpotlight.Stop();

                if (intSpotlightCopyCount > 0 || isExists)
                {
                    strTemp = string.Format($"Windows聚焦图片复制任务完成，共{intSpotlightCount}个文件，复制了{intSpotlightCopyCount}文件，总耗时{ClassLibrary.ShareClass.MillisecondConversion((int)stopwatchSpotlight.Elapsed.TotalMilliseconds)}。");
                }
                else
                {
                    strTemp = $"Windows聚焦目录\r\n{strSourcePath}\r\n图片不存在，请检查你的操作系统是否支持锁屏设置为Windows聚焦。";
                }
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                result = true;
            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                strTemp = $"程序发生错误：{ex.Message}";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                result = false;
            }

            strTemp = $"{DateTime.Now}：Windows聚焦图片复制结束，耗时：{ClassLibrary.ShareClass.ToMinutesAgo(dateTimeBingBegin)}";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            return result;
        }
        #endregion

        #region Windows聚集API图片下载
        // Windows聚集API图片下载Json处理
        // 访问Windows聚焦API图片地址：
        // 得到的JSON数据中包含了另一个带转义符的Json数据，所以需要单独处理，以获取真正想要的Windows聚焦API图片下载地址

        /// <summary>
        /// 配置序列化选项，禁用 HTML 转义
        /// </summary>
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            // - 使生成的JSON字符串有缩进格式，便于阅读
            Formatting = Newtonsoft.Json.Formatting.Indented,
            // - 对HTML字符进行转义处理，防止XSS攻击
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
        };
        /// <summary>
        /// 从 JSON 字符串中提取 Windows Spotlight 图片下载地址，提取指定的 JSON 节点并生成新的 JSON 字符串。。
        /// </summary>
        /// <param name="jsonString">输入的 JSON 字符串。</param>
        /// <returns>返回包含所有item处理结果的JSON字符串，如果未找到则返回 null。</returns>
        public static string ExtractJsonFromWindowsSpotlight(string jsonString)
        {
            // 用于存储临时日志信息的字符串
            string strTemp = string.Empty;
            // 初始化结果集合
            JArray resultArray = new JArray();
            try
            {
                // 1. 解析输入的JSON字符串为JObject对象
                var jsonObject = JObject.Parse(jsonString);

                // 2. 检查JSON对象中是否包含"batchrsp"键
                if (jsonObject.ContainsKey("batchrsp"))
                {
                    // 3. 获取batchrsp对象并转换为JObject
                    var batchrspElement = jsonObject["batchrsp"] as JObject;

                    // 4. 检查batchrsp对象中是否包含"items"键
                    if (batchrspElement != null && batchrspElement.ContainsKey("items"))
                    {
                        // 5. 获取items数组并转换为JArray
                        var itemsElement = batchrspElement["items"] as JArray;

                        if (itemsElement != null)
                        {
                            // 6. 遍历items数组中的每个元素
                            foreach (var itemElement in itemsElement)
                            {
                                // 7. 检查当前元素是否为JObject且包含"item"键
                                if (itemElement is JObject itemObject && itemObject.ContainsKey("item"))
                                {
                                    // 8. 获取item键的值并转换为字符串
                                    var innerJsonString = itemElement["item"].ToString();
                                    // 9. 解析内部的JSON字符串为JObject
                                    var innerJsonObject = JObject.Parse(innerJsonString);

                                    // 10. 创建新的JObject来存储提取的节点
                                    var newJsonObject = new JObject();
                                    // 11. 定义需要从ad节点中提取的字段路径
                                    var nodesToExtract = new[]
                                    {
                                        "landscapeImage", // 横向图片URL
										"portraitImage",  // 纵向图片URL
										"iconHoverText",                 // 热点1标题文本
										"title"                      // 主标题文本
									};

                                    // 12. 遍历需要提取的节点路径（也就是从原始Json文件中找"ad"节点，然后从"ad"节点中循环找nodesToExtract相的路径，提取其下的json数据）
                                    foreach (var nodePath in nodesToExtract)
                                    {
                                        // 13. 从ad节点中获取指定路径的token
                                        var token = innerJsonObject.SelectToken("ad").SelectToken(nodePath);

                                        if (token != null)
                                        {
                                            // 14. 将获取到的token添加到新JObject中
                                            newJsonObject[nodePath] = token;
                                        }
                                    }

                                    // 15. 将新JObject添加到结果集合中
                                    resultArray.Add(newJsonObject);
                                }
                            }
                        }
                        else
                        {
                            // 17. 记录错误日志：items不是数组
                            strTemp = $"JSON 数据中的 'items' 不是数组。";
                            Console.WriteLine(strTemp);
                            ClassLibrary.MyLogHelper.LogSplit(strTemp);
                        }
                    }
                    else
                    {
                        // 18. 记录错误日志：未找到items键
                        strTemp = $"JSON 数据中未找到 'items' 键。";
                        Console.WriteLine(strTemp);
                        ClassLibrary.MyLogHelper.LogSplit(strTemp);
                    }
                }
                else
                {
                    // 19. 记录错误日志：未找到batchrsp键
                    strTemp = $"JSON 数据中未找到 'batchrsp' 键。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(strTemp);
                }
            }
            catch (Exception ex)
            {
                // 20. 捕获并记录JSON解析过程中的异常
                strTemp = $"解析 JSON 时发生错误: {ex.Message}";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                ClassLibrary.MyLogHelper.LogSplit(strTemp);
            }

            // 21. 如果出现任何错误或未找到所需数据，返回null
            return resultArray.Count > 0 ? JsonConvert.SerializeObject(resultArray, _jsonSerializerSettings) : null;
        }


        /// <summary>
        /// Windows聚焦API图片下载
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WindowsSpotlightDownloadAsync()
        {// 定义配置文件路径和相关日志信息
            var logTitle = "Windows聚焦API图片下载(方法)";
            var logType = "WindowsSpotlightDownloadService";
            var logCycle = "yyyyMMdd";
            int intDownloadCount = 0;

            // // 过期失效
            //UriBuilder uriBuilder = new UriBuilder(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Scheme, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Host, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Port, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Path);
            //NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            //queryParameters.Add(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query1, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query1Value2);
            //queryParameters.Add(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query2, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query2Value1);
            //queryParameters.Add(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query3, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query3Value);
            //queryParameters.Add(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query4, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query4Value);
            //queryParameters.Add(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query5, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query4Value);
            //queryParameters.Add(BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query6, BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl.Query6Value);
            //uriBuilder.Query = queryParameters.ToString();
            //string strUrl = uriBuilder.Uri.ToString();

            // 2025-06-13 帮助文档：https://github.com/Biswa96/WinLight/blob/master/Developers.md
            // 2025-06-13 换新的地址：https://fd.api.iris.microsoft.com/v4/api/selection?placement=88000820&bcnt=4&fmt=json&locale=zh-CN&country=CN
            string strUrl = BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIUrl;

            bool result = false;
            DateTime dateTimeSpotlightBegin = DateTime.Now;
            string strTemp = $"{dateTimeSpotlightBegin}：开始下载Windows聚焦API图片";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);


            try
            {
                int loopLimit = 0;
                int loopCount = 0;
                while (loopLimit < BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIRepeatLimit)
                {
                    // 获取下载地址等素材
                    string strJson = await MyHttpServiceHelper.GetClient().GetStringAsync(strUrl);
                    //// 老的过期Json解析
                    // string newJson = ClassLibrary.MyJsonHelper.ExtractWindowsSpotlightDownloadUrl(strJson);
                    // 新的20250613 Json 解析
                    string newJson = ExtractJsonFromWindowsSpotlight(strJson);

                    if (string.IsNullOrWhiteSpace(newJson))
                    {
                        Console.WriteLine($"未获取到有效Json数据，请检查API访问地址是否有效。\r\n{strUrl}");
                        break;
                    }
                    // // 老的过期的Json对应的类
                    // WindowsSpotlightClass windowsSpotlight = ClassLibrary.MyJsonHelper.ReadJsonToClass<WindowsSpotlightClass>(newJson);
                    // 20250613 新的Json对应的类
                    var windowsSpotlightNew = new WindowsSpotlightNewClass();
                    windowsSpotlightNew.Items = ClassLibrary.MyJsonHelper.ReadJsonToClass<List<WindowsSpotlightItem>>(newJson);
                    #region 20250613 新的Json对应的类
                    /*
								 因为新的Json获取到的数据长这样：
										[
										  {
											"landscapeImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/c40575b0-d6ee-fbbe-cdd6-47bc3b1cfa22_desktop-b005_africanpenguincapetownafrica_adobestock_231838564_3840x2160_1690679080489.jpg"
											},
											"portraitImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/62b12dbf-03b1-f348-ebc2-d7c4bc48d065_desktop-b005_africanpenguincapetownafrica_adobestock_231838564_1080x1920_1690679080488.jpg"
											},
											"iconHoverText": "南非，博尔德斯海滩上的非洲企鹅\r\n© Uryadnikov Sergey / Adobe Stock\r\n点击鼠标右键以了解更多信息",
											"title": "嘿，邻居们"
										  },
										  {
											"landscapeImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/c97ba09e-d6aa-5a19-05fe-038e8bf13c54_desktop-b005_meerkatsouthafrica_adobestock_222922405_3840x2160_1690679080503.jpg"
											},
											"portraitImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/c06006c8-9c0e-9261-b012-84e4773d83b7_desktop-b005_meerkatsouthafrica_adobestock_222922405_1080x1920_1690679080503.jpg"
											},
											"iconHoverText": "南非的猫鼬\r\n© byrdyak / Adobe Stock\r\n点击鼠标右键以了解更多信息",
											"title": "小动物，大嘴巴"
										  },
										  {
											"landscapeImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/5806815a-5252-8303-d71e-7abe8a953cb7_desktop-b001_pierseascapekorinthia_gettyimages-995463206_3840x2160_1691072615241.jpg"
											},
											"portraitImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/c44b4729-32f5-c3ce-c9fa-16cbe20d440a_desktop-b001_pierseascapekorinthia_gettyimages-995463206_1080x1920_1691072615239.jpg"
											},
											"iconHoverText": "希腊，科林西亚的一个码头\r\n© Elber / 500px\r\n点击鼠标右键以了解更多信息",
											"title": "眺望平静的海面"
										  },
										  {
											"landscapeImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/3229fd93-4fc2-08b5-aea7-b3ae3fb21f0c_desktop-008_portholeiceland_gettyimages-1404003974_3840x2160.jpg"
											},
											"portraitImage": {
											  "asset": "https://res.public.onecdn.static.microsoft/creativeservice/49d1c206-3680-ffc0-fb3a-3b0d9294392f_desktop-008_portholeiceland_gettyimages-1404003974_1080x1920.jpg"
											},
											"iconHoverText": "冰岛，冰川泻湖杰古沙龙湖上的冰山\r\n© Marco Bottigelli / Moment / Getty Images\r\n点击鼠标右键以了解更多信息",
											"title": "蓝色冰块，粉色天空"
										  }
										]
								所以：
									错误信息表明JSON数据是一个数组，但您尝试将其反序列化为WindowsSpotlightNewClass对象。解决方案有两种：

									1. 修改JSON数据格式为对象形式（包含Items属性）：
									```
									{
			  						  "Items": [
			    						// 原数组内容
			  						  ]
									}
									```
									2. 或者修改类定义，直接反序列化为List
									   ：
									```
									var result = JsonConvert.DeserializeObject<List<WindowsSpotlightItem>>
									(jsonString);
									```
									推荐使用第二种方法，因为它更直接匹配JSON数据的实际结构。

								 */
                    #endregion

#if DEBUG
                    File.WriteAllText(Path.Combine(ClassLibrary.ShareClass._logPath, $"{DateTime.Now.ToString("yyyyMMddHHmmssfffffff")}.json"), strJson, Encoding.UTF8);
                    //// 老
                    // ClassLibrary.MyJsonHelper.WriteClassToJsonFile<WindowsSpotlightClass>(windowsSpotlight, Path.Combine(ClassLibrary.ShareClass._logPath, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.json"));
                    // 新20250613
                    ClassLibrary.MyJsonHelper.WriteClassToJsonFile<WindowsSpotlightNewClass>(windowsSpotlightNew, Path.Combine(ClassLibrary.ShareClass._logPath, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.json"));
#else
#endif
                    if (windowsSpotlightNew.Items.Count > 0)
                    {
                        foreach (var item in windowsSpotlightNew.Items)
                        {
                            int count = 0;
                            result = false;
                            bool resultIsRepeat = false;
                            // 准备开始下载
                            // Page Orientation即页面方向，指的是矩形平面以长边为底还是以短边为底，分为 Portrait（纵向<Portrait指的是人物画像>）和 Landscape（横向<Landscape指的则是风景画>）两类。

                            var iconHoverTextTemp = item.IconHoverText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            string assetTemp = string.Empty;

                            // 电脑壁纸
                            bool landscapeIsRepeat = false;
                            bool resultLandscape = true;
                            //var assetArray = item.LandscapeImage.Asset.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                            //foreach (var asset in assetArray)
                            //{
                            //	if (asset.Contains("x"))
                            //	{
                            //		assetTemp = asset;
                            //	}
                            //}
                            //assetTemp = assetArray.FirstOrDefault(s => s.Contains("x"));
                            string strFileDownloadUrl = item.LandscapeImage.Asset;
                            assetTemp = Regex.Match(strFileDownloadUrl, @"\d+x\d+").Value;
                            // Regex.Match 方法只会返回第一个匹配项。
                            // 如果你想获取所有匹配项：
                            // 可以改用 Regex.Matches，如下所示：

                            // csharp
                            // MatchCollection matches = Regex.Matches(strTemp, @"\d+x\d+");
                            // List<string> results = matches.Cast<Match>().Select(m => m.Value).ToList();

                            string strFileNameLandscape = $"{DateTime.Now.ToString("yyyyMMdd")}_{item.Title}_{iconHoverTextTemp[0]}_{assetTemp}.jpg";
                            strFileNameLandscape = ClassLibrary.ShareClass.RemoveInvalidChars(strFileNameLandscape);
                            strFileNameLandscape = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath, strFileNameLandscape);
                            string strFileHashLandscape = ClassLibrary.MyHashHelper.GetFileHashAsync_Url(strFileDownloadUrl, SHA256.Create()).GetAwaiter().GetResult();
                            if (!BingImageSetting.Overwrite && ClassLibrary.MyMethodExtension.FileExistsByHash(strFileHashLandscape))
                            {
                                strTemp = $"SHA256：{strFileHashLandscape}\r\n{strFileNameLandscape}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                landscapeIsRepeat = true;
                            }
                            else
                            {
                                resultLandscape = ClassLibrary.ShareClass.DownloadFile(strFileDownloadUrl, strFileNameLandscape);
                                ClassLibrary.MyMethodExtension.InsertHashData(strFileNameLandscape, strFileHashLandscape, 0);
                                strTemp = $"{strFileDownloadUrl}\r\nSHA256：{strFileHashLandscape}\r\n{strFileNameLandscape}\r\n文件下载结果：{resultLandscape}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                if (resultLandscape) { intDownloadCount++; count++; }
                            }

                            // 手机壁纸
                            bool portraitIsRepeat = false;
                            bool resultPortrait = true;
                            strFileDownloadUrl = item.PortraitImage.Asset;
                            assetTemp = Regex.Match(strFileDownloadUrl, @"\d+x\d+").Value;
                            string strFileNamePortrait = $"{DateTime.Now.ToString("yyyyMMdd")}_{item.Title}_{iconHoverTextTemp[0]}_{assetTemp}.jpg";
                            strFileNamePortrait = ClassLibrary.ShareClass.RemoveInvalidChars(strFileNamePortrait);
                            strFileNamePortrait = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath, strFileNamePortrait);
                            string strFileHashPortrait = ClassLibrary.MyHashHelper.GetFileHashAsync_Url(strFileDownloadUrl, SHA256.Create()).GetAwaiter().GetResult();
                            if (!BingImageSetting.Overwrite && ClassLibrary.MyMethodExtension.FileExistsByHash(strFileHashPortrait))
                            {
                                strTemp = $"SHA256：{strFileHashPortrait}\r\n{strFileNamePortrait}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                portraitIsRepeat = true;
                            }
                            else
                            {
                                resultPortrait = ClassLibrary.ShareClass.DownloadFile(strFileDownloadUrl, strFileNamePortrait);
                                ClassLibrary.MyMethodExtension.InsertHashData(strFileNamePortrait, strFileHashPortrait, 1);
                                strTemp = $"{strFileDownloadUrl}\r\nSHA256：{strFileHashPortrait}\r\n{strFileNamePortrait}\r\n文件下载结果：{resultPortrait}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                if (resultPortrait) { intDownloadCount++; count++; }
                            }


                            // 记录结果
                            loopCount++;
                            resultIsRepeat = (landscapeIsRepeat && portraitIsRepeat);
                            if (resultIsRepeat)
                            {
                                loopLimit++;
                            }
                            else
                            {
                                // 按必须连续的方式
                                loopLimit = 0;

                                //// 按倒减的方式
                                //	if (loopLimit > 0)
                                //	{
                                //		loopLimit--;
                                //	}
                            }
                            result = (resultLandscape && resultPortrait);
                            int intResult = loopLimit < BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIRepeatLimit ? count : intDownloadCount;
                            string strTest = loopLimit < BingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIRepeatLimit ? "当前" : "共";
                            strTemp = $"Windows聚焦API图片下载完成({result})重复{loopCount.ToString()}-{loopLimit.ToString()}：{strTest}下载{intResult}个文件。";
                            Console.WriteLine(strTemp);
                            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                        }
                    }
                    else
                    {
                        strTemp = $"Windows聚焦API图片Json数据获取失败，windowsSpotlightNew.Items的数量为{windowsSpotlightNew.Items.Count}";
                        Console.WriteLine(strTemp);
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                    }
                }
            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
            }

            strTemp = $"{DateTime.Now}：Windows聚焦API图片下载结束，耗时：{ClassLibrary.ShareClass.ToMinutesAgo(dateTimeSpotlightBegin)}";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            return result;
        }

        #endregion

        #region 文件分类归档
        /// <summary>
        /// 文件分类归档移动(通过文件名来判断目标文件是否存在)
        /// </summary>
        /// <returns></returns>
        public bool CategorizeAndMoveFile()
        {
            // 定义配置文件路径和相关日志信息
            var logTitle = "文件分类归档(方法)";
            var logType = "CategorizeAndMoveFileService";
            var logCycle = "yyyyMMdd";

            bool result = false;
            DateTime dateTimeBingBegin = DateTime.Now;
            string strTemp = $"{dateTimeBingBegin}：开始文件分类归档";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            try
            {
                //// 用图片所在的源目录路径作为日志文件的文件名
                //try
                //{
                //    //strLogType = Path.GetFileName(Path.GetDirectoryName(imageSourcePath));
                //    strLogType = ClassLibrary.ShareClass.RemoveInvalidChars(imageSourcePath, "_");
                //}
                //catch (Exception ex)
                //{
                //    //strLogType = "none";
                //    ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                //}

                // 文件分类归档后存放的目录若不存在，则创建
                Directory.CreateDirectory(BingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath);
                Directory.CreateDirectory(BingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath);
                Directory.CreateDirectory(BingImageSetting.CategorizeAndMoveSetting.RejectedPath);
                Directory.CreateDirectory(BingImageSetting.ImageFileSavePath);

                if (!Directory.Exists(BingImageSetting.CategorizeAndMoveSetting.SearchDirectoryPath))
                {
                    strTemp = $"文件所在路径：\r\n{BingImageSetting.CategorizeAndMoveSetting.SearchDirectoryPath}\r\n源目录不存在，请检查相关配置。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                    result = false;
                }
                else
                {
                    int allFilesCount = 0;
                    int processedFilesCount = 0;

                    foreach (string filePath in Directory.GetFiles(BingImageSetting.CategorizeAndMoveSetting.SearchDirectoryPath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        allFilesCount++;
                        string strFileHash = ClassLibrary.MyHashHelper.GetFileHashAsync_Local(filePath, SHA256.Create()).GetAwaiter().GetResult();
                        if (!BingImageSetting.Overwrite && ClassLibrary.MyMethodExtension.FileExistsByHash(strFileHash))
                        {
                            strTemp = $"SHA256：{strFileHash}\r\n{fileName}\r\n文件已存在，根据参数配置不重新分类归档。";
                            Console.WriteLine(strTemp);
                            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                        }
                        else
                        {
                            string fileExtension = Path.GetExtension(filePath);
                            string fileNewName = fileName;

                            if (string.IsNullOrEmpty(fileExtension))
                            {
                                fileNewName += ".png";
                                //resultMessage = $"修改文件名：{fileNewName}";
                                //ClassLibrary.MyLogHelper.LogSplit(strLogTitle, resultMessage, strLogType);
                            }
                            //如果有后缀名，且后缀名也是指定的图片格式
                            // Ensure the file has a valid image extension.
                            else if (!string.IsNullOrEmpty(fileExtension) && ClassLibrary.ShareClass.ValidImageExtensions.Contains(fileExtension.ToLower()))
                            {
                                fileNewName = fileName;
                            }
                            //否则，可能不是图片文件，那就跳过
                            else
                            {
                                continue;
                            }

                            #region 老代码会导致：内存不足
                            //由于System.Drawing.Image.FromFile()方法在加载图像时会将整个图像内容加载到内存中。
                            //当处理大量大尺寸图像时，尤其是系统资源有限的情况下，内存可能会耗尽。
                            //为避免这种问题，你可以考虑以下优化策略：
                            //分批处理：不要一次性加载所有文件，而是可以使用批处理逻辑，每次只处理一定数量的文件，然后释放内存。
                            //流式处理：使用文件流读取和处理图像，而不是一次性加载整个文件。例如，使用System.IO.FileStream和System.Drawing.Imaging.ImageFormat类来读取和保存图像，这样可以减少内存占用。
                            //使用更高效库：考虑使用如ImageSharp或SkiaSharp这样的现代图像处理库，它们通常有更好的内存管理和性能。
                            //优化图像大小：在加载图像之前，先检查文件大小，对过大图像进行压缩或缩小，以减少内存消耗。
                            //增强系统资源：确保运行该方法的机器有足够的内存和CPU资源来处理大量图像。
                            //异常处理：在调用System.Drawing.Image.FromFile()时，添加异常处理代码，以便在内存不足时能优雅地处理错误，例如记录日志并继续处理其他文件。

                            /*
                            string targetFilePath;
                            using (System.Drawing.Image image = System.Drawing.Image.FromFile(filePath))
                            {
                                int width = image.Width;
                                int height = image.Height;

                                resultMessage = $"{fileNewName} 的像素为：{width} x {height}";
                                ClassLibrary.MyLogHelper.LogSplit(strLogTitle, resultMessage, strLogType);
                                //如果图片像素过小，则有可能并不适合作为壁纸，应该抛弃
                                if (width < 500 || height < 500)
                                {
                                    targetFilePath = Path.Combine(rejectedPath, fileName);
                                }
                                else
                                {
                                    if (IsLandscapeImage(width, height))
                                    {
                                        targetFilePath = Path.Combine(computerTargetPath, fileNewName);
                                    }
                                    else
                                    {
                                        targetFilePath = Path.Combine(mobileTargetPath, fileNewName);
                                    }
                                }
                            }
                            */
                            #endregion

                            string targetFilePath;
                            // 图片的方向是否是竖向的（对应数据库中：横向：0 否，竖向：1 是）
                            int isPortrait = 0;

                            // 使用 BitmapDecoder 获取图像尺寸，避免加载整个图像到内存
                            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                // 需在项目-引用处添加程序集-框架-WindowsBase： using System.Windows.Media.Imaging;
                                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                                int width = (int)decoder.Frames[0].PixelWidth;
                                int height = (int)decoder.Frames[0].PixelHeight;

                                strTemp = $"{fileNewName} 的像素为：{width} x {height}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

                                if (width < 500 || height < 500)
                                {
                                    targetFilePath = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.RejectedPath, fileName);
                                }
                                else if (ClassLibrary.ShareClass.IsLandscapeImage(width, height))
                                {
                                    targetFilePath = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath, fileNewName);
                                }
                                else
                                {
                                    targetFilePath = Path.Combine(BingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath, fileNewName);
                                    isPortrait = 1;
                                }
                            }

                            #region 在外层就用Hash判断文件是否已存在，所以这里的判断注释掉不执行
                            //// 根据需要处理文件已经存在的情况。
                            //// Handle file already exists situation based on your requirements.
                            //// 例如，您可以选择覆盖、跳过或重命名文件。
                            //// For example, you can choose to overwrite, skip, or rename the file.
                            //if (File.Exists(targetFilePath))
                            //{
                            //    // 执行适当的处理策略。
                            //    // TODO: Implement appropriate handling strategy.
                            //    strTemp = $"未移动：{fileNewName} 在 {targetFilePath} 中已存在";
                            //    Console.WriteLine(strTemp);
                            //    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                            //    continue;
                            //} 
                            #endregion

                            //File.Move(filePath, targetFilePath);
                            try
                            {
                                string strFileOperationType = string.Empty;
                                bool isRename = false;
                                string directory = Path.GetDirectoryName(targetFilePath);
                                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetFilePath);
                                // 返回结果是包含 . 的，如果需要不带 . 的扩展名（例如 "txt"），可以使用 .TrimStart('.')。
                                // 如果处理的是用户输入或不确定格式的路径，请先用 Path.HasExtension() 判断是否存在扩展名。
                                string extension = Path.GetExtension(targetFilePath);
                                // 防止异常消息一：当文件已存在时，无法创建该文件
                                // 生成新的目标路径，直到找到一个不存在的文件名
                                while (File.Exists(targetFilePath))
                                {
                                    targetFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{extension}");
                                    isRename = true;
                                }

                                switch (BingImageSetting.CategorizeAndMoveSetting.FileOperationType)
                                {
                                    case ClassLibrary.ShareClass.FileOperationType.Move:
                                        // 防止异常消息二：当文件已存在时，无法创建该文件
                                        if (File.Exists(targetFilePath))
                                        {
                                            // 删除已存在的目标文件
                                            File.Delete(targetFilePath);
                                        }
                                        File.Move(filePath, targetFilePath);
                                        strFileOperationType = isRename ? "移动并重命名为" : "移动到";
                                        break;
                                    case ClassLibrary.ShareClass.FileOperationType.Copy:
                                        // 防止异常消息二：当文件已存在时，无法创建该文件
                                        //File.Copy(filePath, targetFilePath);
                                        // 覆盖已存在的文件
                                        File.Copy(filePath, targetFilePath, true);
                                        strFileOperationType = isRename ? "复制并重命名为" : "复制到";
                                        break;
                                    default:
                                        throw new ArgumentException("指定的文件操作类型无效。\r\nInvalid file operation type specified.", nameof(BingImageSetting.CategorizeAndMoveSetting.FileOperationType));
                                }
                                ClassLibrary.MyMethodExtension.InsertHashData(targetFilePath, strFileHash, isPortrait);
                                processedFilesCount++;
                                strTemp = $"{processedFilesCount}：{fileNewName} {strFileOperationType} {targetFilePath}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                            }
                            catch (Exception ex)
                            {
                                result = false;
                                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                                strTemp = $"处理文件时，程序发生错误：{ex.Message}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                            }
                        }
                    }
                    result = true;
                    strTemp = $"指定目录：{BingImageSetting.CategorizeAndMoveSetting.SearchDirectoryPath}\r\n共处理了{allFilesCount} - {processedFilesCount} 个文件。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                }
            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                strTemp = $"程序发生错误：{ex.Message}";
                Console.WriteLine(strTemp);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                result = false;
            }


            strTemp = $"{DateTime.Now}：文件分类归档结束，耗时：{ClassLibrary.ShareClass.ToMinutesAgo(dateTimeBingBegin)}";
            Console.WriteLine(strTemp);
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);

            // 任务结束，做个数据统计，记录到日志文件中
            ClassLibrary.MySQLiteHelper mySQLite = new MySQLiteHelper(ClassLibrary.ShareClass._sqliteConnectionString);
            string sqliteScript_FileHashes = $@"SELECT STRFTIME('%Y-%m-%d',datetime(CreatedTime)) AS CreatedDay,COUNT(1) AS Count FROM FileHashes WHERE 1=1 AND CreatedDay = date('now') GROUP BY CreatedDay;";
            // string sqliteScript_FileHashes = $@"SELECT COUNT(1) Count FROM FileHashes WHERE 1=1 AND STRFTIME('%Y-%m-%d',datetime(CreatedTime)) = date('now') GROUP BY STRFTIME('%Y-%m-%d',datetime(CreatedTime));";
            List<Dictionary<string, object>> listResult = mySQLite.GetExecuteResult(sqliteScript_FileHashes);
            string strDateToDay = string.Empty;
            string strDataCount = string.Empty;
            foreach (var item in listResult)
            {
                foreach (KeyValuePair<string, object> keyValuePair in item)
                {
                    if (keyValuePair.Key == "CreatedDay")
                    {
                        strDateToDay = keyValuePair.Value.ToString();
                    }
                    if (keyValuePair.Key == "Count")
                    {
                        strDataCount = keyValuePair.Value.ToString();
                    }
                }
            }
            ClassLibrary.MyLogHelper.LogSplit($"截止当前时间{DateTime.Now}，\r\n\t {strDateToDay} 共获取图片： {strDataCount} 张。");
            return result;
        }

        #region 文件分类归档时考虑通过判断文件的哈希值来判断文件是否存在
        /*
         选择使用 SQLite 还是二进制文件来存储文件哈希值，取决于你的具体需求和数据量。以下是两种方法在不同数据量范围下的适用性和性能考虑：

            SQLite
            适用数据量范围
            小型到中型数据集：几千到几百万条记录。
            大型数据集：数千万甚至上亿条记录，但需要更多的优化和索引管理。
            优点
            高效查询：SQLite 支持 SQL 查询，可以轻松实现复杂的查询和过滤操作。
            索引支持：可以通过索引加速查询性能。
            事务支持：支持事务操作，保证数据的一致性和完整性。
            易于扩展：可以轻松地添加新的字段和表结构。
            缺点
            文件大小：随着数据量的增加，数据库文件会变得更大。
            学习曲线：需要了解基本的 SQL 语法和数据库管理知识。
            二进制文件
            适用数据量范围
            小型到中型数据集：几千到几十万条记录。
            大型数据集：数百万条记录以上，但需要自定义高效的读写逻辑和索引机制。
            优点
            存储效率高：二进制文件通常比文本文件更紧凑，文件体积更小。
            读取和写入速度快：直接读取和写入内存，避免了文本解析的开销。
            灵活性：可以根据具体需求自定义数据结构和存储格式。
            缺点
            不易阅读和编辑：二进制文件不便于人类直接阅读和编辑。
            需要自定义逻辑：需要编写代码来处理数据的序列化和反序列化，以及高效的读写逻辑。
            查询性能：没有内置的索引和查询优化，需要自行实现。
            具体数据量范围建议
            几千到几万条记录：

            推荐使用 SQLite：SQLite 在这个范围内表现良好，查询性能优秀，且易于管理和维护。
            也可以使用二进制文件：如果对性能有极高要求，可以考虑使用二进制文件，但需要额外的开发工作。
            几十万到几百万条记录：

            推荐使用 SQLite：SQLite 仍然能够很好地处理这个范围内的数据，特别是通过合理的索引和优化。
            二进制文件：可以考虑使用，但需要实现高效的索引机制和查询逻辑。
            数百万到上亿条记录：

            推荐使用 SQLite：SQLite 可以处理大规模数据，但可能需要更多的优化和索引管理。
            二进制文件：在这个范围内使用二进制文件需要非常复杂的自定义逻辑，包括高效的索引和查询机制。
            示例场景
            小型项目（几千条记录）：

            SQLite：简单易用，查询性能好。
            二进制文件：如果对性能有极高要求，可以考虑。
            中型项目（几万到几十万条记录）：

            SQLite：推荐，性能和管理都较好。
            二进制文件：可以考虑，但需要额外的开发工作。
            大型项目（数百万到上亿条记录）：

            SQLite：推荐，但需要优化和索引管理。
            二进制文件：可以考虑，但需要非常复杂的自定义逻辑。
            总结
            SQLite：适用于从小型到大型的数据集，特别是需要高效查询和事务支持的场景。
            二进制文件：适用于对性能有极高要求的小型到中型数据集，但需要自定义高效的读写逻辑和索引机制。
         */
        #endregion

        #endregion

    }
}
