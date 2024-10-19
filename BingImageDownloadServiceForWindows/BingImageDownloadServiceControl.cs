using ClassLibrary;
using ClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BingImageDownloadServiceForWindows
{
    public partial class BingImageDownloadServiceControl : ServiceBase
    {
        //用于记录已执行过的日期
        private HashSet<DateTime> _runOnSpecificDays = new HashSet<DateTime>();

        // 添加一个字段来记录服务是否处于暂停状态
        private volatile bool _isPaused = false;

        // 创建一个后台工作线程来处理 指定服务 服务
        private Thread _backgroundWorkerThread;

        // 创建一个取消令牌源，以便在服务停止时能够安全地取消后台任务
        private CancellationTokenSource _cancellationTokenSource;

        // 日志记数，用于降低日志记录频率
        private int _logCounter = 0;
        private int _logCount = 0;

        /// <summary>
        /// 为了使Windows服务能够在启动失败后进行重试(经测试，参数初始化失败大概需要15秒，重试10次大概需要12分钟，即需要在十分钟内启动数据库SQLSERVERAGENT服务)
        /// </summary>
        private System.Threading.Timer _retryTimer;
        private int _retryCount = 0;
        private const int MaxRetryAttempts = 10; // 最大重试次数
        private const int RetryIntervalMilliseconds = 60000; // 重试间隔（这里是1分钟）

        /// <summary>
        /// 构造函数执行组件初始化
        /// </summary>
        public BingImageDownloadServiceControl()
        {
            InitializeComponent();
        }
        // 添加公共方法来调用 OnStart 和 OnStop
        public void StartService(string[] args)
        {
            OnStart(args);
        }

        public void StopService()
        {
            OnStop();
        }
        /// <summary>
        /// 当服务启动时调用此方法
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //// 调试：检查获取路径等静态参数是否正常
            //File.WriteAllText("D:\\WindowsServiceBingImage\\20241016.txt", $"ClassLibrary.ShareClass._logPath：{ClassLibrary.ShareClass._logPath}\r\nClassLibrary.ShareClass._logType：{ClassLibrary.ShareClass._logType}\r\nClassLibrary.ShareClass._logCycle：{ClassLibrary.ShareClass._logCycle}");

            // 记录服务开始初始化的日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", $"开始参数初始化-->--> at {DateTime.Now}",
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

            StartRetryLogic();
        }

        /// <summary>
        /// 服务暂停// 在BackgroundWorker方法中检查暂停标志_isPaused
        /// </summary>
        protected override void OnPause()
        {
            // 设置暂停标志为true，使得后台任务不再尝试运行 指定服务 
            _isPaused = true;

            // 记录服务暂停日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务暂停", $"服务已暂停 at {DateTime.Now}",
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
        }

        /// <summary>
        /// 重新定义OnContinue以响应服务恢复
        /// </summary>
        protected override void OnContinue()
        {
            // 将暂停标志设置为false，使得后台任务可以再次尝试运行 指定服务 
            _isPaused = false;

            // 记录服务恢复日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务恢复", $"服务已从暂停状态恢复 at {DateTime.Now}",
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
        }

        /// <summary>
        /// 当服务停止时调用此方法
        /// </summary>
        protected override void OnStop()
        {
            if (_retryTimer != null)
            {
                _retryTimer.Dispose();
                _retryTimer = null;
            }
            if (_cancellationTokenSource != null)
            {
                // 请求取消后台任务
                _cancellationTokenSource.Cancel();
            }
            if (_backgroundWorkerThread != null)
            {
                // 等待后台线程结束（确保服务完全停止前所有操作已完成）
                _backgroundWorkerThread.Join();
            }
            // 停止服务时，释放HttpClient资源
            MyHttpServiceHelper.DisposeClient();

            // 记录服务停止日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务停止", $"服务被停止 <--<-- at {DateTime.Now}",
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
        }

        /// <summary>
        /// 启动重试逻辑
        /// </summary>
        private void StartRetryLogic()
        {
            string logMessage = $"参数初始化中……，最大重试次数{MaxRetryAttempts}，重试间隔时间{ClassLibrary.ShareClass.MillisecondConversion(RetryIntervalMilliseconds)}";

            // 记录服务开始初始化日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", logMessage,
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

            _retryTimer = new System.Threading.Timer(RetryCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// 重试回调
        /// </summary>
        /// <param name="state"></param>
        private void RetryCallback(object state)
        {
            // 尝试读取和应用配置
            bool configApplied = ClassLibrary.ShareClass.ReadConfig();

            if (configApplied)
            {
                // 记录服务结束初始化日志信息
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", $"参数初始化成功-->--> at {DateTime.Now}",
                    ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

                // 配置成功应用，启动服务核心逻辑
                InitializeAndStartServiceCore();
                _retryTimer.Dispose(); // 成功后释放定时器
                return;
            }

            // 配置应用失败
            _retryCount++;

            string logMessage = $"初始化失败，等待重试。重试次数：{_retryCount}";
            // 记录服务启动日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", logMessage,
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

            // 达到最大重试次数后停止重试
            if (_retryCount >= MaxRetryAttempts)
            {
                string errorMessage = $"达到最大重试次数({MaxRetryAttempts})，服务启动失败。";
                // 记录服务启动日志信息
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", errorMessage,
                    ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                _retryTimer.Dispose();
                // 这里可以选择停止服务，但通常情况下，服务会因为启动失败而自动停止
                // this.Stop();
            }
            else
            {
                // 计划下一次重试
                _retryTimer.Change(RetryIntervalMilliseconds, Timeout.Infinite);
            }
        }

        /// <summary>
        /// 此处编写服务启动的核心逻辑
        /// </summary>
        private void InitializeAndStartServiceCore()
        {
            // 初始化取消令牌源
            _cancellationTokenSource = new CancellationTokenSource();

            // 记录服务启动日志信息
            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务启动", $"服务已启动-->--> at {DateTime.Now}",
                ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

            //MonthlyTimer_Elapsed(null, null);

            //// 启动后台工作线程，开始处理 指定服务 任务
            //_backgroundWorkerThread = new Thread(BackgroundWorker);
            //_backgroundWorkerThread.Start();

            //这里使用了 Task.Delay 来实现延时操作，并且确保当服务停止时能够正确地取消延时任务。
            //如果在15秒内服务被停止，由于我们传递了 _cancellationTokenSource.Token 参数给 Task.Delay，
            //所以当调用 _cancellationTokenSource.Cancel() 时，延时任务会立即结束，从而避免启动后台工作线程。
            // 延迟15秒后再启动后台工作线程TimeSpan.FromSeconds(15)
            Task.Delay(TimeSpan.FromMilliseconds(ClassLibrary.ShareClass._serviceRunningFirstWaitTime), _cancellationTokenSource.Token).ContinueWith(_ =>
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    // 启动后台工作线程，开始处理 指定服务 任务
                    _backgroundWorkerThread = new Thread(BackgroundWorker);
                    _backgroundWorkerThread.Start();
                }
            }, TaskScheduler.Default);
        }

        /// <summary>
        /// 后台工作线程的核心方法，负责循环执行服务中的各种任务
        /// </summary>
        private async void BackgroundWorker()
        {
            string strNetworkStateTestService = " 网络状态测试服务 ";
            string strBingImageDownloadService = " 必应每日壁纸下载服务 ";
            string strWindowsSpotlightCopyService = " Windows聚焦图片复制服务 ";
            string strCategorizeAndMoveFileService = " 文件分类归档服务 ";



            string strTemp = string.Empty;
            //// 在低版本中StringBuilder没有IndexOf()方法，所以还是需要转换为字符串才可以，所以这里直接用字符串操作
            //StringBuilder sbTemp = new StringBuilder();
            //if(sbTemp.IndexOf())
            //sbTemp.Append("(");
            if (!strTemp.Contains("("))
            {
                strTemp += "(";
            }

            //// 添加一个字段来记录今天是否已经执行过获取系统信息的服务
            //bool _computerInfoExecutedToday = false;

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                //注意位置：为了实现日志呈递增频率记录，需将代码放在循环里
                _logCount++;
                int intFrequency = (int)Math.Pow(2, _logCounter);

                if (!_isPaused)
                {
                    try
                    {
                        // 获取当前日期
                        var currentDate = DateTime.Today;

                        // 一、根据配置判断是否开启 测试网络 的服务；
                        // 因为有时电脑刚启动，尚未连接网络，所以这里加入网络测试，既测试网络状态，又拖延时间等待系统连接网络(在测试网络中加入延迟等待时间)
                        // 因此当天只执行一次

                        if (!_runOnSpecificDays.Contains(currentDate) && ClassLibrary.ShareClass._networkStateTestService)
                        {
                            await BingImageDownloadServiceForWindows.NetworkStateTestService.NetworkStateTestServiceAsync(_cancellationTokenSource.Token);

                            // 只运行一次
                            if (ClassLibrary.ShareClass._networkStateTestService)
                            {
                                ClassLibrary.ShareClass._networkStateTestService = false;
                                ClassLibrary.MyLogHelper.LogSplit($"服务已触发一次，现，设置 ClassLibrary.ShareClass._NetworkStateTestService 的值为{ClassLibrary.ShareClass._networkStateTestService}");
                            }
                            else
                            {
                                ClassLibrary.MyLogHelper.LogSplit($"不该发生的触发事件：\r\n ClassLibrary.ShareClass._NetworkStateTestService 的值为{ClassLibrary.ShareClass._networkStateTestService}\r\n检查判断是否有误。");
                            }

                            _runOnSpecificDays.Add(currentDate);

                            if (!strTemp.Contains(strNetworkStateTestService))
                            {
                                strTemp += strNetworkStateTestService;
                            }
                        }
                        else
                        {
                            if (_logCount % intFrequency == 0)
                            {
                                ClassLibrary.MyLogHelper.LogSplit($"根据配置，{strNetworkStateTestService}不开启");
                            }
                            strTemp = strTemp.Replace(strNetworkStateTestService, "");
                        }

                        // 二、根据配置判断是否开启 必应每日壁纸下载 的服务；并且当天没有执行过

                        if (ClassLibrary.ShareClass._bingImageDownloadService)
                        {
                            await BingImageDownloadServiceForWindows.BingImageDownloadService.BingImageDownloadServiceAsync(_cancellationTokenSource.Token);
                            if (!strTemp.Contains(strBingImageDownloadService))
                            {
                                strTemp += strBingImageDownloadService;
                            }
                        }
                        else
                        {
                            if (_logCount % intFrequency == 0)
                            {
                                ClassLibrary.MyLogHelper.LogSplit($"根据配置，{strBingImageDownloadService}不开启");
                            }
                            strTemp = strTemp.Replace(strBingImageDownloadService, "");
                        }

                        // 三、根据配置判断是否开启 Windows聚焦图片复制 的服务

                        if (ClassLibrary.ShareClass._windowsSpotlightService)
                        {
                            await BingImageDownloadServiceForWindows.WindowsSpotlightCopyService.WindowsSpotlightCopyServiceAsync(_cancellationTokenSource.Token);
                            if (!strTemp.Contains(strWindowsSpotlightCopyService))
                            {
                                strTemp += strWindowsSpotlightCopyService;
                            }
                        }
                        else
                        {
                            if (_logCount % intFrequency == 0)
                            {
                                ClassLibrary.MyLogHelper.LogSplit($"根据配置，{strWindowsSpotlightCopyService}不开启");
                            }
                            strTemp = strTemp.Replace(strWindowsSpotlightCopyService, "");
                        }


                        // 四、根据配置判断是否开启 文件分类归档 的服务

                        if (ClassLibrary.ShareClass._categorizeAndMoveFileService)
                        {
                            await BingImageDownloadServiceForWindows.CategorizeAndMoveFileService.CategorizeAndMoveFileServiceAsync(_cancellationTokenSource.Token);
                            if (!strTemp.Contains(strCategorizeAndMoveFileService))
                            {
                                strTemp += strCategorizeAndMoveFileService;
                            }
                        }
                        else
                        {
                            if (_logCount % intFrequency == 0)
                            {
                                ClassLibrary.MyLogHelper.LogSplit($"根据配置，{strCategorizeAndMoveFileService}不开启");
                            }
                            strTemp = strTemp.Replace(strCategorizeAndMoveFileService, "");
                        }









                        // 应该将其他服务放置在*****服务之前，以便及时处理，而不是到下一个循环才处理

                        // 最后、根据配置判断是否开******服务

                        strTemp = strTemp.Replace(")", "");
                        strTemp += ")";
                    }
                    catch (OperationCanceledException)
                    {
                        // 当取消令牌被请求时，记录日志并跳出循环
                        ClassLibrary.MyLogHelper.LogSplit($"主体程序已取消{strTemp}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        // 记录运行过程中的异常信息
                        ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, $"主体程序运行时发生错误{strTemp}", ex.Message,
                                                          ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                    }

                    // 2024-10-06：注意下方的服务运行间隔时间。
                    // 原来间隔时间短，TimeSpan.FromSeconds(ClassLibrary.ShareClass._serviceRunningWaitTime); 10秒运行一次，那日志过于频繁，所以通过幂运算取余来作为降低日志记录频率的方法。
                    // 现在间隔时间长，TimeSpan.FromMinutes(ClassLibrary.ShareClass._serviceRunningWaitTime); 60分钟运行一次，那按每天开关机时间基本也运行不了几次，所以下面的方法就有点鸡肋了。
                    // 以上为 2024-10-06 备忘
                    // 为了实现日志呈递增频率记录，需将_logCounter++;放在最后
                    if (_logCount % intFrequency == 0)
                    {
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务运行", $"主体程序正在运行中……{strTemp}",
                            ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                        if (_logCounter <= ClassLibrary.ShareClass._logCounter)
                        {
                            _logCounter++;
                        }
                    }
                }
                else
                {
                    // 如果服务处于暂停状态，则跳过 指定服务 ，并继续等待
                    ClassLibrary.MyLogHelper.LogSplit($"主体程序已暂停，进入休眠状态{strTemp}");
                }

                // 按照配置的时间间隔休眠一段时间后再次尝试执行任务
                var waitTime = TimeSpan.FromMinutes(ClassLibrary.ShareClass._serviceRunningWaitTime);
                ClassLibrary.MyLogHelper.LogSplit($"服务暂停 {ClassLibrary.ShareClass._serviceRunningWaitTime} 分钟\r\n按照配置的时间间隔休眠一段时间后将再次执行任务。");
                await Task.Delay(waitTime, _cancellationTokenSource.Token);
            }
        }
    }
}
