using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BingImageDownloadServiceForWindows
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
			if (Environment.UserInteractive)
            {
                // 控制台模式
                var service = new BingImageDownloadServiceControl();
                service.StartService(args);
                Console.WriteLine("Service started. Press any key to stop...");
                Console.Read();
                service.StopService();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new BingImageDownloadServiceControl()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
        #region Environment.UserInteractive 是一个布尔属性，用于指示当前进程是否正在与用户交互。具体来说，它返回一个布尔值，表示当前进程是否有一个图形用户界面（GUI）并且可以接收用户输入。

        /*
            控制台模式 (Environment.UserInteractive == true)：

            调试和测试：当你从 Visual Studio 或命令行直接运行服务时，Environment.UserInteractive 为 true。
            代码执行：
            创建 BingImageDownloadServiceControl 实例。
            调用 StartService 方法启动服务。
            输出提示信息，等待用户按键。
            用户按键后，调用 StopService 方法停止服务。

            服务模式 (Environment.UserInteractive == false)：

            安装并运行：当你使用 InstallUtil.exe 或 sc 命令将服务安装并启动时，Environment.UserInteractive 为 false。
            代码执行：
            创建 BingImageDownloadServiceControl 实例数组。
            调用 ServiceBase.Run 方法启动服务。
         */

        #endregion
    }
}