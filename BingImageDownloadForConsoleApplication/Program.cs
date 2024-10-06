using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BingImageDownloadForConsoleApplication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Bing必应每日壁纸图片下载";
            string strTemp = string.Empty;
            Console.WriteLine("HelloWorld!");
            Console.WriteLine(DateTime.Now);
            Console.WriteLine();

            #region 参数展示
            ClassLibrary.Services.BingImageDownloadService bingImageDownloadService = new ClassLibrary.Services.BingImageDownloadService();
            Console.WriteLine("参数配置如下：");
            string strConfig = File.ReadAllText(ClassLibrary.ShareClass._bingImageSettingFile);
            Console.WriteLine(strConfig);
            Console.WriteLine();
            #endregion

            #region 暂停、插入测试代码

            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsPath = Path.Combine(userPath, "Downloads");
            Console.WriteLine(downloadsPath);
            string windowsSpotlight = Path.Combine(userPath, "AppData", "Local", "Packages", "Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy", "LocalState", "Assets");
            Console.WriteLine(windowsSpotlight);
            Console.WriteLine();
            userPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)).FullName;
            Console.WriteLine(userPath);
            downloadsPath = Path.Combine(userPath, "Downloads");
            Console.WriteLine(downloadsPath);
            Console.WriteLine();

            //Console.ReadKey(true);
            //Console.WriteLine("确定要运行下面的程序？");
            //Console.ReadKey(true);

            #endregion

            #region 网络测试
            if (bingImageDownloadService.BingImageSetting.NetworkStateTestSwitch)
            {
                if (bingImageDownloadService.NetworkStateTest())
                {
                    Console.WriteLine($"网络通畅。");
                }
                else
                {
                    Console.WriteLine($"网络不通，可参考日志记录进行排查。");
                }
                Console.WriteLine();
            }
            #region 已注释：封装前的代码
            /*
                string dns = bingImageDownloadService.BingImageSetting.NetworkInformation.DnsServerAddress;
                Console.WriteLine("测试一：传入IPAddress");
                IPAddress iPAddress = null;
                if (IPAddress.TryParse(dns, out iPAddress))
                {
                    strTemp = $"指定的DNS服务器地址是：{dns}\r\n连接结果：{ClassLibrary.ShareClass.IsNetworkAvailable(iPAddress)}";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                else
                {
                    strTemp = "指定的DNS服务器地址非法";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                Console.WriteLine();

                Console.WriteLine("测试二：传入string");
                bool? netStatus;
                netStatus = ClassLibrary.ShareClass.IsNetworkAvailable(dns, bingImageDownloadService.BingImageSetting.NetworkInformation.SocketSendTimeout, bingImageDownloadService.BingImageSetting.NetworkInformation.SocketReceiveTimeout);
                if (netStatus is null)
                {
                    strTemp = "传入地址有误。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                else if (netStatus == true)
                {
                    strTemp = "网络通畅。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                else if (netStatus == false)
                {
                    strTemp = "网络不通。";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                else
                {
                    strTemp = "网络未知……";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                Console.WriteLine();

                Console.WriteLine("测试三：ping 域名地址");
                if (!ShareClass.NetworkTest(bingImageDownloadService.BingImageSetting.NetworkInformation.PingReplyDomain, bingImageDownloadService.BingImageSetting.NetworkInformation.NetRetryCount, bingImageDownloadService.BingImageSetting.NetworkInformation.NetWaitTime))
                {
                    strTemp = "通过发送ping指定地址(如域名)地址命令来测试网络是否通畅";
                    Console.WriteLine(strTemp);
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络是否通畅测试", strTemp, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                }
                Console.WriteLine();
                */
            #endregion

            #endregion

            //Console.ReadKey(true);
            //Console.WriteLine("确定要运行下面的程序？");
            //Console.ReadKey(true);


            #region 必应每日壁纸下载
            if (bingImageDownloadService.BingImageSetting.BingImageDownloadSwitch)
            {
                var logTitle = "必应每日壁纸下载(控制台)";
                var logType = "BingImageDownloadService";
                var logCycle = "yyyyMMdd";


                try
                {
                    // 具体要执行的代码块
                    Dictionary<string, List<string>> dicBingDownloadUrl = bingImageDownloadService.GetDownloadUrlFromResultList();
                    if (dicBingDownloadUrl.Count > 0)
                    {
                        if (bingImageDownloadService.BingImageDownloads(dicBingDownloadUrl))
                        {
                            strTemp = "成功";
                        }
                        else
                        {
                            strTemp = "失败";
                        }
                    }
                    else
                    {
                        strTemp = "错误";
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, "读取必应每日壁纸下载地址URL失败，未获取到对应下载地址等元素信息。", "Error_" + logType, logCycle);
                    }
                }
                catch (Exception ex)
                {
                    strTemp = "BUG";
                    // 记录全局异常
                    ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                }

                Console.WriteLine($"{DateTime.Now}：下载必应每日壁纸结束({strTemp})");
                Console.WriteLine();
            }
            #endregion

            #region 聚焦图片
            if (bingImageDownloadService.BingImageSetting.WindowsSpotlightSwitch)
            {
                if (bingImageDownloadService.WindowsSpotlightCopy())
                {
                    Console.WriteLine($"{DateTime.Now}：复制聚焦图片结束(成功)。");
                    // \r\n耗时：{ClassLibrary.ShareClass.ToMinutesAgo(dateTimeBingBegin)}
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}：复制聚焦图片结束(失败)。");
                }

                Console.WriteLine();
            }
            #endregion

            #region 文件分类归档
            if (bingImageDownloadService.BingImageSetting.CategorizeAndMoveFileSwitch)
            {

                if (bingImageDownloadService.CategorizeAndMoveFile())
                {
                    Console.WriteLine($"{DateTime.Now}：文件分类归档结束(成功)。");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}：文件分类归档结束(失败)。");
                }

                Console.WriteLine();
            }
            #endregion

            Console.ReadKey(true);
        }



        /// <summary>
        /// 注册事件
        /// </summary>
        static Program()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                MyHttpServiceHelper.DisposeClient();
            };
        }
    }
}
