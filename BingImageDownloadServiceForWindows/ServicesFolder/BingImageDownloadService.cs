using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BingImageDownloadServiceForWindows
{
    internal class BingImageDownloadService
    {
        /// <summary>
        /// 创建一个信号量，用于同步并发访问
        /// </summary>
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 异步方法，现在接受一个取消令牌（必应每日壁纸下载服务）
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task BingImageDownloadServiceAsync(CancellationToken cancellationToken)
        {
            // 定义配置文件路径和相关日志信息
            var logTitle = "必应每日壁纸下载(服务)";
            var logType = "BingImageDownloadService";
            var logCycle = "yyyyMMdd";

            try
            {
                // 获取信号量，确保同一时间只有一个任务执行
                await Semaphore.WaitAsync(cancellationToken);

                //获取当前方法所在的命名空间类名方法名作为数据来源(发送邮件的创建者CreatedBy)
                string strCreatedBy = ClassLibrary.ShareClass.GetCallerMethodInfo(MethodBase.GetCurrentMethod());

                ////指定数据库用的是哪个，从这个源读取数据
                //var dataSource = ClassLibrary.ShareClass._readEmailOneDataSource;

                //读取必应每日壁纸下载配置文件，获取过滤条件
                ClassLibrary.Services.BingImageDownloadService bingImageDownloadService = new ClassLibrary.Services.BingImageDownloadService();
                if (bingImageDownloadService.BingImageSetting != null /*&& bingImageSetting.NBSailingDate != null*/)
                {
                    try
                    {
                        // 具体要执行的代码块
                        // 获取必应每日壁纸的Url及下载必要信息放入字典中存放
                        Dictionary<string, List<string>> dicBingDownloadUrl = bingImageDownloadService.GetDownloadUrlFromResultList();
                        // 通过上面获取到的信息，传入方法，开始下载
                        if (dicBingDownloadUrl.Count > 0)
                        {
                            if (bingImageDownloadService.BingImageDownloads(dicBingDownloadUrl))
                            {
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, "必应每日壁纸下载完成。", logType, logCycle);
                            }
                            else
                            {
                                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, "下载必应每日壁纸失败。", "Error_" + logType, logCycle);
                            }
                        }
                        else
                        {
                            ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, "读取必应每日壁纸下载地址URL失败，未获取到对应下载地址等元素信息。", "Error_" + logType, logCycle);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录全局异常
                        ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                    }
                }
                else
                {
                    ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, $"{strCreatedBy}\r\n读取必应每日壁纸下载服务配置文件失败，未获取到对应过滤条件，请检查相关配置。", "Error_" + logType, logCycle);
                }
            }
            catch (OperationCanceledException)
            {
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, logTitle, " 必应每日壁纸下载 服务已取消", logType, logCycle);
                // 如果是由于取消导致的异常，这里无需特别处理，因为在上层已经捕获并记录了该情况
                throw;
            }
            catch (Exception ex)
            {
                // 记录全局异常
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
            }
            finally
            {
                // 释放信号量以便其他任务可以继续执行
                Semaphore.Release();
            }
        }
    }
}
