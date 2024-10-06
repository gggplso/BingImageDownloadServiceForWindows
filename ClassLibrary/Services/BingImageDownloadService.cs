using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ClassLibrary.ShareClass;
using System.Windows.Media.Imaging;

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
                string strJson3 = GetJsonFromBingAPI(BingImageSetting, BingImageSetting.BingImageApi.BingApiRequestParams.BingUrlDaysAgo, BingImageSetting.BingImageApi.BingApiRequestParams.BingUrlAFewDays);
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
            string strUrl = bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl1
                          + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl2
                          + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl3
                          + intDaysAgo
                          + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl4
                          + intAFewDays
                          + bingImageSetting.BingImageApi.BingApiRequestParams.BingUrl5;
            try
            {
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


                        // [1]：高清电脑壁纸的下载地址 添加高清像素的分辨率(横版：landscape)
                        string strUrlIsUHD = strUrl.Substring(0, strUrl.LastIndexOf('_')) + "_UHD.jpg&qlt=100";
                        listInformation.Add(strUrlIsUHD);

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
                if (!Directory.Exists(BingImageSetting.ImageFileSavePath))
                {
                    Directory.CreateDirectory(BingImageSetting.ImageFileSavePath);
                }

                foreach (KeyValuePair<string, List<string>> item in dicBingDownloadUrl)
                {
                    //string strUrl = item.Value[0];
                    string strUrlIsUHD = item.Value[1];
                    string strUrlISMobile = item.Value[2];
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
                            string strFileNameNew = Path.Combine(BingImageSetting.ImageFileSavePath, strFileName + strChineseIsUHD);
                            if (File.Exists(strFileNameNew) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlIsUHD, strFileNameNew);
                                strTemp = $"{strFileNameNew}\r\n文件下载结果：{result}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                intBingUHDCount++;
                            }
                        }
                        if (BingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsMobile)
                        {
                            string strFileNameNew = Path.Combine(BingImageSetting.ImageFileSavePath, strFileName + strChineseIsMobile);
                            if (File.Exists(strFileNameNew) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlISMobile, strFileNameNew);
                                strTemp = $"{strFileNameNew}\r\n文件下载结果：{result}";
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
                            string strFileNameNew = Path.Combine(BingImageSetting.ImageFileSavePath, strFileName + strEnglishIsUHD);
                            if (File.Exists(strFileNameNew) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlIsUHD, strFileNameNew);
                                strTemp = $"{strFileNameNew}\r\n文件下载结果：{result}";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                intBingUHDCount++;
                            }
                        }
                        if (BingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsMobile)
                        {
                            string strFileNameNew = Path.Combine(BingImageSetting.ImageFileSavePath, strFileName + strEnglishIsMobile);
                            if (File.Exists(strFileNameNew) && !BingImageSetting.Overwrite)
                            {
                                strTemp = $"{strFileNameNew}\r\n文件已存在，根据参数配置不重新下载。";
                                Console.WriteLine(strTemp);
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                                isDownloadExists = true;
                                intBingExistsCount++;
                            }
                            else
                            {
                                result = ClassLibrary.ShareClass.DownloadFile(strUrlISMobile, strFileNameNew);
                                strTemp = $"{strFileNameNew}\r\n文件下载结果：{result}";
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

        #region Windows聚焦图片
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
                string strSourcePath = BingImageSetting.WindowsSpotlightPath;
                int intSpotlightCopyCount = 0;
                bool isExists = false;

                Stopwatch stopwatchSpotlight = new Stopwatch();
                stopwatchSpotlight.Start();
                if (Directory.Exists(strSourcePath))
                {
                    Directory.GetFiles(strSourcePath, "*", SearchOption.TopDirectoryOnly).ToList().ForEach(x =>
                    {
                        string strFileName = Path.GetFileName(x) + ".png";
                        string strDownloadFile = Path.Combine(strPath, strFileName);
                        if (!BingImageSetting.Overwrite && File.Exists(strDownloadFile))
                        {
                            strTemp = $"{strDownloadFile}\r\n文件已存在，根据参数配置不重新复制。";
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
                                                strTemp = $"{strDownloadFile}\r\n复制完成。";
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

        #region 文件分类归档
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
                        string fileExtension = Path.GetExtension(filePath);
                        string fileNewName = fileName;
                        allFilesCount++;

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
                            }
                        }
                        // 根据需要处理文件已经存在的情况。
                        // Handle file already exists situation based on your requirements.
                        // 例如，您可以选择覆盖、跳过或重命名文件。
                        // For example, you can choose to overwrite, skip, or rename the file.
                        if (File.Exists(targetFilePath))
                        {
                            // 执行适当的处理策略。
                            // TODO: Implement appropriate handling strategy.
                            strTemp = $"未移动：{fileNewName} 在 {targetFilePath} 中已存在";
                            Console.WriteLine(strTemp);
                            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, strTemp, logType, logCycle);
                            continue;
                        }

                        //File.Move(filePath, targetFilePath);
                        try
                        {
                            string strFileOperationType = string.Empty;
                            switch (BingImageSetting.CategorizeAndMoveSetting.fileOperationType)
                            {
                                case ClassLibrary.ShareClass.FileOperationType.Move:
                                    File.Move(filePath, targetFilePath);
                                    strFileOperationType = "移动到";
                                    break;
                                case ClassLibrary.ShareClass.FileOperationType.Copy:
                                    File.Copy(filePath, targetFilePath);
                                    strFileOperationType = "复制到";
                                    break;
                                default:
                                    throw new ArgumentException("指定的文件操作类型无效。\r\nInvalid file operation type specified.", nameof(BingImageSetting.CategorizeAndMoveSetting.fileOperationType));
                            }

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
                    result = true;
                    strTemp = $"共处理了{allFilesCount} - {processedFilesCount} 个文件。";
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

            return result;
        }
        #endregion



    }
}
