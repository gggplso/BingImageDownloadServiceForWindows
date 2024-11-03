using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net.Http;
using ClassLibrary.Classes;
using System.Security.Cryptography;
using System.Data.SQLite;

namespace ClassLibrary
{
    public class ShareClass
    {
        #region 日志周期设定
        /// <summary>
        /// 时间周期枚举：每年、每月、每日、每时、每分、每秒
        /// </summary>
        public enum LogCycleOptions
        {
            Yearly = 0,
            Monthly = 1,
            Daily = 2,
            Hourly = 3,
            MinuteByMinute = 4,
            SecondBySecond = 5,
        }
        #region 枚举

        /// <summary>
        /// 邮件类型(枚举)
        /// </summary>
        public enum EmailType
        {
            /// <summary>
            /// 测试
            /// </summary>
            Test = 0,
            /// <summary>
            /// 通知
            /// </summary>
            Notification = 1,
            /// <summary>
            /// 提醒
            /// </summary>
            Reminder = 2,
            /// <summary>
            /// 公告
            /// </summary>
            Announcement = 3,
            /// <summary>
            /// 信息
            /// </summary>
            Informational = 4,
            /// <summary>
            /// 欢迎邮件
            /// </summary>
            WelcomeEmail = 5,

        }
        /// <summary>
        /// 邮件优先级别(枚举)
        /// </summary>
        public enum EmailPriority
        {
            /// <summary>
            /// 最高(Highest)
            /// </summary>
            Highest,
            /// <summary>
            /// 高(AboveNormal)
            /// </summary>
            AboveNormal,
            /// <summary>
            /// 正常(Normal)
            /// </summary>
            Normal,
            /// <summary>
            /// 低(BelowNormal)
            /// </summary>
            BelowNormal,
            /// <summary>
            /// 最低(Lowest)
            /// </summary>
            Lowest,
        }
        /// <summary>
        /// 邮件状态(枚举)
        /// </summary>
        public enum EmailStatus
        {
            /// <summary>
            /// 等待发送(Pending)
            /// </summary>
            Pending,
            /// <summary>
            /// 正在发送(Sending)
            /// </summary>
            Sending,
            /// <summary>
            /// 部分发送成功(PartiallySent)
            /// </summary>
            PartiallySent,
            /// <summary>
            /// 发送成功(Sent)
            /// </summary>
            Sent,
            /// <summary>
            /// 延迟发送(Delayed)
            /// </summary>
            Delayed,
            /// <summary>
            /// 已取消(Cancelled)
            /// </summary>
            Cancelled,
            /// <summary>
            /// 发送失败(Failed)
            /// </summary>
            Failed,
            /// <summary>
            /// 退信(Bounced)
            /// </summary>
            Bounced,
            /// <summary>
            /// 阅读确认(ReadReceiptReceived)
            /// </summary>
            ReadReceiptReceived,
        }

        #endregion

        /// <summary>
        /// 定义日志周期选项类
        /// </summary>
        public class LogCycleOption
        {
            public LogCycleOptions Option { get; set; }
            public string FormatString { get; set; }
            public string DisplayText { get; set; }
        }
        /// <summary>
        /// 创建一个静态的 LogCycleConverter 类，用于转换逻辑
        /// </summary>
        public static class LogCycleConverter
        {
            /// <summary>
            /// 日志周期选项与格式字符串及显示名称的集合
            /// </summary>
            public static readonly List<LogCycleOption> CycleOptions = new List<LogCycleOption>
            {
                new LogCycleOption { Option = LogCycleOptions.Yearly, FormatString = "yyyy", DisplayText = "年 - 每年新建" },
                new LogCycleOption { Option = LogCycleOptions.Monthly, FormatString = "yyyyMM", DisplayText = "年月 - 每月新建" },
                new LogCycleOption { Option = LogCycleOptions.Daily, FormatString = "yyyyMMdd", DisplayText = "年月日 - 每日新建" },
                new LogCycleOption { Option = LogCycleOptions.Hourly, FormatString = "yyyyMMddHH", DisplayText = "年月日时 - 每小时新建" },
                new LogCycleOption { Option = LogCycleOptions.MinuteByMinute, FormatString = "yyyyMMddHHmm", DisplayText = "年月日分 - 每分钟新建" },
                new LogCycleOption { Option = LogCycleOptions.SecondBySecond, FormatString = "yyyyMMddHHmmss", DisplayText = "年月日分秒 - 每秒钟新建" }
            };

            /// <summary>
            /// 根据文本描述获取对应的格式字符串
            /// </summary>
            /// <param name="textDescription"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static string GetFormatStringFromText(string textDescription)
            {
                var option = CycleOptions.Find(o => o.DisplayText == textDescription);
                if (option != null)
                    return option.FormatString;
                else
                    return "yyyyMMdd";
            }

            /// <summary>
            /// 根据格式字符串获取对应的文本描述
            /// </summary>
            /// <param name="logCycleFormat"></param>
            /// <returns></returns>
            public static string ConvertToText(string logCycleFormat)
            {
                var option = CycleOptions.Find(o => o.FormatString == logCycleFormat);
                return option?.DisplayText ?? "未知的日志周期格式";
            }
            /// <summary>
            /// 检查传入的字符串是否是有效的日志周期格式字符串
            /// </summary>
            /// <param name="logCycle"></param>
            /// <returns></returns>
            private static bool IsValidFormatString(string logCycle)
            {
                return CycleOptions.Any(option => option.FormatString == logCycle);
            }
            /// <summary>
            /// 将不符合日志周期格式的字符串定制为按年月日"yyyyMMdd"的格式
            /// </summary>
            /// <param name="logCycle"></param>
            /// <returns></returns>
            public static string VerifyValidFormatString(string logCycle)
            {
                if (IsValidFormatString(logCycle))
                {
                    return logCycle;
                }
                else
                {
                    return "yyyyMMdd";
                }
            }
        }

        #endregion

        #region 参数配置

        #region 用户目录路径
        #region 注意事项
        /*
         * 
         一、
            现象：用程序运行没问题，获取到的是：   D:\WJJ    C:\Users\wjj

                  但用Windows服务运行时，获取到的目录路径却是：  C:\WINDOWS\system32\config\systemprofile

            原因：
                 在 Windows 服务中运行时，可能会遇到这种情况，
                 因为 Windows 服务通常是以系统账户（如 LocalSystem 或 NetworkService）的身份运行，
                 而不是以普通用户的账户身份运行。
                 因此，Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) 和 Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 返回的路径可能是系统账户的路径，
                 而不是普通用户的路径。

            解决方案：
                 在运行Windows服务前，先通过WinForm程序设置好参数配置。


         二、
            服务运行时：
            MyDocuments 路径：C:\Windows\system32\config\systemprofile\Documents
            UserProfile 路径：C:\Windows\system32\config\systemprofile

            双击程序运行时：
            MyDocuments 路径：D:\MyFiles\Documents
            UserProfile 路径：C:\Users\WJJ

            Windows 服务和普通应用程序在获取用户目录路径时的行为差异是由它们的运行上下文决定的。

            运行上下文的差异
            Windows 服务：

            运行账户：Windows 服务通常以系统账户（如 LocalSystem、NetworkService 或 LocalService）或指定的用户账户运行。
            用户配置文件：这些账户通常没有完整的用户配置文件，因此 Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 和 Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) 返回的是系统账户的路径，而不是普通用户的路径。
            普通应用程序：

            运行账户：普通应用程序通常以当前登录的用户账户运行。
            用户配置文件：这些应用程序可以访问当前用户的完整配置文件，因此 Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 和 Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) 返回的是当前用户的路径。

         */
        #endregion
#if DEBUG
        // Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        // 返回的是当前登录用户的主目录路径，通常这个路径是在系统默认的盘符上（通常是 C 盘）。
        // 如果用户将自己的文件夹位置改到了 D 盘，那么这个方法返回的仍然是系统默认的主目录路径。
        public static readonly string _systemUserProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // 使用 Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        // 这个方法返回的是用户的文档目录，即使用户更改了文档目录的位置，它也能返回正确的路径。
        public static readonly string _userPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)).FullName;
        /*
         很奇怪的问题，同样的文件在另一台电脑上可以正常运行Windows服务，在这台电脑上报错：
         无法启动服务。
         System.TypeInitializationException: “ClassLibrary.ShareClass”的类型初始值设定项引发异常。
             ---> System.ArgumentException: 路径不能为空字符串或全为空白。
             参数名: path 在 System.IO.Directory.GetParent(String path)
                          在 ClassLibrary.ShareClass..cctor()
                          位置 D:\GitFiles\BingImageDownloadServiceForWindows\ClassLibrary\ShareClass.cs:行号 248
                               --- 内部异常堆栈跟踪的结尾
                               --- 在 BingImageDownloadServiceForWindows.BingImageDownloadServiceControl.OnStart(String[] args)
                                   位置 D:\GitFiles\BingImageDownloadServiceForWindows\BingImageDownloadServiceForWindows\BingImageDownloadServiceControl.cs:行号 57 
                                   在 System.ServiceProcess.ServiceBase.ServiceQueuedMainCallback(Object state)
         */
        //为了解决上面的问题，尝试修改代码如下：
#else
        public static readonly string _userPath = InitializeUserPath();
        public static readonly string _systemUserProfilePath = InitializeSystemUserProfilePath();

        private static string InitializeUserPath()
        {
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ClassLibrary.MyLogHelper.LogSplit(_logPath, "用户自定义文档目录的路径检查", $"MyDocuments路径: {myDocumentsPath}", _logType, _logCycle);
            if (string.IsNullOrWhiteSpace(myDocumentsPath))
            {
                //throw new ArgumentException("用户自定义文档目录：路径不能为空字符串或全为空白。", nameof(myDocumentsPath));
                myDocumentsPath = "C:\\Users\\Administrator\\Documents";
            }
            return Directory.GetParent(myDocumentsPath).FullName;
        }

        private static string InitializeSystemUserProfilePath()
        {
            string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            ClassLibrary.MyLogHelper.LogSplit(_logPath, "系统默认用户主目录的路径检查", $"UserProfile路径: {userProfilePath}", _logType, _logCycle);
            if (string.IsNullOrWhiteSpace(userProfilePath))
            {
                //throw new ArgumentException("系统默认用户主目录：路径不能为空字符串或全为空白。", nameof(userProfilePath));
                userProfilePath = "C:\\Users\\Administrator";
            }
            return userProfilePath;
        }

        static ShareClass()
        {
            ClassLibrary.MyLogHelper.LogSplit(_logPath, "调试：静态构造函数", "静态构造函数被调用", _logType, _logCycle);
        }

        #region 当路径为空时设置默认值以防服务启动不了。
        // 未完成：其实这里不应该这么处理，或许可以通过在服务启动时判断，并给出提示：请先配置参数设置。
        /*
         无法启动服务。System.TypeInitializationException: “ClassLibrary.ShareClass”的类型初始值设定项引发异常。 ---> System.ArgumentException: 用户自定义文档目录：路径不能为空字符串或全为空白。
            参数名: myDocumentsPath
               在 ClassLibrary.ShareClass.InitializeUserPath() 位置 D:\GitFiles\BingImageDownloadServiceForWindows\ClassLibrary\ShareClass.cs:行号 298
               在 ClassLibrary.ShareClass..cctor() 位置 D:\GitFiles\BingImageDownloadServiceForWindows\ClassLibrary\ShareClass.cs:行号 289
               --- 内部异常堆栈跟踪的结尾 ---
               在 BingImageDownloadServiceForWindows.BingImageDownloadServiceControl.OnStart(String[] args) 位置 D:\GitFiles\BingImageDownloadServiceForWindows\BingImageDownloadServiceForWindows\BingImageDownloadServiceControl.cs:行号 70
               在 System.ServiceProcess.ServiceBase.ServiceQueuedMainCallback(Object state)
         */
        #endregion
#endif

        #endregion

        #region 基础信息配置
        /// <summary>
        /// 配置文件
        /// </summary>
        public const string _mainSetting = "appSetting.json";
        public const string _bingImageSetting = "bingImageSetting.json";
        /// <summary>
        /// 配置文件及路径
        /// </summary>
        public static readonly string _mainSettingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _mainSetting);
        public static readonly string _bingImageSettingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _bingImageSetting);
        /// <summary>
        /// 默认日志保存目录
        /// </summary>
        public static string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\");
        /// <summary>
        /// 默认日志类型
        /// </summary>
        public static string _logType = "AppSetting";
        /// <summary>
        /// 默认新建日志文件的周期
        /// </summary>
        public static string _logCycle = "yyyyMMdd";
        /// <summary>
        /// 是否详细记录日志(是否开启调试日志，默认关)
        /// </summary>
        public static bool _logIsDebug = false;
        /// <summary>
        /// 首次服务启动后需等待多少时间再执行任务
        /// </summary>
        public static int _serviceRunningFirstWaitTime = 60000;
        /// <summary>
        /// 默认服务运行间隔时间
        /// </summary>
        public static int _serviceRunningWaitTime = 60;
        /// <summary>
        /// 每循环指定次数记录一回主服务的日志信息
        /// </summary>
        public static int _logCounter = 6;
        /// <summary>
        /// 必应每日壁纸下载服务开关(服务)
        /// </summary>
        public static bool _bingImageDownloadService = true;
        /// <summary>
        /// Windows聚集图片复制开关(服务)
        /// </summary>
        public static bool _windowsSpotlightCopyService = true;
        /// <summary>
        /// 文件分类归档开关(服务)
        /// </summary>
        public static bool _categorizeAndMoveFileService = true;
        /// <summary>
        /// 网络连接测试开关(服务)
        /// </summary>
        public static bool _networkStateTestService = true;
        /// <summary>
        /// Windows聚集API图片下载开关(服务)
        /// </summary>
        public static bool _windowsSpotlightDownloadService = true;
        #endregion

        #region 必应每日壁纸下载相关参数配置
        // --总配置
        //    必应每日壁纸下载开关
        //    Windows聚集图片复制开关.SpotlightSwitch = true;
        //    图片保存目录
        //    图片文件分类整理开关

        // --网络测试
        //    测试地址
        //    重试次数.NetRetryCount = 5;
        //    等待时间.NetWaitTime = 2000;

        // --必应API接口
        //    .Url1 = "https://";
        //    .Url2 = "www.bing.com";
        //    .Url3 = "/HPImageArchive.aspx?format=js&cc=cn&idx=";
        //    .Url4 = "&n=";
        //    .Url5 = "&video=1";
        //    .Url6 = "&qlt=100";
        //    .DaysAgo = 0;
        //    .AFewDays = 8;
        //    是否下载高清壁纸.PixelResolutionIsUHD = true;
        //    是否下载手机壁纸
        //    是否获取中文名.FileNameLanguageIsChinese = false;
        //    是否获取图片说明.FileNameAddTitle = false;


        //    按默认配置处理图片
        //    处理图片(移动/复制)
        //    指定电脑壁纸目录
        //    指定手机壁纸目录
        //    指定视频文件目录
        //    指定丢弃文件目录
        //      /// <summary>
        ///// 默认Windows聚焦文件所在目录
        ///// </summary>
        //public static string globalWindowsSpotlightPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\");
        //      /// <summary>
        //      /// 指定搜索的目录路径
        //      /// </summary>
        //      public static string globalSearchDirectoryPath = Environment.ExpandEnvironmentVariables(@"D:\%USERNAME%\Downloads");
        //      /// <summary>
        //      /// 电脑壁纸的目标路径
        //      /// </summary>
        //      public static string globalComputerWallpaperPath = Environment.ExpandEnvironmentVariables(@"D:\%USERNAME%\Pictures\Computer");
        //      /// <summary>
        //      /// 手机壁纸的目标路径
        //      /// </summary>
        //      public static string globalMobileWallpaperPath = Environment.ExpandEnvironmentVariables(@"D:\%USERNAME%\Pictures\Mobile");
        //      /// <summary>
        //      /// 视频文件的目标路径
        //      /// </summary>
        //      public static string globalVideoWallpaperPath = Environment.ExpandEnvironmentVariables(@"D:\%USERNAME%\Pictures\Landscape");
        //      /// <summary>
        //      /// 丢弃抛弃掉的壁纸的目标路径（Rejected/Discard）
        //      /// </summary>
        //      public static string globalRejectedPath = Environment.ExpandEnvironmentVariables(@"D:\%USERNAME%\Pictures\Rejected");
        //      /// <summary>
        //      /// 设定等待时间为5秒
        //      /// </summary>
        //      public static int globalWaitingTime = 5;

        //    Windows聚集图片目录
        //    string strSpotlightPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\");
        //            if (!Directory.Exists(strSpotlightPath))
        //            {
        //                strSpotlightPath = "";
        //            }
        //        appSettingClass.WindowsSpotlight.SpotlightPath = strSpotlightPath;
        #endregion

        #region SQLite相关配置
        /// <summary>
        /// 数据库名称
        /// </summary>
        public const string _sqliteDataName = "BingServiceDataFile.db";
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static readonly string _sqliteConnectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _sqliteDataName)};Version=3;";
        /// <summary>
        /// 初始化一张表，用于存储文件的哈希值等信息(FileHashes)
        /// </summary>
        public const string _sqliteCreateTableScript_FileHashes = @"
CREATE TABLE IF NOT EXISTS FileHashes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    HashValue TEXT NOT NULL,
    CreatedTime TEXT DEFAULT (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')),
    LastModifiedTime TEXT DEFAULT (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')),
    CONSTRAINT uc_HashValue UNIQUE (HashValue)
);

CREATE TRIGGER IF NOT EXISTS set_created_time
BEFORE INSERT ON FileHashes
FOR EACH ROW
WHEN NEW.CreatedTime IS NULL
BEGIN
    UPDATE FileHashes 
    SET CreatedTime = strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime') 
    WHERE id = NEW.id;
END;

CREATE TRIGGER IF NOT EXISTS update_last_modified_time
AFTER UPDATE ON FileHashes
FOR EACH ROW
BEGIN
    UPDATE FileHashes 
    SET LastModifiedTime = strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime') 
    WHERE id = OLD.id;
END;
";

        #endregion

        #endregion

        #region 枚举、集合
        /// <summary>
        /// 定义文件操作类型(移动/复制)
        /// </summary>
        public enum FileOperationType
        {
            /// <summary>
            /// 指示应执行文件移动操作。
            /// </summary>
            Move,

            /// <summary>
            /// 指示应执行文件复制操作。
            /// </summary>
            Copy
        }
        /// <summary>
        /// 图片后缀名的集合
        /// </summary>
        public static readonly HashSet<string> ValidImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
			// Add more supported extensions as needed.
			".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff",".jfif",
        };
        /// <summary>
        /// 视频后缀名的集合
        /// </summary>
        public static readonly HashSet<string> ValidMp4Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
			// Add more supported extensions as needed.
			".mp4",
        };
        #endregion

        #region 共享方法

        #region 读取Json配置文件
        /// <summary>
        /// 读取配置文件 appSetting.json数据到对象
        /// </summary>
        /// <returns></returns>
        public static ClassLibrary.Classes.AppSettingClass GetAppSettingClass()
        {
            try
            {
                if (File.Exists(ClassLibrary.ShareClass._mainSettingFile))
                {
                    ClassLibrary.Classes.AppSettingClass appSettingClass = ClassLibrary.MyJsonHelper.ReadJsonFileToClass<ClassLibrary.Classes.AppSettingClass>(ClassLibrary.ShareClass._mainSettingFile);
                    return appSettingClass;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                return null;
            }
        }
        /// <summary>
        /// 读取配置文件 bingImageSetting.json 数据到对象
        /// 若配置文件不存在，则初始化必应的配置参数，并保存到指定文件 bingImageSetting.json 中
        /// </summary>
        /// <returns>必应配置参数的对象</returns>
        public static ClassLibrary.Classes.BingImageSettingClass GetBingImageSettingClass()
        {
            try
            {
                ClassLibrary.Classes.BingImageSettingClass bingImageSetting = new ClassLibrary.Classes.BingImageSettingClass();
                if (File.Exists(ClassLibrary.ShareClass._bingImageSettingFile))
                {
                    bingImageSetting = ClassLibrary.MyJsonHelper.ReadJsonFileToClass<ClassLibrary.Classes.BingImageSettingClass>(ClassLibrary.ShareClass._bingImageSettingFile);
                }
                else
                {
                    if (ClassLibrary.MyJsonHelper.WriteClassToJsonFile<ClassLibrary.Classes.BingImageSettingClass>(bingImageSetting, ClassLibrary.ShareClass._bingImageSettingFile))
                    {
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "必应每日壁纸下载配置参数初始化", "配置文件初始化成功，生成配置文件。", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                    }
                    else
                    {
                        ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "必应每日壁纸下载配置参数初始化", "配置文件初始化失败，请检查原因。", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                    }
                }
                if (bingImageSetting is null)
                {
                    MyLogHelper.LogSplit(ShareClass._logPath, "必应每日壁纸下载配置参数初始化", "配置文件初始化失败,\r\n请删除文件 " + ClassLibrary.ShareClass._bingImageSettingFile + " 并重启程序。", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                }
                else
                {
                    MyLogHelper.LogSplit(ShareClass._logPath, "必应每日壁纸下载配置参数初始化", $"配置文件{ClassLibrary.ShareClass._bingImageSettingFile}读取完成，\r\n参数初始化成功 ", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                }

                return bingImageSetting;
            }
            catch (Exception ex)
            {
                ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
                ClassLibrary.MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "必应每日壁纸下载配置参数初始化", "配置文件初始化失败，请检查异常原因。\r\n" + ex.Message, ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                return null;
            }
        }
        #endregion

        #region 配置参数初始化读取
        /// <summary>
        /// 读取配置文件信息，服务OnStart启动时调用此方法，更新配置参数
        /// </summary>
        public static bool ReadConfig()
        {
            bool initializationResult = false;
            #region 全局静态变量

            if (!File.Exists(ShareClass._mainSettingFile))
            {
                ClassLibrary.Classes.AppSettingClass appSettingClass = new ClassLibrary.Classes.AppSettingClass();
                appSettingClass.MainSettingFile = ClassLibrary.ShareClass._mainSettingFile;
                appSettingClass.LogPath = ClassLibrary.ShareClass._logPath;
                appSettingClass.LogType = ClassLibrary.ShareClass._logType;
                appSettingClass.LogCycle = ClassLibrary.ShareClass._logCycle;
                appSettingClass.LogIsDebug = ClassLibrary.ShareClass._logIsDebug;
                appSettingClass.ServiceRunningFirstWaitTime = ClassLibrary.ShareClass._serviceRunningFirstWaitTime;
                appSettingClass.ServiceRunningWaitTime = ClassLibrary.ShareClass._serviceRunningWaitTime;
                appSettingClass.LogCounter = ClassLibrary.ShareClass._logCounter;
                appSettingClass.BingImageDownloadService = ClassLibrary.ShareClass._bingImageDownloadService;
                appSettingClass.WindowsSpotlightCopyService = ClassLibrary.ShareClass._windowsSpotlightCopyService;
                appSettingClass.CategorizeAndMoveFileService = ClassLibrary.ShareClass._categorizeAndMoveFileService;
                appSettingClass.NetworkStateTestService = ClassLibrary.ShareClass._networkStateTestService;
                appSettingClass.WindowsSpotlightDownloadService = ClassLibrary.ShareClass._windowsSpotlightDownloadService;


                if (MyJsonHelper.WriteClassToJsonFile<ClassLibrary.Classes.AppSettingClass>(appSettingClass, ClassLibrary.ShareClass._mainSettingFile))
                {
                    MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", "配置文件初始化成功", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

                    initializationResult = true;
                }
                else
                {
                    MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", "配置文件初始化失败", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);

                    //return initializationResult = false;
                    initializationResult = false;
                    return initializationResult;
                }
            }
            try
            {
                ClassLibrary.Classes.AppSettingClass appSettingClass = ClassLibrary.ShareClass.GetAppSettingClass();

                //ClassLibrary.ShareClass._mainSettingFile = appSettingClass.MainSettingFile;
                ClassLibrary.ShareClass._logPath = appSettingClass.LogPath;
                ClassLibrary.ShareClass._logType = appSettingClass.LogType;
                ClassLibrary.ShareClass._logCycle = appSettingClass.LogCycle;
                ClassLibrary.ShareClass._logIsDebug = appSettingClass.LogIsDebug;
                ClassLibrary.ShareClass._serviceRunningFirstWaitTime = appSettingClass.ServiceRunningFirstWaitTime;
                ClassLibrary.ShareClass._serviceRunningWaitTime = appSettingClass.ServiceRunningWaitTime;
                ClassLibrary.ShareClass._logCounter = appSettingClass.LogCounter;
                ClassLibrary.ShareClass._bingImageDownloadService = appSettingClass.BingImageDownloadService;
                ClassLibrary.ShareClass._windowsSpotlightCopyService = appSettingClass.WindowsSpotlightCopyService;
                ClassLibrary.ShareClass._categorizeAndMoveFileService = appSettingClass.CategorizeAndMoveFileService;
                ClassLibrary.ShareClass._networkStateTestService = appSettingClass.NetworkStateTestService;
                ClassLibrary.ShareClass._windowsSpotlightDownloadService = appSettingClass.WindowsSpotlightDownloadService;

                initializationResult = true;
            }
            catch (Exception ex)
            {
                MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "服务初始化", "配置文件初始化失败,请删除文件 " + ClassLibrary.ShareClass._mainSettingFile + " 并重启。", ClassLibrary.ShareClass._logType, ClassLibrary.ShareClass._logCycle);
                MyLogHelper.GetExceptionLocation(ex);
                //throw new InitializationFailedException("初始化失败，配置文件初始化失败。");
                initializationResult = false;
                return initializationResult;
            }

            #endregion
            #region 建表
            ClassLibrary.MySQLiteHelper mySQLite = new MySQLiteHelper(ClassLibrary.ShareClass._sqliteConnectionString);
            mySQLite.ExecuteScript(ClassLibrary.ShareClass._sqliteCreateTableScript_FileHashes);
            #endregion
            return initializationResult;
        }
        #endregion

        /// <summary>
        /// 计算耗时
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToMinutesAgo(DateTime dateTime)
        {
            TimeSpan ts = DateTime.Now - dateTime;

            return $"{ts.Days} 天 {ts.Hours} 时 {ts.Minutes} 分 {ts.Seconds} 秒 {ts.Milliseconds} 毫秒";
        }
        /// <summary>
        /// 将输入的毫秒数值转换为天时分秒的字符串
        /// </summary>
        /// <param name="input">需要转换的毫秒数值</param>
        /// <returns></returns>
        public static string MillisecondConversion(int input)
        {
            string result = string.Empty;
            int second = 0;
            if (input == 0)
            {
                result = "0秒";
            }
            else
            {
                while (true)
                {
                    if (input % 1000 != 0)
                    {
                        result = (input % 1000).ToString() + "毫秒";
                    }
                    second = input / 1000;
                    if (second == 0)
                    {
                        break;
                    }
                    else
                    {
                        if (second % 60 != 0)
                        {
                            result = (second % 60).ToString() + "秒" + result;
                        }
                        second = second / 60;
                        if (second == 0)
                        {
                            break;
                        }
                        else
                        {
                            if (second % 60 != 0)
                            {
                                result = (second % 60).ToString() + "分" + result;
                            }
                            second = second / 60;
                            if (second == 0)
                            {
                                break;
                            }
                            else
                            {
                                if (second % 24 != 0)
                                {
                                    result = (second % 24).ToString() + "小时" + result;
                                }
                                second = second / 24;
                                if (second == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    result = second.ToString() + "天" + result;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!result.EndsWith("毫秒") && (result.EndsWith("秒") || result.EndsWith("分")))
            {
                result += "钟";
            }
            return result;
        }

        /// <summary>
        /// 粗略验证输入的字符串是否符合电子邮箱地址的规则。
        /// </summary>
        /// <param name="emailTo">待验证的电子邮件地址。</param>
        /// <returns>如果电子邮件地址有效，则返回true；否则返回false。</returns>
        /// 本方法中的正则表达式是一个相对基础的邮件地址验证规则，但它可能不会覆盖所有合法但复杂的邮件地址格式。RFC 5322定义了电子邮件地址的标准格式，实际应用时可能需要更复杂的正则表达式来确保更高的准确性。
        /// 请注意，对于真实的生产环境，最佳实践是发送一个确认邮件到用户提供的地址以进一步验证其有效性，因为仅通过正则表达式无法完全保证地址的真实性和可送达性。
        public static bool VerifyEmailAddress(string emailTo)
        {
            //使用一个常用的电子邮件地址正则表达式进行匹配
            //string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$";
            ////创建正则表达式实例
            //Regex regex = new Regex(pattern);
            // 创建正则表达式实例并进行编译，以提高后续调用效率
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            //进行验证
            return regex.IsMatch(emailTo);
        }
        /// <summary>
        /// 验证输入字符串中的电子邮件地址是否符合规则。输入字符串可以包含多个电子邮件地址，中间用指定字符分隔，默认用分号分隔。
        /// </summary>
        /// <param name="emails">包含一个或多个电子邮件地址的字符串，地址间用指定字符分隔。</param>
        /// <param name="splitChar">用于分隔电子邮件地址的字符，默认为分号。</param>
        /// <returns>如果所有电子邮件地址都有效，则返回true；否则返回false。</returns>
        public static bool VerifyEmailAddresses(string emails, char splitChar = ';')
        {
            // 定义电子邮件地址的正则表达式
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(?:\.[a-zA-Z]{2,})?$";
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            // 将输入字符串分割成单个电子邮件地址
            string[] emailList = emails.Split(splitChar);

            // 遍历每个电子邮件地址并验证
            foreach (var email in emailList)
            {
                if (!regex.IsMatch(email.Trim()))
                {
                    // 如果发现任何一个地址无效，则立即返回false
                    return false;
                }
            }

            // 如果所有地址都通过验证，则返回true
            return true;
        }

        /// <summary>
        /// 通过此方法得到调用时所在的命名空间.类名.方法名
        /// </summary>
        /// <param name="callerMethod"></param>
        /// <returns>本方法使用方法：ClassLibrary.ShareClass.GetCallerMethodInfo(MethodBase.GetCurrentMethod());</returns>
        public static string GetCallerMethodInfo(MethodBase callerMethod)
        {
            string namespaceName = callerMethod.DeclaringType.Namespace;
            string className = callerMethod.DeclaringType.Name;
            string methodName = callerMethod.Name;
            return $"{namespaceName}.{className}.{methodName}";
            //// 通过MethodBase.GetCurrentMethod()获取当前方法（即MethodB）的信息，并传递给GetCallerMethodInfo
            //// ClassLibrary.ShareClass.GetCallerMethodInfo(MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// IP4地址附加验证规则
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// IPAddress.TryParse(input, out var ipAddress) 方法在处理IPv4地址时，确实能够识别大多数标准格式的IPv4地址，并且在地址值合法（即每个八位字节都在0-255范围内）的情况下成功解析。但是，此方法并不会检查超出正常范围的数值。
        /// 例如，输入 "256.0.0.1" 将不会通过 IPAddress.TryParse 的验证，因为它包含了超出 IPv4 地址有效范围的数字。然而，对于那些看似合法但实际上无效的IP地址，如 "0.0.0.256" 或 "192.168.0."（缺少一个数字），该方法仍可能返回 true，因为.NET框架会尽可能地尝试将输入解析为有意义的形式。
        /// 因此，在要求严格校验IP地址完整性和有效性的场景下，即使是对IPv4地址，也可能需要额外的逻辑来确保地址的所有部分都在正确范围内。但通常情况下，IPAddress.TryParse 已经能提供基本的合法性检查，并满足大多数应用场景的需求。
        private static bool VerifyIPv4(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            string[] parts = input.Split('.');
            if (parts.Length != 4)
            {
                return false;
            }
            foreach (string part in parts)
            {
                if (!byte.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out byte value) || value > 255)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 验证IP地址是否合法
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool VerifyIPAddress(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            // 先尝试IPv6解析，如果成功则返回true
            if (IPAddress.TryParse(input, out var ipAddress) && ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return true;
            }
            // 如果IPv6解析失败，则尝试IPv4解析，并对其进行详细验证
            if (IPAddress.TryParse(input, out var ip4Address) && ip4Address.AddressFamily == AddressFamily.InterNetwork)
            {
                return VerifyIPv4(input);
            }
            else
            {
                return false;
            }
            /*
             IPAddress.TryParse(input, out var ipAddress) 方法在处理IPv4地址时，确实能够识别大多数标准格式的IPv4地址，并且在地址值合法（即每个八位字节都在0-255范围内）的情况下成功解析。
             但是，此方法并不会检查超出正常范围的数值。
             例如，输入 "256.0.0.1" 将不会通过 IPAddress.TryParse 的验证，因为它包含了超出 IPv4 地址有效范围的数字。
             然而，对于那些看似合法但实际上无效的IP地址，如 "0.0.0.256" 或 "192.168.0."（缺少一个数字），该方法仍可能返回 true，因为.NET框架会尽可能地尝试将输入解析为有意义的形式。
             因此，在要求严格校验IP地址完整性和有效性的场景下，即使是对IPv4地址，也可能需要额外的逻辑来确保地址的所有部分都在正确范围内。
             但通常情况下，IPAddress.TryParse 已经能提供基本的合法性检查，并满足大多数应用场景的需求。
             */
        }

        /// <summary>
        /// 获取传入的日期中最大的日期值
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime? GetMaxDateTime(params DateTime?[] dateTimes)
        {
            if (dateTimes == null || dateTimes.Length == 0)
            {
                return null;
            }

            DateTime? maxDateTime = null;
            foreach (var dateTime in dateTimes)
            {
                if (dateTime.HasValue && (maxDateTime == null || dateTime.Value > maxDateTime.Value))
                {
                    maxDateTime = dateTime;
                }
            }
            return maxDateTime;
        }
        /// <summary>
        /// 获取传入的日期中最小的日期值
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public static DateTime? GetMinDateTime(params DateTime?[] dateTimes)
        {
            if (dateTimes == null || dateTimes.Length == 0)
            {
                return null;
            }

            DateTime? minDateTime = null;
            foreach (var dateTime in dateTimes)
            {
                if (dateTime.HasValue && (minDateTime == null || dateTime.Value < minDateTime.Value))
                {
                    minDateTime = dateTime;
                }
            }
            return minDateTime;
        }
        #region 为了进一步提高代码的复用性和可读性，可以考虑封装成一个通用方法来处理任意类型的可空值排序
        /*
        public static T? GetExtremeValue<T>(params T?[] values) where T : struct, IComparable<T>
        {
            if (values == null || values.Length == 0)
            {
                return null;
            }

            T? extremeValue = default(T?);

            foreach (var value in values)
            {
                if (value.HasValue)
                {
                    if (extremeValue == null || value.Value.CompareTo(extremeValue.Value) > 0)
                    {
                        extremeValue = value;
                    }
                }
            }

            return extremeValue;
        }

        public static DateTime? GetMaxDateTime2(params DateTime?[] dateTimes)
        {
            return GetExtremeValue<DateTime>(dateTimes);
        }

        public static DateTime? GetMinDateTime2(params DateTime?[] dateTimes)
        {
            return GetExtremeValue<DateTime>((IEnumerable<DateTime?>)dateTimes.OrderByDescending(dt => dt));
        }
        */
        #endregion

        /// <summary>
        /// 用于判断一个字符串数组是否包含另一个字符串数组
        /// </summary>
        /// <param name="sourceArray">源字符串数组</param>
        /// <param name="targetArray">需要比较的字符串数组</param>
        /// <returns></returns>
        public static bool ContainsAllStrings(string[] sourceArray, string[] targetArray)
        {
            if (targetArray == null || targetArray.Length == 0)
            {
                return true; // 如果目标数组为空或长度为0，视为源数组包含目标数组
            }

            // 使用HashSet提高查找效率（可选，适用于较大规模数据）
            HashSet<string> sourceSet = new HashSet<string>(sourceArray);

            foreach (string item in targetArray)
            {
                if (!sourceSet.Contains(item))
                {
                    return false; // 只要发现有任何一个不在源数组中，就立即返回false
                }
            }

            return true; // 所有条件都满足则说明源数组包含目标数组的所有元素
        }

        /// <summary>
        /// 从输入字符串中提取指定正则表达式匹配的内容。
        /// </summary>
        /// <param name="input">输入字符串。</param>
        /// <param name="pattern">正则表达式模式。</param>
        /// <param name="isFirst">指示是否提取第一个匹配项（true）或最后一个匹配项（false）。</param>
        /// <returns>提取到的内容或空字符串。</returns>
        public static string ExtractContentWithRegex(string input, string pattern, bool isFirst = true)
        {
            // 初始化结果为空字符串，避免返回null
            string result = string.Empty;

            try
            {
                // 检查输入字符串和正则表达式是否都不为空
                if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(pattern))
                {
                    // 创建正则表达式对象
                    Regex regex = new Regex(pattern);

                    // 尝试在输入字符串中进行匹配
                    MatchCollection matches = regex.Matches(input);

                    // 如果有匹配项
                    if (matches.Count > 0)
                    {
                        Match match;
                        // 根据 isFirst 参数选择第一个或最后一个匹配项
                        if (isFirst)
                        {
                            match = matches[0];
                        }
                        else
                        {
                            match = matches[matches.Count - 1];
                        }

                        // 如果匹配成功并且至少有一个捕获组
                        if (match.Success && match.Groups.Count > 1)
                        {
                            // 提取第一个捕获组的内容
                            result = match.Groups[1].Value;
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                // 处理正则表达式编译失败的情况
                Console.WriteLine("Error compiling regex(编译Regex时出错): " + ex.Message);
            }

            // 返回提取到的内容，或空字符串如果没有找到匹配
            return result;
        }


        /// <summary>
        /// 通过发送ping指定地址(如域名)地址命令来测试网络是否通畅
        /// </summary>
        /// <param name="strUrl">需要测试的指定地址(如域名)</param>
        /// <param name="retryCount">重复尝试的次数</param>
        /// <param name="waitTime">阻塞时重试间隔时间</param>
        /// <returns></returns>
        public static bool NetworkTest(string strUrl, int retryCount = 5, int waitTime = 5000)
        {
            bool isSuccess = false;
            int intCount = 0;
            string strResult = string.Empty;
            while (!isSuccess && intCount < retryCount)
            {
                Ping pingSender = new Ping();
                PingReply pingReply = null;
                try
                {
                    pingReply = pingSender.Send(strUrl, 1000);
                }
                catch (Exception ex)
                {
                    MyLogHelper.GetExceptionLocation(ex);
                }
                finally
                {
                    intCount++;
                    if (pingReply == null || (pingReply != null && pingReply.Status != IPStatus.Success))
                    {
                        strResult = $"第{intCount}次测试 ping {strUrl} 地址失败，\r\n无法连接，请检查网络。等待{ClassLibrary.ShareClass.MillisecondConversion(waitTime)}，重新尝试{(intCount + 1).ToString()}。";
                        Console.WriteLine(strResult);
                        MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络测试(ping指定地址)", strResult, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                        //intCount++;
                        Thread.Sleep(waitTime);
                    }
                    else if (pingReply != null && pingReply.Status == IPStatus.Success)
                    {
                        strResult = $"第{intCount}次测试 ping {strUrl} 地址成功。";
                        Console.WriteLine(strResult);
                        MyLogHelper.LogSplit(ClassLibrary.ShareClass._logPath, "网络测试(ping指定地址)", strResult, "NetworkStateTestService", ClassLibrary.ShareClass._logCycle);
                        isSuccess = true;
                    }
                }
            }
            return isSuccess;
        }
        /// <summary>
        /// 通过Socket连接到DNS服务器来判断网络是否通畅(IPAddress)
        /// </summary>
        /// <param name="dnsServerAddress">IPAddress类型的DNS服务器地址，可以是 IPv4 或 IPv6。</param>
        /// <param name="sendTimeout">默认1秒 . 超时值(以笔秒为单位)。 如果设置具有1和 499 之间的值的属性，则该值将更改为 500。 默认值为 0，表示超时期限无限。指定-1 还指示超时期限无限。</param>
        /// <param name="receiveTimeout">默认1秒 . 超时值(以毫秒为单位)。默认值为 0，表示超时期限无限。指定-1还指示超时期限无限。</param>
        /// <returns>网络是否通畅</returns>
        public static bool IsNetworkAvailable(IPAddress dnsServerAddress, int sendTimeout = 1000, int receiveTimeout = 1000)
        {
            try
            {
                // 根据 dnsServerAddress 的地址族动态创建 Socket。
                // dnsServerAddress.AddressFamily 表示 IP 地址族，可以是 IPv4 或 IPv6。
                // SocketType.Dgram 表示数据报类型（UDP）。
                // 0 表示协议号，默认为 UDP 协议。
                using (Socket socket = new Socket(dnsServerAddress.AddressFamily, SocketType.Dgram, 0))
                {
                    // 设置超时时间，避免无限等待
                    socket.SendTimeout = sendTimeout; // 默认1秒
                    socket.ReceiveTimeout = receiveTimeout; // 默认1秒

                    // 使用 Connect 方法的标准版本，传入 IPEndPoint 对象，包含了 IP 地址和端口号。
                    // Connect 方法接受一个 IPEndPoint 对象。
                    // IPEndPoint 包含 IP 地址和端口号。
                    // dnsServerAddress 是一个 IPAddress 对象。
                    // 端口号为 53，这是 DNS 的默认端口号。
                    socket.Connect(new IPEndPoint(dnsServerAddress, 53));
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 通过Socket连接到DNS服务器来判断网络是否通畅(string)
        /// </summary>
        /// <param name="serverIPAddress">string字符串类型的DNS服务器地址，可以是 IPv4 或 IPv6。如果传入的字符串不能转换为IPAddress，则返回空。</param>
        /// <param name="sendTimeout">发送数据时的超时时间。当 Socket 发送数据时，如果超过这个时间仍然没有完成发送操作，就会抛出超时异常。</param>
        /// <param name="receiveTimeout">接收数据时的超时时间。当 Socket 接收数据时，如果超过这个时间仍然没有收到任何数据，就会抛出超时异常。 </param>
        /// <returns></returns>
        public static bool? IsNetworkAvailable(string serverIPAddress, int sendTimeout = 1000, int receiveTimeout = 1000)
        {
            IPAddress dnsServerAddress = null;
            if (IPAddress.TryParse(serverIPAddress, out dnsServerAddress))
            {
                try
                {
                    // 根据 dnsServerAddress 的地址族动态创建 Socket。
                    // dnsServerAddress.AddressFamily 表示 IP 地址族，可以是 IPv4 或 IPv6。
                    // SocketType.Dgram 表示数据报类型（UDP）。
                    // 0 表示协议号，默认为 UDP 协议。
                    using (Socket socket = new Socket(dnsServerAddress.AddressFamily, SocketType.Dgram, 0))
                    {
                        // 设置超时时间，避免无限等待
                        socket.SendTimeout = sendTimeout; // 默认1秒
                        socket.ReceiveTimeout = receiveTimeout; // 默认1秒

                        // 使用 Connect 方法的标准版本，传入 IPEndPoint 对象，包含了 IP 地址和端口号。
                        // Connect 方法接受一个 IPEndPoint 对象。
                        // IPEndPoint 包含 IP 地址和端口号。
                        // dnsServerAddress 是一个 IPAddress 对象。
                        // 端口号为 53，这是 DNS 的默认端口号。
                        socket.Connect(new IPEndPoint(dnsServerAddress, 53));
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }
        #region Socket超时限制示例(已注释)
        /*
         发送数据时的超时：
        当你发送大量数据时，可能会遇到网络拥塞或其他问题导致数据发送不及时。
        设置 socket.SendTimeout 可以防止无限等待，及时发现发送失败的情况。

        接收数据时的超时：
        当你接收数据时，可能会遇到对方没有发送数据或者网络中断的情况。
        设置 socket.ReceiveTimeout 可以防止无限等待，及时发现接收失败的情况。

        示例：
            using System;
            using System.Net;
            using System.Net.Sockets;

            public static class NetworkHelper
            {
                /// <summary>
                /// 通过Socket连接到DNS服务器来判断网络是否通畅
                /// </summary>
                /// <param name="dnsServerAddress">DNS服务器地址，可以是 IPv4 或 IPv6。</param>
                /// <returns>网络是否通畅</returns>
                public static bool IsNetworkAvailable(IPAddress dnsServerAddress)
                {
                    try
                    {
                        // 根据 dnsServerAddress 的地址族动态创建 Socket。
                        // dnsServerAddress.AddressFamily 表示 IP 地址族，可以是 IPv4 或 IPv6。
                        // SocketType.Dgram 表示数据报类型（UDP）。
                        // 0 表示协议号，默认为 UDP 协议。
                        using (Socket socket = new Socket(dnsServerAddress.AddressFamily, SocketType.Dgram, 0))
                        {
                            // 设置超时时间，避免无限等待
                            socket.SendTimeout = 1000; // 发送超时时间：1秒
                            socket.ReceiveTimeout = 1000; // 接收超时时间：1秒

                            // 使用 Connect 方法的标准版本，传入 IPEndPoint 对象，包含了 IP 地址和端口号。
                            // IPEndPoint 包含 IP 地址和端口号。
                            // dnsServerAddress 是一个 IPAddress 对象。
                            // 端口号为 53，这是 DNS 的默认端口号。
                            socket.Connect(new IPEndPoint(dnsServerAddress, 53));

                            // 发送一个简单的 DNS 请求
                            byte[] request = new byte[1]; // 示例请求数据
                            socket.Send(request); // 发送数据

                            // 接收响应数据
                            byte[] response = new byte[1024];
                            int receivedBytes = socket.Receive(response); // 接收数据

                            return receivedBytes > 0; // 如果接收到数据，则表示网络通畅
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录具体的异常信息
                        Console.WriteLine($"连接失败: {ex.Message}");
                        return false;
                    }
                }
            }
         */
        #endregion

        #region 哈希值补充说明
        /*
		            // 异步主方法：将 Main 方法标记为 async Task，并在其中使用 await 调用异步方法。
					// 避免阻塞：避免在同步方法中使用.Result 和 .Wait()，以防止潜在的死锁问题。
		 */
        /*
		    //空流:
			//D41D8CD98F00B204E9800998ECF8427E 是空字符串的 MD5 哈希值。

		MD5：
		创建方法：MD5.Create()
		示例：using (HashAlgorithm hashAlgorithm = MD5.Create())

		SHA1：
		创建方法：SHA1.Create()
		示例：using (HashAlgorithm hashAlgorithm = SHA1.Create())

		SHA256：
		创建方法：SHA256.Create()
		示例：using (HashAlgorithm hashAlgorithm = SHA256.Create())

		SHA384：
		创建方法：SHA384.Create()
		示例：using (HashAlgorithm hashAlgorithm = SHA384.Create())

		SHA512：
		创建方法：SHA512.Create()
		示例：using (HashAlgorithm hashAlgorithm = SHA512.Create())

		RIPEMD160：
		创建方法：RIPEMD160.Create()
		示例：using (HashAlgorithm hashAlgorithm = RIPEMD160.Create())
		
		 */
        #endregion
        /// <summary>
        /// 根据提供的文件，获取文件的hash哈希值(本地文件)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="hashAlgorithm">所有加密哈希算法实现均必须从中派生的基类。传入参数如：MD5.Create()  SHA256.Create()</param>
        /// <returns>文件的哈希值，如果失败则返回null</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task<string> GetFileHashAsync_Local(string filePath, HashAlgorithm hashAlgorithm)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException("文件路径不能为空", nameof(filePath));
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("指定的文件不存在", filePath);
                }

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }

                    hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                    byte[] hashBytes = hashAlgorithm.Hash;

                    string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                    return hash;
                }
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
                return null;
            }
        }
        /// <summary>
        /// 根据提供的地址，获取文件的hash哈希值(可通过Url下载的文件)
        /// </summary>
        /// <param name="url">文件Url地址</param>
        /// <param name="hashAlgorithm">所有加密哈希算法实现均必须从中派生的基类。传入参数如：MD5.Create()  SHA256.Create()</param>
        /// <returns>文件的哈希值，如果失败则返回null</returns>
        public static async Task<string> GetFileHashAsync_Url(string url, HashAlgorithm hashAlgorithm)
        {
            try
            {
                using (var client = MyHttpServiceHelper.GetClient())
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;

                                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);
                                }

                                hashAlgorithm.TransformFinalBlock(buffer, 0, 0);
                                byte[] hashBytes = hashAlgorithm.Hash;
                                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                                return hash;
                            }
                        }
                        else
                        {
                            throw new Exception("请求失败，状态码: " + (int)response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
                return null;
            }
        }

        /// <summary>
        /// 根据提供的地址，下载文件到指定的路径
        /// </summary>
        /// <param name="strUrl"></param>
        /// <param name="filePath"></param>
        public static bool DownloadFile(string strUrl, string filePath)
        {
            try
            {
                System.IO.Stream stream = MyHttpServiceHelper.GetClient().GetStreamAsync(strUrl).GetAwaiter().GetResult();
                using (FileStream fsWrite = System.IO.File.Create(filePath))
                {
                    stream.CopyTo(fsWrite);
                }
                return true;
            }
            catch (Exception ex)
            {
                MyLogHelper.GetExceptionLocation(ex);
                return false;
            }
            finally
            {
                // 不需要在这里调用 DisposeClient

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
            }
        }

        /// <summary>
        /// 去除不允许在路径、文件名中使用的字符
        /// </summary>
        /// <param name="strSource">需要去除的源字符串</param>
        /// <returns></returns>
        public static string RemoveInvalidChars(string strSource, string strReplace = " ")
        {
            string regSearch = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
            Regex rg = new Regex(string.Format("[{0}]", Regex.Escape(regSearch)));
            return rg.Replace(strSource, strReplace);
        }

        /// <summary>
        /// 通过图片像素的宽高值，判断是否为横向(壁纸)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static bool IsLandscapeImage(int width, int height)
        {
            return width >= height;
        }



        #endregion
    }
}
