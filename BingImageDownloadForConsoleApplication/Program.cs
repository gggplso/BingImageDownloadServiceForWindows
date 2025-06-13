using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

            //获取参数
            // 尝试读取和应用配置
            bool configApplied = ClassLibrary.ShareClass.ReadConfig();
            Console.WriteLine($"配置初始化：{configApplied.ToString()}");
            ClassLibrary.Services.BingImageDownloadService bingImageDownloadService = new ClassLibrary.Services.BingImageDownloadService();

#if DEBUG
            // 新写的方法放在这里方便调试

            // bingImageDownloadService.CategorizeAndMoveFile();
            // Dictionary<string, List<string>> dicBingDownloadUrltest = bingImageDownloadService.GetDownloadUrlFromResultList();

            //// 聚焦图片下载的API地址测试（失效）
            //bingImageDownloadService.WindowsSpotlightDownloadAsync().GetAwaiter().GetResult();

            #region 已注释：调试Hash
            /*
            Console.WriteLine(Task.Run(async () => { return await bingImageDownloadService.WindowsSpotlightDownloadAsync(); }).GetAwaiter().GetResult());
            Console.WriteLine();

            Console.WriteLine("输入一个文件的完整路径：");
            string strFilePath = Console.ReadLine();
            if (File.Exists(strFilePath))
            {
                string hash = ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create()).GetAwaiter().GetResult();
                //string hash = Task.Run(async () =>
                //{
                //    return await ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create());
                //}).GetAwaiter().GetResult();
                string tableName = "FileHashes";
                string createTableColumnScript = $"Id INTEGER PRIMARY KEY AUTOINCREMENT,FileName TEXT NOT NULL,FilePath TEXT NOT NULL,HashValue TEXT NOT NULL,CONSTRAINT uc_HashValue UNIQUE (HashValue)";
                var necessary = new (string column, object value)[] {
                                    ("FileName",Path.GetFileName(strFilePath)),
                                    ("FilePath",strFilePath),
                                    ("HashValue",hash)
                            };
                var conditions = new (string column, object value)[] { ("HashValue", hash) };
                string connectionString = "Data Source=example.db;Version=3;";
                var s = new ClassLibrary.MySQLiteHelper(connectionString);
                List<Dictionary<string, object>> listResult = s.GetExecuteResult(tableName, createTableColumnScript, necessary, conditions);
                foreach (var item in listResult)
                {
                    foreach (KeyValuePair<string, object> keyValuePair in item)
                    {
                        Console.WriteLine($"{keyValuePair.Key}：{keyValuePair.Value}");
                    }
                }
            }
            else
            {
                Console.WriteLine("输入有误，无法读取文件，结束。");
            } 
            */
            #endregion

            Console.WriteLine("暂停");
            Console.ReadKey(true);
            Console.WriteLine(ClassLibrary.ShareClass._sqliteConnectionString);
            Console.WriteLine("确定要运行下面的程序？");
            Console.ReadKey(true);
#else
#endif

            #region 参数展示
            Console.WriteLine("调试----");
            Console.WriteLine("参数配置如下：");
            string strAppConfig = File.ReadAllText(ClassLibrary.ShareClass._mainSettingFile);
            Console.WriteLine(strAppConfig);
            Console.WriteLine();
            Console.WriteLine("".PadRight(50, '*'));
            Console.WriteLine();
            string strBingImageConfig = File.ReadAllText(ClassLibrary.ShareClass._bingImageSettingFile);
            Console.WriteLine(strBingImageConfig);
            Console.WriteLine();
            #endregion

            #region 暂停、插入测试代码
            Console.WriteLine("测试----");
            Console.WriteLine("系统默认用户主目录");
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsPath = Path.Combine(userPath, "Downloads");
            Console.WriteLine(downloadsPath);
            string windowsSpotlight = Path.Combine(userPath, "AppData", "Local", "Packages", "Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy", "LocalState", "Assets");
            Console.WriteLine(windowsSpotlight);
            Console.WriteLine();
            Console.WriteLine("用户自定义文档目录");
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
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, "读取必应每日壁纸下载地址URL失败(程序)，未获取到对应下载地址等元素信息。", "Error_" + logType, logCycle);
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

            #region 聚焦图片复制
            if (bingImageDownloadService.BingImageSetting.WindowsSpotlightCopySwitch)
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

            #region 聚焦API图片下载
            if (bingImageDownloadService.BingImageSetting.WindowsSpotlightDownloadSwitch)
            {
                //if (bingImageDownloadService.WindowsSpotlightDownloadAsync())
                if (Task.Run(async () => { return await bingImageDownloadService.WindowsSpotlightDownloadAsync(); }).GetAwaiter().GetResult())
                {
                    Console.WriteLine($"{DateTime.Now}：Windowss聚焦API图片下载结束(成功)。");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}：Windowss聚焦API图片下载结束(失败)。");
                }

                Console.WriteLine();
            }
            #endregion

            // 应该将其他服务放置在 ** 文件分类归档 ** 服务之前，以便及时处理，而不是到下一个循环才处理

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

            #region 已注释： .Wait() 、 .GetAwaiter().GetResult() 、 .Result 区别分析
            /*
				1. .Wait()
					作用：阻塞当前线程，直到任务完成。
					返回值：无返回值，仅等待任务完成。
					异常处理：如果任务抛出异常，Wait 会抛出 AggregateException，其中包含所有内部异常。
					适用场景：适用于不需要返回值的场景，或者你只关心任务是否完成，而不关心具体的返回值。
					潜在问题：可能会导致死锁，特别是在 UI 线程中，因为 Wait 会阻塞当前线程，而异步方法可能需要当前线程来完成。
				2. .GetAwaiter().GetResult()
					作用：阻塞当前线程，直到任务完成，并返回任务的结果。
					返回值：返回任务的结果。
					异常处理：如果任务抛出异常，GetResult 会直接抛出该异常，而不是 AggregateException。
					适用场景：适用于需要获取任务结果的场景，特别是在 Main 方法中使用时相对安全。
					潜在问题：可能会导致死锁，特别是在 UI 线程中，因为 GetResult 会阻塞当前线程，而异步方法可能需要当前线程来完成。
				3. .Result
					作用：阻塞当前线程，直到任务完成，并返回任务的结果。
					返回值：返回任务的结果。
					异常处理：如果任务抛出异常，Result 会抛出 AggregateException，其中包含所有内部异常。
					适用场景：适用于需要获取任务结果的场景，特别是在 Main 方法中使用时相对安全。
					潜在问题：可能会导致死锁，特别是在 UI 线程中，因为 Result 会阻塞当前线程，而异步方法可能需要当前线程来完成。
				总结
					.Wait()：适用于不需要返回值的场景，但可能会导致死锁。
					.GetAwaiter().GetResult()：适用于需要获取任务结果的场景，但可能会导致死锁。
					.Result：适用于需要获取任务结果的场景，但可能会导致死锁。
				推荐做法
					使用 await：如果你可以使用 async 版本的 Main 方法，这是最安全和推荐的方式。
					避免在 UI 线程中使用 .Wait(), .GetAwaiter().GetResult(), 和 .Result：这些方法可能会导致死锁，特别是在 UI 线程中。
					如果你不能使用 async 版本的 Main 方法，可以考虑使用 Task.Run 结合 await 或 GetAwaiter().GetResult() 来避免潜在的死锁问题。

				如果你不能使用 async 版本的 Main 方法，但仍然希望避免潜在的死锁问题，可以结合使用 Task.Run 和 await 或 GetAwaiter().GetResult()。以下是两种方法的详细说明和示例代码：
					1. 使用 Task.Run 和 await
					虽然 Main 方法不能是 async 的，但你可以在 Main 方法中启动一个新的 Task，并在该任务中使用 await。这样可以避免在 Main 方法中直接阻塞主线程。

					示例代码
								public static void Main(string[] args)
								{
									try
									{
										// 启动一个新的任务来异步调用 WindowsSpotlightDownloadAsync
										Task.Run(async () =>
										{
											bool result = await WindowsSpotlightDownloadAsync();
											Console.WriteLine($"Download result: {result}");
										}).Wait(); // 等待任务完成
									}
									catch (Exception ex)
									{
										Console.WriteLine($"An error occurred: {ex.Message}");
									}
								}

								public static async Task<bool> WindowsSpotlightDownloadAsync()
								{
									await Task.Delay(1000);
									return true; // 假设下载成功
								}
					2. 使用 Task.Run 和 GetAwaiter().GetResult()
					你也可以使用 Task.Run 来启动异步方法，并在 Main 方法中使用 GetAwaiter().GetResult() 来获取结果。这种方式在 Main 方法中相对安全，但仍然需要注意潜在的死锁问题。

					示例代码
								public static void Main(string[] args)
								{
									try
									{
										// 启动一个新的任务来异步调用 WindowsSpotlightDownloadAsync
										bool result = Task.Run(() => WindowsSpotlightDownloadAsync()).GetAwaiter().GetResult();
										Console.WriteLine($"Download result: {result}");
									}
									catch (Exception ex)
									{
										Console.WriteLine($"An error occurred: {ex.Message}");
									}
								}

								public static async Task<bool> WindowsSpotlightDownloadAsync()
								{
									await Task.Delay(1000);
									return true; // 假设下载成功
								}
				解释
					1、使用 Task.Run 和 await：
					Task.Run(async () => { ... })：启动一个新的任务来异步调用 WindowsSpotlightDownloadAsync。
					await WindowsSpotlightDownloadAsync()：在新任务中异步等待 WindowsSpotlightDownloadAsync 完成。
					.Wait()：在 Main 方法中等待新任务完成。这种方式避免了在 Main 方法中直接阻塞主线程，从而减少了死锁的风险。

					2、使用 Task.Run 和 GetAwaiter().GetResult()：
					Task.Run(() => WindowsSpotlightDownloadAsync())：启动一个新的任务来异步调用 WindowsSpotlightDownloadAsync。
					.GetAwaiter().GetResult()：在 Main 方法中阻塞当前线程，等待任务完成并获取结果。这种方式在 Main 方法中相对安全，但仍然需要注意潜在的死锁问题。

				总结
					使用 Task.Run 和 await：这是最推荐的方式，因为它避免了在 Main 方法中直接阻塞主线程，从而减少了死锁的风险。
					使用 Task.Run 和 GetAwaiter().GetResult()：这种方式在 Main 方法中相对安全，但仍然需要注意潜在的死锁问题。
					无论哪种方式，都能确保你能够正确调用异步方法并获取其返回结果 true 或 false。选择适合你具体需求的方法即可。


				 Task.Run(() => WindowsSpotlightDownloadAsync).Wait();

          =============================================================================================================

          =============================================================================================================

            GetAwaiter().GetResult()
            csharp
            string hash = Task.Run(async () =>
            {
                return await ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create());
            }).GetAwaiter().GetResult();
            Wait()
            csharp
            string hash = Task.Run(async () =>
            {
                return await ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create());
            }).Wait();
            区别：
            返回值处理：

            GetAwaiter().GetResult()：返回任务的结果。如果你的任务返回一个值（如 string），GetAwaiter().GetResult() 会返回这个值。
            Wait()：不返回任务的结果，只等待任务完成。如果你想获取任务的结果，需要使用 Result 属性。
            异常处理：

            GetAwaiter().GetResult()：如果任务抛出异常，GetAwaiter().GetResult() 会立即抛出该异常。
            Wait()：如果任务抛出异常，Wait() 会抛出一个 AggregateException，其中包含任务抛出的所有异常。
            性能和资源管理：

            GetAwaiter().GetResult()：通常更轻量级，因为它直接返回结果，不需要额外的包装。
            Wait()：可能会稍微重一些，因为它需要创建和处理 AggregateException。

            
          =============================================================================================================

          =============================================================================================================


            1. 直接调用 GetAwaiter().GetResult()
            csharp
            string hash = ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create()).GetAwaiter().GetResult();
            2. 使用 Task.Run 调用 GetAwaiter().GetResult()
            csharp
            string hash = Task.Run(async () =>
            {
                return await ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create());
            }).GetAwaiter().GetResult();
            区别：
            线程上下文：

            直接调用 GetAwaiter().GetResult()：在当前线程上直接执行异步方法，并阻塞当前线程直到异步操作完成。这通常会导致当前线程（例如 UI 线程）被阻塞，可能导致应用程序无响应。
            使用 Task.Run：在新的线程池线程上执行异步方法，并阻塞当前线程直到新线程上的异步操作完成。这可以避免阻塞当前线程，特别是在 UI 应用程序中，可以防止界面冻结。
            性能：

            直接调用 GetAwaiter().GetResult()：通常更轻量级，因为它不需要额外的线程切换开销。
            使用 Task.Run：可能会稍微重一些，因为它涉及到线程池线程的调度和切换，但可以更好地利用多核处理器的优势。
            适用场景：

            直接调用 GetAwaiter().GetResult()：适用于简单的同步等待，特别是在不需要考虑线程上下文的控制台应用程序或后台服务中。
            使用 Task.Run：适用于需要避免阻塞当前线程的场景，特别是在 UI 应用程序中，可以保持界面的响应性。
            示例代码：
            直接调用 GetAwaiter().GetResult()
            csharp
            string hash = ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create()).GetAwaiter().GetResult();
            使用 Task.Run 调用 GetAwaiter().GetResult()
            csharp
            string hash = Task.Run(async () =>
            {
                return await ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create());
            }).GetAwaiter().GetResult();
            推荐做法：
            直接调用 GetAwaiter().GetResult()：适用于简单的同步等待，特别是在不需要考虑线程上下文的控制台应用程序或后台服务中。
            使用 Task.Run：适用于需要避免阻塞当前线程的场景，特别是在 UI 应用程序中，可以保持界面的响应性。
            异步方法推荐做法：
            在现代 C# 编程中，推荐使用 async/await 模式来处理异步操作，以避免阻塞调用和提高代码的可读性和性能。例如：

            csharp
            using System.Security.Cryptography;
            using System.Threading.Tasks;

            class Program
            {
                static async Task Main()
                {
                    string strFilePath = "path/to/your/file.txt";

                    string hash = await ClassLibrary.MyHashHelper.GetFileHashAsync_Local(strFilePath, SHA256.Create());

                    Console.WriteLine("File hash: " + hash);
                }
            }
            这样可以避免阻塞主线程，并且代码更加清晰和易于维护。
				 */
            #endregion

            if (bingImageDownloadService.BingImageSetting.AppAutoExitWaitTime >= 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(bingImageDownloadService.BingImageSetting.AppAutoExitWaitTime));
            }
            else
            {
                Console.ReadKey(true);
            }
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
