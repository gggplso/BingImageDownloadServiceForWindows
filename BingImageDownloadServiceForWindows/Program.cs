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
    }
}