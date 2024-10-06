using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class MyLogHelper
    {
        #region 基础日志记录
        /// <summary>
        /// 基础日志记录
        /// </summary>
        /// <param name="strPath">存放日志文件的目录路径，默认为程序所在路径的.\Logs\文件夹中</param>
        /// <param name="strContent">需要记录的具体的日志信息</param>
        /// <param name="logType">日志类型，并作为日志文件名的一部分，以作区分</param>
        /// <param name="logCycle">新建日志文件循环周期，以年月日时间的格式以区分(年月日时分秒.千万分之一秒 yyyyMMddHHmmss.fffffff)</param>
        private static void Log(string strPath = "Logs\\", string strContent = "", string logType = "", string logCycle = "yyyyMMdd")
        {
            if (string.IsNullOrEmpty(strPath) || strPath.Length < 1)
            {
                strPath = "Logs\\";
            }
            #region 替换非法字符不大实用
            //目录初始化
            //处理特殊字符(new string(Path.GetInvalidFileNameChars()) + 不适合处理目录路径)
            //经测试，好像没啥用，除了|外，其它的像?*<>:/"都不能被替换去除
            //string strInvalid = new string(Path.GetInvalidPathChars());
            //foreach (char item in strInvalid)
            //{
            //    strPath = strPath.Replace(item.ToString(), "");
            //}
            //strInvalid = new string(Path.GetInvalidFileNameChars());
            //foreach (char item in strInvalid)
            //{
            //    strPath = strPath.Replace(item, char.MinValue);
            //}
            //strPath = ClassLibrary.ShareClass.RemoveInvalidChars(strPath); 
            #endregion
            #region 路径和文件名中不允许的字符 方法说明
            /*
             ?*<>:/"

                public static char[] GetInvalidFileNameChars() => new char[]
                {
                    '\"', '<', '>', '|', '\0',
                    (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
                    (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
                    (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
                    (char)31, ':', '*', '?', '\\', '/'
                };

                public static char[] GetInvalidPathChars() => new char[]
                {
                    '|', '\0',
                    (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
                    (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
                    (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
                    (char)31
                };
             */
            #endregion

            //若传进来空值则处理为默认目录
            if (string.IsNullOrEmpty(strPath) || strPath.Trim().Length < 1)
            {
                strPath = @"Logs\";
            }
            if (strPath == @"Logs\")
            {
                strPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, strPath);
            }

            //判断若目录不存在，则创建
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
            //日志文件名：年月日时分秒.千万分之一秒 yyyyMMddHHmmss.fffffff
            logCycle = ClassLibrary.ShareClass.LogCycleConverter.VerifyValidFormatString(logCycle);
            string fileName = logType + "Log" + DateTime.Now.ToString(logCycle);
            //获取目录下包含日志文件名的所有文件
            string[] fileOldLogs = Directory.GetFiles(strPath, fileName + "*.log", SearchOption.TopDirectoryOnly);
            //如果不存在则就以此作为文件名
            if (fileOldLogs.Length < 1)
            {
                fileName += ".log";
            }
            else
            {
                FileInfo fileInfo;
                for (int i = 0; i < fileOldLogs.Length; i++)
                {
                    fileInfo = new FileInfo(fileOldLogs[i]);
                    //3、最后一个文件也满了就新建
                    if (fileInfo.Length >= 1024 * 1024 * 1 && i == fileOldLogs.Length - 1)
                    {
                        fileName += (i + 1).ToString() + ".log";
                        break;
                    }
                    //2、如果文件满了就略过
                    else if (fileInfo.Length >= 1024 * 1024 * 1)
                    {
                        continue;
                    }
                    //1、如果文件没满就继续用
                    else
                    {
                        fileName = fileInfo.Name;
                        break;
                    }
                }
            }
            try
            {
                //以追加的方式添加日志内容
                using (FileStream fsWrite = new FileStream(Path.Combine(strPath.EndsWith("\\") ? strPath : strPath + "\\", fileName.EndsWith(".log") ? fileName : fileName + ".log"), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    //strContent = DateTime.Now.ToString("yyyyMMddHHmmss.fffffff") + " ：" + strContent;
                    byte[] buffer = Encoding.UTF8.GetBytes(strContent + "\r\n");
                    fsWrite.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
            }
        }
        #endregion
        #region 异常信息捕获
        /// <summary>
        /// 将异常对象从其构造的堆栈跟踪，获取引发异常的具体文件位置信息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>返回一个字符串数组4项：第一个元素下标0的存放完整的信息strings[0]:拼凑出来的完整信息，strings[1]:产生异常的文件名，strings[2]:行数，strings[3]:列数</returns>
        public static string[] GetExceptionLocation(Exception ex)
        {
            string[] strings = new string[4];
            //使用提供的异常对象，捕获源信息
            StackTrace stackTrace = new StackTrace(ex, true);
            StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            strings[3] = stackFrame.GetFileColumnNumber().ToString();
            strings[2] = stackFrame.GetFileLineNumber().ToString();
            strings[1] = stackFrame.GetFileName();
            strings[0] = $"在文件{strings[1]}中的第{strings[2]}行第{strings[3]}列引发异常：{ex.Message}";

            //记录日志
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("".PadRight(30, '*'));
            sb.AppendLine("异常发生时间：" + DateTime.Now.ToString("yyyyMMddHHmmss.fffffff"));
            sb.AppendLine($"异常类型：{ex.HResult}");
            sb.AppendLine(string.Format("导致当前异常的 Exception 实例：{0}", ex.InnerException));
            sb.AppendLine(@"导致异常的应用程序或对象的名称：" + ex.Source);
            sb.Append("引发异常的方法：");
            sb.AppendLine(ex.TargetSite.ToString());
            sb.AppendFormat("异常堆栈信息：{0} \r\n", ex.StackTrace);
            sb.AppendFormat("异常消息：{0}", ex.Message);
            sb.AppendLine("\r\n 异常位置：" + strings[0]);
            sb.Append("".PadLeft(30, '*'));

            ClassLibrary.MyLogHelper.Log(strContent: sb.ToString(), logType: "Error_Exception");
            return strings;
        }
        #endregion
        #region 扩展日志方法

        /// <summary>
        /// 以星号*分隔开的基础日志
        /// </summary>
        /// <param name="logPath">日志存放路径</param>
        /// <param name="logContent">日志内容</param>
        /// <param name="logType">日志类型</param>
        /// <param name="logCycle">新建日志的循环周期</param>
        private static void LogSplit(string logPath, string logContent, string logType, string logCycle)
        {
            string str = "".PadRight(30, '*') + "\r\n";
            logContent = "\r\n" + str + logContent + "\r\n" + str;
            Log(logPath, logContent, logType, logCycle);
        }
        /// <summary>
        /// 添加标题的基础日志
        /// </summary>
        /// <param name="strPath">日志存放路径</param>
        /// <param name="strContent">日志内容</param>
        /// <param name="logType">日志类型</param>
        /// <param name="logCycle">新建日志的循环周期</param>
        /// <param name="strTitle">日志标题</param>
        public static void LogSplit(string strPath, string strTitle, string strContent, string logType, string logCycle)
        {
            strContent = "标题：" + strTitle + "\r\n内容：" + strContent + "\r\n时间：" + DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss.fffffff");
            if (string.IsNullOrEmpty(strPath) || strPath.Length < 1)
            {
                strPath = "Logs\\";
            }
            LogSplit(strPath, strContent, logType, logCycle);
        }
        /// <summary>
        /// 添加标题的简化日志
        /// </summary>
        /// <param name="strTitle">日志标题</param>
        /// <param name="strContent">日志内容</param>
        public static void LogSplit(string strTitle, string strContent)
        {
            strContent = "标题：" + strTitle + "\r\n内容：" + strContent + "\r\n时间：" + DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss.fffffff");
            LogSplit("Logs\\", strContent, "Infomational", "yyyyMMdd");
        }
        /// <summary>
        /// 添加标题、日志类型的简化日志
        /// </summary>
        /// <param name="strTitle">日志标题</param>
        /// <param name="strContent">日志内容</param>
        /// <param name="logType">日志类型</param>
        public static void LogSplit(string strTitle, string strContent, string logType)
        {
            strContent = "标题：" + strTitle + "\r\n内容：" + strContent + "\r\n时间：" + DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss.fffffff");
            LogSplit("Logs\\", strContent, logType, "yyyyMMdd");
        }
        /// <summary>
        /// 简化日志
        /// </summary>
        /// <param name="strContent">日志内容</param>
        public static void LogSplit(string strContent)
        {
            strContent = "标题：" + "未指定" + "\r\n内容：" + strContent + "\r\n时间：" + DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss.fffffff");
            LogSplit("Logs\\", strContent, "Infomational", "yyyyMMdd");
        }

        #endregion
    }
}
