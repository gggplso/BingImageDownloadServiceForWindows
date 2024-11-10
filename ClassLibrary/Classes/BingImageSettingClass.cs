using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes
{
	/// <summary>
	/// 必应每日壁纸下载图片相关参数配置类
	/// </summary>
	public class BingImageSettingClass
	{
		/// <summary>
		/// 网络状态测试开关(程序)
		/// </summary>
		public bool NetworkStateTestSwitch { get; set; } = true;
		/// <summary>
		/// 必应每日壁纸下载开关(程序)
		/// </summary>
		public bool BingImageDownloadSwitch { get; set; } = true;
		/// <summary>
		/// Windows聚焦图片复制开关(程序)
		/// </summary>
		public bool WindowsSpotlightCopySwitch { get; set; } = true;
		/// <summary>
		/// Windows聚焦API图片下载开关(程序)
		/// </summary>
		public bool WindowsSpotlightDownloadSwitch { get; set; } = true;
		/// <summary>
		/// 文件分类归档开关(程序)
		/// </summary>
		public bool CategorizeAndMoveFileSwitch { get; set; } = true;
		/// <summary>
		/// 表示若有同名文件时重新下载覆盖原文件，false则保留原文件不重新下载。
		/// </summary>
		public bool Overwrite { get; set; } = false;
		/// <summary>
		/// 不能直接分类归档的图片文件保存目录路径
		/// </summary>
		//public string ImageFileSavePath { get; set; } = Environment.ExpandEnvironmentVariables(@"D:\%USERNAME%\Downloads");
		public string ImageFileSavePath { get; set; } = Path.Combine(ClassLibrary.ShareClass._userPath, "Pictures", "Downloads");
		/// <summary>
		/// 网络信息
		/// </summary>
		public NetworkInformationClass NetworkInformation { get; set; } = new NetworkInformationClass();
		/// <summary>
		/// 必应每日壁纸下载API相关
		/// </summary>
		public BingImageApiClass BingImageApi { get; set; } = new BingImageApiClass();
		/// <summary>
		/// Windows聚焦的相关参数配置
		/// </summary>
		public WindowsSpotlightSettingClass WindowsSpotlightSetting { get; set; } = new WindowsSpotlightSettingClass();
		/// <summary>
		/// 文件分类并移动的相关参数配置
		/// </summary>
		public CategorizeAndMoveSettingClass CategorizeAndMoveSetting { get; set; } = new CategorizeAndMoveSettingClass();
	}
	/// <summary>
	/// 网络信息类
	/// </summary>
	public class NetworkInformationClass
	{
		/// <summary>
		/// 通过Socket网络测试对端DNS服务器地址
		/// 默认阿里云DNS：223.5.5.5
		/// </summary>
		public string DnsServerAddress { get; set; } = "223.5.5.5";
		/// <summary>
		/// Socket发送数据时的超时时间
		/// 默认1秒钟
		/// </summary>
		public int SocketSendTimeout { get; set; } = 1000;
		/// <summary>
		/// Socket接收数据时的超时时间
		/// 默认1秒钟
		/// </summary>
		public int SocketReceiveTimeout { get; set; } = 1000;
		/// <summary>
		/// 通过PingReply(ping回复)测试对端域名地址
		/// 默认必应域名 www.bing.com
		/// </summary>
		public string PingReplyDomain { get; set; } = "www.bing.com";
		/// <summary>
		/// 指定PingSender(ping发送)重试次数
		/// 默认5次
		/// </summary>
		public int NetRetryCount { get; set; } = 5;
		/// <summary>
		/// 暂停指定的时间间隔(Thread.Sleep(NetWaitTime);)
		/// 默认2秒
		/// </summary>
		//public TimeSpan NetWaitTime { get; set; } = TimeSpan.FromSeconds(2);
		public int NetWaitTime { get; set; } = 5000;
	}
	/// <summary>
	/// 必应每日壁纸下载API相关类
	/// </summary>
	public class BingImageApiClass
	{
		/// <summary>
		/// API请求参数
		/// </summary>
		public BingApiRequestParamsClass BingApiRequestParams { get; set; } = new BingApiRequestParamsClass();
		/// <summary>
		/// API查询结果
		/// </summary>
		public BingApiSearchResultsClass BingApiSearchResults { get; set; } = new BingApiSearchResultsClass();
	}
	/// <summary>
	/// 必应每日壁纸下载API请求参数类
	/// </summary>
	public class BingApiRequestParamsClass
	{
		//// https://www.bing.com/HPImageArchive.aspx?format=js&cc=cn&idx=0&n=8&video=1
		//public string BingUrl1 { get; set; } = "https://";
		//public string BingUrl2 { get; set; } = "www.bing.com";
		//public string BingUrl3 { get; set; } = "/HPImageArchive.aspx?format=js&cc=cn&idx=";
		//public string BingUrl4 { get; set; } = "&n=";
		//public string BingUrl5 { get; set; } = "&video=1";
		public string BingUrl6 { get; set; } = "&qlt=100";

		/// <summary>
		/// 方案（Scheme）
		/// </summary>
		public string Scheme { get; set; } = "https";
		/// <summary>
		/// 主机（Host）
		/// </summary>
		public string Host { get; set; } = "www.bing.com";
		/// <summary>
		/// 端口（Port）
		/// </summary>
		public int Port { get; set; } = 443;
		/// <summary>
		/// 路径（Path）
		/// </summary>
		public string Path { get; set; } = "/HPImageArchive.aspx";
		/// <summary>
		/// 查询字符串（Query）FORMAT
		/// </summary>
		public string Query1 { get; set; } = "format";
		/// <summary>
		/// format：下载格式
		/// </summary>
		public string Query1Value1 { get; set; } = "js";
		/// <summary>
		/// format：下载格式
		/// </summary>
		public string Query1Value2 { get; set; } = "xml";
		/// <summary>
		/// 查询字符串（Query）CC
		/// </summary>
		public string Query2 { get; set; } = "cc";
		/// <summary>
		/// cc：国家？
		/// </summary>
		public string Query2Value { get; set; } = "cn";
		/// <summary>
		/// 查询字符串（Query）IDX
		/// </summary>
		public string Query3 { get; set; } = "idx";
        /// <summary>
        /// idx：开始时间，从几天前开始下载，目前最早是往前7天
        /// 默认0是从今天开始(原参数名：BingUrlDaysAgo)
        /// </summary>
        public int Query3Value { get; set; } = 0;
		/// <summary>
		/// 查询字符串（Query）N
		/// </summary>
		public string Query4 { get; set; } = "n";
        /// <summary>
        /// n：请求数量，下载多少数量几天，目前最多一次获取8张
        /// 默认8是下载8天(8张)的图片(原参数名：BingUrlAFewDays)
        /// </summary>
        public int Query4Value { get; set; } = 8;
		/// <summary>
		/// 查询字符串（Query）VIDEO
		/// </summary>
		public string Query5 { get; set; } = "video";
		/// <summary>
		/// video：是否包含视频
		/// </summary>
		public string Query5Value { get; set; } = "1";

		/// <summary>
		/// 是否下载高清像素的分辨率(横版：landscape)
		/// </summary>
		public bool PixelResolutionIsUHD { get; set; } = true;
		/// <summary>
		/// 是否下载手机类像素的分辨率(竖版：portrait)
		/// </summary>
		public bool PixelResolutionIsMobile { get; set; } = true;
		/// <summary>
		/// 是否用中文来命名文件名
		/// 默认是，取图片的中文说明来当文件名；否，用图片的英文作为文件名。
		/// </summary>
		public bool FileNameLanguageIsChinese { get; set; } = true;
		/// <summary>
		/// 是否将图片说标题说明添加到文件名中
		/// </summary>
		public bool FileNameAddTitle { get; set; } = true;
		/// <summary>
		/// 是否将日期添加到文件名中
		/// </summary>
		public bool FileNameAddDate { get; set; } = true;
		/// <summary>
		/// 是否全量下载
		/// 默认是：下载今天往前的8张(idx=0 n=8)，额外再下载7天前往前的8张(idx=7 n=8)，总共15张图片。
		/// </summary>
		public bool IsFullDownload { get; set; } = true;
	}
	/// <summary>
	/// 必应每日壁纸下载API查询结果类
	/// </summary>
	public class BingApiSearchResultsClass
	{
		// 结果在另一个单独类中
		// ClassLibrary.Classes.BingClass
	}
	/// <summary>
	/// Windows聚焦的相关参数配置类
	/// </summary>
	public class WindowsSpotlightSettingClass
	{
		/// <summary>
		/// Windows聚焦图片所在目录路径
		/// </summary>
		//public string WindowsSpotlightPath { get; set; } = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\");
		public string WindowsSpotlightPath { get; set; } = Path.Combine(ClassLibrary.ShareClass._systemUserProfilePath, "AppData", "Local", "Packages", "Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy", "LocalState", "Assets");
		/// <summary>
		/// 微软官方的Windows聚焦API地址
		/// </summary>
		public WindowsSpotlightAPIUrlClass WindowsSpotlightAPIUrl { get; set; } = new WindowsSpotlightAPIUrlClass();
		/// <summary>
		/// Windows聚焦API图片下载重复次数
		/// 循环访问，最大重复次数后退出循环
		/// </summary>
		public int WindowsSpotlightAPIRepeatLimit { get; set; } = 20;
	}
	public class WindowsSpotlightAPIUrlClass
	{
		/// <summary>
		/// 方案（Scheme）
		/// </summary>
		public string Scheme { get; set; } = "https";
		/// <summary>
		/// 主机（Host）
		/// </summary>
		public string Host { get; set; } = "arc.msn.com";
		/// <summary>
		/// 端口（Port）
		/// </summary>
		public int Port { get; set; } = 443;
		/// <summary>
		/// 路径（Path）
		/// </summary>
		public string Path { get; set; } = "/v3/Delivery/Placement";
		/// <summary>
		/// 查询字符串（Query）PID
		/// </summary>
		public string Query1 { get; set; } = "pid";
		/// <summary>
		/// pid：Windows锁屏的公共订阅ID。请勿更改此值(209567/209563)
		/// </summary>
		public string Query1Value1 { get; set; } = "209563";
		/// <summary>
		/// pid：Windows锁屏的公共订阅ID。请勿更改此值(209567/209563)
		/// </summary>
		public string Query1Value2 { get; set; } = "209567";
		/// <summary>
		/// 查询字符串（Query）FMT
		/// </summary>
		public string Query2 { get; set; } = "fmt";
		/// <summary>
		/// fmt：输出格式，例如json
		/// </summary>
		public string Query2Value1 { get; set; } = "json";
		/// <summary>
		/// fmt：输出格式，例如json
		/// </summary>
		public string Query2Value2 { get; set; } = "xml";
		/// <summary>
		/// 查询字符串（Query）CDM
		/// </summary>
		public string Query3 { get; set; } = "cdm";
		public string Query3Value { get; set; } = "1";
		/// <summary>
		/// 查询字符串（Query）PL
		/// </summary>
		public string Query4 { get; set; } = "pl";
		/// <summary>
		/// pl：语言环境，例如en-US
		/// </summary>
		public string Query4Value { get; set; } = "zh-CN";
		/// <summary>
		/// 查询字符串（Query）LC
		/// </summary>
		public string Query5 { get; set; } = "lc";
		/// <summary>
		/// lc：语言，例如en-US
		/// </summary>
		public string Query5Value { get; set; } = "zh-CN";
		/// <summary>
		/// 查询字符串（Query）CTRY
		/// </summary>
		public string Query6 { get; set; } = "ctry";
		/// <summary>
		/// ctry：国家，例如us,World
		/// </summary>
		public string Query6Value { get; set; } = "CN";

		/// <summary>
		/// 其他没用到的参数说明：
		///  rafb ：目的目前未知，可选
		///  ua ：客户端用户代理字符串
		///  disphorzres：屏幕宽度（以像素为单位）
		///  dispvertres：屏幕高度（以像素为单位）
		///  lo ：目前未知的用途，可选
		///  time ：时间，例如 2017-12-31T23:59:59Z
		/// </summary>

	}
	/// <summary>
	/// 文件分类并移动的相关参数配置类
	/// </summary>
	public class CategorizeAndMoveSettingClass
	{
		/// <summary>
		/// 文件操作类型(处理文件是移动还是复制)
		/// </summary>
		public ClassLibrary.ShareClass.FileOperationType FileOperationType { get; set; } = ShareClass.FileOperationType.Move;
		/// <summary>
		/// 文件所在目录源路径(需要处理的检索的源目录)
		/// </summary>
		public string SearchDirectoryPath { get; set; } = Path.Combine(ClassLibrary.ShareClass._userPath, "Pictures", "Downloads");
		/// <summary>
		/// 指定存放电脑壁纸的目标路径
		/// </summary>
		public string ComputerWallpaperPath { get; set; } = Path.Combine(ClassLibrary.ShareClass._userPath, "Pictures", "Computer");
		/// <summary>
		/// 指定存放手机壁纸的目标路径
		/// </summary>
		public string MobileWallpaperPath { get; set; } = Path.Combine(ClassLibrary.ShareClass._userPath, "Pictures", "Mobile");
		/// <summary>
		/// 指定存放视频文件的目标路径
		/// </summary>
		public string VideoWallpaperPath { get; set; } = Path.Combine(ClassLibrary.ShareClass._userPath, "Pictures", "Landscape");
		/// <summary>
		/// 指定存放丢弃抛弃掉的文件的目标路径（Rejected/Discard）
		/// </summary>
		public string RejectedPath { get; set; } = Path.Combine(ClassLibrary.ShareClass._userPath, "Pictures", "Rejected");
	}


	#region Bing必应官方接口分析说明
	/*
       
    // https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN

    //    format （非必需）
    //    返回数据格式，不存在返回xml格式 js (一般使用这个，返回json格式) xml（返回xml格式）

    //    idx （非必需）
    //    请求图片截止天数 0 今天 -1 截止中明天 （预准备的） 1 截止至昨天，类推（目前最多获取到7天前的图片）

    //    n （非必需）
    //    1-8 返回请求数量，目前最多一次获取8张

    //    mkt （非必需）
    //    地区  zh-CN

    //   举例说明：
    //   今天是2024-09-28，看返回Json中的enddate值
    //   https://cn.bing.com/HPImageArchive.aspx?format=js&idx=7&n=8&mkt=zh-CN
    //   idx=7 n=8 获取到的是  "enddate": "20240914",  ————  "enddate": "20240921",
    //   https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=8&mkt=zh-CN
    //   idx=0 n=8 获取到的是  "enddate": "20240921",  ————  "enddate": "20240928",

    //   结果中：
    //   "url": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
    //
    //    https://www.bing.com/th?id=OHR.MunichBeerfest_ZH-CN2688087718_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp
    //   id 参数：标识了这张壁纸的唯一ID，通常以 OHR. 开头，后跟壁纸的主题和一些数字。
    //   rf 参数：表示 "redirect"，用于指定一个重定向的目标地址，这个地址通常是相同图片不同分辨率的版本。
    //   pid 参数：标识了请求的来源或类型，在这里为 hp，可能是表示这张图片是在首页（Home Page）使用的。
    //   
    //   https://www.bing.com/th?id=OHR.MunichBeerfest_ZH-CN2688087718_UHD.jpg&qlt=100
    //   id 参数：标识了这张壁纸的唯一 ID，例如 OHR.MunichBeerfest_ZH-CN2688087718_UHD。
    //      OHR.：前缀，表示这是一个 Bing 壁纸的 ID。
    //      MunichBeerfest：壁纸的主题。
    //      ZH-CN：表示语言和地区（中文 - 中国）。
    //      2688087718：一个数字序列，可能表示壁纸的版本或时间戳。
    //      UHD：表示超高清（Ultra High Definition）版本。
    //   qlt 参数：表示图像的质量（Quality）。值为 100 表示最高质量。

     */
	#endregion

	#region 如何在创建大类的对象时自动初始化所有小类
	/*

     在 C# 中，如果你有一个较大的类包含了许多较小的类，
     并且希望在创建大类的对象时自动初始化所有的小类对象，可以采用以下几种策略：

     1、   构造函数初始化：在大类的构造函数中初始化所有的小类对象。
     2、   依赖注入：通过依赖注入框架来管理对象的生命周期。
     3、   属性初始化：在大类中定义小类的属性，并在构造函数中初始化这些属性。
     4、   工厂模式：使用工厂模式来创建复杂对象。

示例代码：
        假设你有一个大类 CompositeClass 包含了三个小类 ComponentA、ComponentB 和 ComponentC。

        1. 构造函数初始化
        csharp
        public class ComponentA
        {
            // ComponentA 的实现
        }

        public class ComponentB
        {
            // ComponentB 的实现
        }

        public class ComponentC
        {
            // ComponentC 的实现
        }

        public class CompositeClass
        {
            public ComponentA A { get; private set; }
            public ComponentB B { get; private set; }
            public ComponentC C { get; private set; }

            public CompositeClass()
            {
                A = new ComponentA();
                B = new ComponentB();
                C = new ComponentC();
            }
        }

        // 使用示例
        class Program
        {
            static void Main(string[] args)
            {
                var composite = new CompositeClass();
                // 现在 composite 对象包含了已经初始化的 A、B 和 C 组件
            }
        }
        2. 依赖注入
        如果你的应用程序使用依赖注入（DI），可以利用 DI 框架来管理对象的生命周期。

        csharp
        public class ComponentA
        {
            // ComponentA 的实现
        }

        public class ComponentB
        {
            // ComponentB 的实现
        }

        public class ComponentC
        {
            // ComponentC 的实现
        }

        public class CompositeClass
        {
            public ComponentA A { get; }
            public ComponentB B { get; }
            public ComponentC C { get; }

            public CompositeClass(ComponentA a, ComponentB b, ComponentC c)
            {
                A = a;
                B = b;
                C = c;
            }
        }

        // 使用示例
        class Program
        {
            static void Main(string[] args)
            {
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<ComponentA>()
                    .AddSingleton<ComponentB>()
                    .AddSingleton<ComponentC>()
                    .BuildServiceProvider();

                var composite = serviceProvider.GetService<CompositeClass>();
                // 现在 composite 对象包含了已经初始化的 A、B 和 C 组件
            }
        }
        3. 属性初始化
        csharp
        public class ComponentA
        {
            // ComponentA 的实现
        }

        public class ComponentB
        {
            // ComponentB 的实现
        }

        public class ComponentC
        {
            // ComponentC 的实现
        }

        public class CompositeClass
        {
            public ComponentA A { get; } = new ComponentA();
            public ComponentB B { get; } = new ComponentB();
            public ComponentC C { get; } = new ComponentC();
        }

        // 使用示例
        class Program
        {
            static void Main(string[] args)
            {
                var composite = new CompositeClass();
                // 现在 composite 对象包含了已经初始化的 A、B 和 C 组件
            }
        }
        4. 工厂模式
        使用工厂模式来创建 CompositeClass 的实例。

        csharp
        public class ComponentA
        {
            // ComponentA 的实现
        }

        public class ComponentB
        {
            // ComponentB 的实现
        }

        public class ComponentC
        {
            // ComponentC 的实现
        }

        public class CompositeClass
        {
            public ComponentA A { get; private set; }
            public ComponentB B { get; private set; }
            public ComponentC C { get; private set; }

            public CompositeClass()
            {
                A = new ComponentA();
                B = new ComponentB();
                C = new ComponentC();
            }
        }

        public class CompositeFactory
        {
            public static CompositeClass CreateComposite()
            {
                return new CompositeClass();
            }
        }

        // 使用示例
        class Program
        {
            static void Main(string[] args)
            {
                var composite = CompositeFactory.CreateComposite();
                // 现在 composite 对象包含了已经初始化的 A、B 和 C 组件
            }
        }
        以上四种方法都可以实现在创建大类对象时自动初始化所有的小类对象。选择哪种方法取决于你的具体需求和项目的设计原则。构造函数初始化和属性初始化较为简单，适用于小型项目。依赖注入和工厂模式更适合大型项目，特别是需要管理复杂依赖关系的情况。


 */
	#endregion

	#region 必应近期图片（Bing的图片分辨率格式种类）
	/*
     https://cn.bing.com/hp/api/v1/imagegallery?format=json

        {
            "title": "近期的主页图片",
            "data": {
                "images": [
                    {
                        "caption": "安康圣母教堂，威尼斯，意大利",
                        "title": "大运河和安康圣母教堂的鸟瞰图，威尼斯，意大利",
                        "description": "威尼斯的安康圣母教堂历史悠久，是巴洛克建筑风格的典范。这座教堂建于1631年至1687年，是为了纪念1630年威尼斯从一场毁灭性瘟疫中解脱出来。教堂建在大运河的入口处，其圆顶在威尼斯的天际线上占据着主导地位，正如今天的图片所示。意大利建筑师巴尔达萨雷·隆盖纳在26岁时就构思了这座建筑，它成为了隆盖纳毕生的事业，并在他去世后完工。教堂外部的涡卷状装饰和雕像与宽敞的八角形内部形成鲜明对比，内部装饰着文艺复兴时期艺术家提香和丁托列托的作品。每年11月，威尼斯都会在圣母节期间纪念这座教堂的重要意义，这是一场文化庆祝活动，内容包括节日游行和献蜡烛等，以感谢圣母将人类从瘟疫中解救出来。",
                        "copyright": "© Bachir Moukarzel/Amazing Aerial Agency",
                        "date": "2024 9月 27",
                        "clickUrl": "/search?q=%e5%ae%89%e5%ba%b7%e5%9c%a3%e6%af%8d%e6%95%99%e5%a0%82&form=hpcapt&filters=HpDate:\"20240926_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_UHD.jpg",
                                "wallpaper": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "",
                        "descriptionPara3": "",
                        "isoDate": "20240927"
                    },
                    {
                        "caption": "蓝斑巨嘴鸟，洛斯戈查尔斯国家公园，哥斯达黎加",
                        "title": "蓝斑巨嘴鸟，洛斯戈查尔斯国家公园，哥斯达黎加",
                        "description": "洛斯戈查尔斯国家公园坐落在哥斯达黎加的高原地区，这里有万花筒般繁茂的栖息地，其中包括蓝斑巨嘴鸟的家园。蓝斑巨嘴鸟身长11-15英寸，不是候鸟，可以用灵巧的脚趾在树枝上穿梭，用独特的喙熟练地觅食水果、昆虫和小蜥蜴。",
                        "copyright": "© Oscar Dominguez/Tandem Stills + Motion",
                        "date": "2024 9月 26",
                        "clickUrl": "/search?q=%e8%93%9d%e6%96%91%e5%b7%a8%e5%98%b4%e9%b8%9f&form=hpcapt&filters=HpDate:\"20240925_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_UHD.jpg",
                                "wallpaper": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "在3月至8月的繁殖季节，巨嘴鸟栖息在树洞里，用树叶和羽毛筑巢。鸟蛋的孵化期为16天，一对关系密切的巨嘴鸟可能会共同承担养育子女的责任，给它们的幼鸟喂大量的水果。蓝斑巨嘴鸟生性好斗，在哥斯达黎加、巴拿马西部和哥伦比亚西北部的森林中，到处都能听到它们的叫声。这些咕哝声和呱呱声混合在一起，形成一连串冗长而不入调的声音，可以传得很远。",
                        "descriptionPara3": "",
                        "isoDate": "20240926"
                    },
                    {
                        "caption": "红杉国家公园134周年纪念日",
                        "title": "巨型红杉，红杉国家公园，加利福尼亚州，美国",
                        "description": "1890年的今天，红杉国家公园在内华达山脉南部成立，旨在保护美国的自然奇观之一。红杉国家公园以占地面积超过629平方英里的巨型红杉树命名，是黑熊、骡鹿等野生动物以及包括莺、莺雀、鹟在内的200多种鸟类的家园。这里的巨型红杉树已经扎根2200多年，是地球上最古老的生物之一。公园里著名的谢尔曼将军树高达275英尺。它以美国内战将军威廉·特库姆塞·谢尔曼的名字命名，不仅高，而且宽度超过36英尺。游客们蜂拥而至，捕捉这些古老巨树的瞬间，它们红褐色的纤维状树皮和宽阔的树枝唤起了一种鲜活的历史感。",
                        "copyright": "© Galyna Andrushko/Shutterstock",
                        "date": "2024 9月 25",
                        "clickUrl": "/search?q=%e7%be%8e%e5%9b%bd%e7%ba%a2%e6%9d%89%e5%9b%bd%e5%ae%b6%e5%85%ac%e5%9b%ad&form=hpcapt&filters=HpDate:\"20240924_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_UHD.jpg",
                                "wallpaper": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "",
                        "descriptionPara3": "",
                        "isoDate": "20240925"
                    },
                    {
                        "caption": "斯卡夫塔山，瓦特纳冰川国家公园，冰岛",
                        "title": "斯卡夫塔山的瀑布，瓦特纳冰川国家公园，冰岛",
                        "description": "在冰岛的斯卡夫塔山，秋天不仅仅是一个季节，更是一种体验。从冰川到瀑布，再到北极光，瓦特纳冰川国家公园中的这片区域展示了大自然的原始之美。斯卡夫山曾经是一个繁荣的农场，现在是一个保护区，它是几千年来火山火焰和冰川水激烈交织形成的。作为欧洲第二大冰盖瓦特纳冰川的一个分支，斯卡夫塔山冰川像一条冰冻的河流一样延伸开来，吸引着游客在导游的带领下徒步旅行。公园内的斯瓦蒂瀑布的水源则来自冰川融水。瀑布周围环绕着深色的熔岩柱，从远处看就像一颗心脏，冰岛国家大剧院和雷克雅未克的哈尔格林姆斯教堂的设计灵感就来源于此。斯卡夫塔山的徒步路径既有简单的步行道，又有具有挑战性的攀登路线，每个人都能在这里找到适合自己的路。虽然这个季节的日照时间可能较短，但也有好的地方。当夜幕降临时，极光经常出现，将天空变成绿色和粉色的混合漩涡。",
                        "copyright": "© Nopasorn Kowathanakul/Getty Images",
                        "date": "2024 9月 24",
                        "clickUrl": "/search?q=%e6%96%af%e5%8d%a1%e5%a4%ab%e5%a1%94%e5%b1%b1+%e5%86%b0%e5%b2%9b&form=hpcapt&filters=HpDate:\"20240923_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_UHD.jpg",
                                "wallpaper": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "",
                        "descriptionPara3": "",
                        "isoDate": "20240924"
                    },
                    {
                        "caption": "海獭",
                        "title": "海獭，威廉王子湾,阿拉斯加州,美国",
                        "description": "本周是“海獭宣传周”，这项活动旨在引起人们对这些毛茸茸的水生哺乳动物的关注。今天图片中的海獭是在阿拉斯加威廉王子湾拍摄的。",
                        "copyright": "© Gerald Corsi/Getty Images",
                        "date": "2024 9月 23",
                        "clickUrl": "/search?q=%e6%b5%b7%e7%8d%ad&form=hpcapt&filters=HpDate:\"20240922_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_UHD.jpg",
                                "wallpaper": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "海獭原产于北太平洋北部和东部海岸。它们重达30-100磅，是觅食高手，会潜入海底寻找食物。这些小动物既聪明又可爱：它们用石头敲开贝类，是少数使用工具的动物之一。野外曾有多达30万只海獭，但在近两个世纪的时间里，由于它们浓密的皮毛非常珍贵，因此遭到了大量捕杀。到1911年，据说只剩下大约2000只。值得庆幸的是，国际狩猎禁令、保护工作和重新引入计划帮助它们恢复了生机，现在海獭的栖息地约占其历史栖息地的三分之二。尽管取得了这些成就，但一些海獭种群仍在挣扎，海獭依然是濒危物种。通过“海獭宣传周”，公众可以更好地了解这些动物的重要性。",
                        "descriptionPara3": "",
                        "isoDate": "20240923"
                    },
                    {
                        "caption": "秋分",
                        "title": "巴特米尔湖，湖区国家公园，英格兰，英国",
                        "description": "秋天正式登场了。在北半球，九月分点标志着从繁茂的夏季向温润的秋季过渡，而在南半球，它则预示着春天的开始。“Equinox”一词源于拉丁语，意为 “相等的夜晚”。在这一时刻，地轴与太阳垂直，这意味着太阳从正东升起，从正西落下，这是难得的全球平衡的时刻。这种平衡的出现是因为地轴既没有向太阳倾斜，也没有远离太阳。",
                        "copyright": "© Alan Novelli/Alamy Stock Photo",
                        "date": "2024 9月 22",
                        "clickUrl": "/search?q=%e4%b9%9d%e6%9c%88%e5%88%86%e7%82%b9&form=hpcapt&filters=HpDate:\"20240921_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_UHD.jpg",
                                "wallpaper": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "让我们一起去探索英国湖区的巴特米尔湖，这里宁静的湖水和充满传奇色彩的湖岸相映成趣。12世纪时，一位名叫布塔尔酋长的北欧领袖曾在这里反抗诺曼人的征服。有些人认为巴特米尔的名字可能来自布塔尔，但更可能的解释是来自附近的奶牛牧场，因为巴特米尔的字面意思是“奶牛牧场旁的湖泊”。该湖长约1.2英里，不仅是英国国家名胜古迹信托组织的珍宝，也是一个让你从喧嚣中解脱出来，让大自然帮你恢复精神的绝佳去处。",
                        "descriptionPara3": "",
                        "isoDate": "20240922"
                    },
                    {
                        "caption": "啤酒节，慕尼黑，德国",
                        "title": "日落时分，从高处俯瞰慕尼黑啤酒节",
                        "description": "慕尼黑啤酒节是世界上最大的节日之一，吸引了600多万游客。除了身着传统巴伐利亚服装的服务员端上一大杯泡沫丰富的啤酒之外，每个人都能在这里找到自己喜欢的东西。在这里，您可以享受欢快的音乐、惊险刺激的游乐设施以及令人垂涎欲滴的椒盐卷饼和香肠等当地美食。啤酒节在全球其他地方也有庆祝活动，包括美国的辛辛那提和丹佛等几个城市，每个城市都为节日增添了自己的地方特色。因此，无论您是喜欢啤酒还是喜欢热闹的派对，慕尼黑啤酒节都会让你体验到巴伐利亚的风情。",
                        "copyright": "© AllesSuper21/iStock/Getty Images",
                        "date": "2024 9月 21",
                        "clickUrl": "/search?q=%e6%85%95%e5%b0%bc%e9%bb%91%e5%95%a4%e9%85%92%e8%8a%82&form=hpcapt&filters=HpDate:\"20240920_1600\"",
                        "imageUrls": {
                            "landscape": {
                                "highDef": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_1920x1080.jpg",
                                "ultraHighDef": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_UHD.jpg",
                                "wallpaper": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_1920x1200.jpg"
                            },
                            "portrait": {
                                "highDef": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_768x1366.jpg",
                                "ultraHighDef": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_1080x1920.jpg",
                                "wallpaper": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_1080x1920_logo.jpg"
                            }
                        },
                        "descriptionPara2": "",
                        "descriptionPara3": "",
                        "isoDate": "20240921"
                    }
                ]
            },
            "statusCode": 200,
            "statusMessage": ""
        }
     */
	#endregion

	#region 必应接口返回的Json数据（以日期2024-09-28为例）
	/*
     * https://cn.bing.com/HPImageArchive.aspx?format=js&idx=7&n=8&mkt=zh-CN
     
         {
            "images": [
                {
                    "startdate": "20240927",
                    "fullstartdate": "202409271600",
                    "enddate": "20240928",
                    "url": "/th?id=OHR.FloridaSeashore_ZH-CN4671838639_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.FloridaSeashore_ZH-CN4671838639",
                    "copyright": "日出时的海滩，海湾岛国家海岸，佛罗里达，美国 (© Tim Fitzharris/Minden Pictures)",
                    "copyrightlink": "https://www.bing.com/search?q=%E6%B5%B7%E6%B9%BE%E5%B2%9B%E5%9B%BD%E5%AE%B6%E6%B5%B7%E5%B2%B8&form=hpcapt&mkt=zh-cn",
                    "title": "海湾群岛的光芒",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240927_FloridaSeashore%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "dc2ac4d8f535a6c6ffd9ba5921d4dccb",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240926",
                    "fullstartdate": "202409261600",
                    "enddate": "20240927",
                    "url": "/th?id=OHR.VeniceAerial_ZH-CN4070720525_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.VeniceAerial_ZH-CN4070720525",
                    "copyright": "大运河和安康圣母教堂的鸟瞰图，威尼斯，意大利 (© Bachir Moukarzel/Amazing Aerial Agency)",
                    "copyrightlink": "https://www.bing.com/search?q=%E5%AE%89%E5%BA%B7%E5%9C%A3%E6%AF%8D%E6%95%99%E5%A0%82&form=hpcapt&mkt=zh-cn",
                    "title": "在漂浮的城市之上",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240926_VeniceAerial%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "bc169a8649e14b1219be21276294badd",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240925",
                    "fullstartdate": "202409251600",
                    "enddate": "20240926",
                    "url": "/th?id=OHR.LittleToucanet_ZH-CN2910262009_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.LittleToucanet_ZH-CN2910262009",
                    "copyright": "蓝斑巨嘴鸟，洛斯戈查尔斯国家公园，哥斯达黎加 (© Oscar Dominguez/Tandem Stills + Motion)",
                    "copyrightlink": "https://www.bing.com/search?q=%E8%93%9D%E6%96%91%E5%B7%A8%E5%98%B4%E9%B8%9F&form=hpcapt&mkt=zh-cn",
                    "title": "呼叫所有巨嘴鸟的粉丝",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240925_LittleToucanet%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "b81ebeffcce069e5aa2139731a172ca9",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240924",
                    "fullstartdate": "202409241600",
                    "enddate": "20240925",
                    "url": "/th?id=OHR.GiantSequoias_ZH-CN2666897238_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.GiantSequoias_ZH-CN2666897238",
                    "copyright": "巨型红杉，红杉国家公园，加利福尼亚州，美国 (© Galyna Andrushko/Shutterstock)",
                    "copyrightlink": "https://www.bing.com/search?q=%E7%BE%8E%E5%9B%BD%E7%BA%A2%E6%9D%89%E5%9B%BD%E5%AE%B6%E5%85%AC%E5%9B%AD&form=hpcapt&mkt=zh-cn",
                    "title": "古代巨人的王国",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240924_GiantSequoias%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "66a08441f12b289acf19ecf1a9ab4421",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240923",
                    "fullstartdate": "202409231600",
                    "enddate": "20240924",
                    "url": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.SkaftafellWaterfall_ZH-CN1766863001",
                    "copyright": "斯卡夫塔山的瀑布，瓦特纳冰川国家公园，冰岛 (© Nopasorn Kowathanakul/Getty Images)",
                    "copyrightlink": "https://www.bing.com/search?q=%E6%96%AF%E5%8D%A1%E5%A4%AB%E5%A1%94%E5%B1%B1+%E5%86%B0%E5%B2%9B&form=hpcapt&mkt=zh-cn",
                    "title": "你会相信这种观点吗？",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240923_SkaftafellWaterfall%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "933dd45d25be8fb9dd9b937723bea8ed",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240922",
                    "fullstartdate": "202409221600",
                    "enddate": "20240923",
                    "url": "/th?id=OHR.IcebergOtter_ZH-CN0972467238_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.IcebergOtter_ZH-CN0972467238",
                    "copyright": "海獭，威廉王子湾,阿拉斯加州,美国 (© Gerald Corsi/Getty Images)",
                    "copyrightlink": "https://www.bing.com/search?q=%E6%B5%B7%E7%8D%AD&form=hpcapt&mkt=zh-cn",
                    "title": "令人惊叹的海獭",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240922_IcebergOtter%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "2250afc26d711b2b7542a4c7a86c2b10",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240921",
                    "fullstartdate": "202409211600",
                    "enddate": "20240922",
                    "url": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.AutumnCumbria_ZH-CN0565958390",
                    "copyright": "巴特米尔湖，湖区国家公园，英格兰，英国 (© Alan Novelli/Alamy Stock Photo)",
                    "copyrightlink": "https://www.bing.com/search?q=%E4%B9%9D%E6%9C%88%E5%88%86%E7%82%B9&form=hpcapt&mkt=zh-cn",
                    "title": "陷入沉思",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240921_AutumnCumbria%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "887ed5d68d6d3ff8cfdbe94db7dac285",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                },
                {
                    "startdate": "20240920",
                    "fullstartdate": "202409201600",
                    "enddate": "20240921",
                    "url": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
                    "urlbase": "/th?id=OHR.MunichBeerfest_ZH-CN2688087718",
                    "copyright": "日落时分，从高处俯瞰慕尼黑啤酒节 (© AllesSuper21/iStock/Getty Images)",
                    "copyrightlink": "https://www.bing.com/search?q=%E6%85%95%E5%B0%BC%E9%BB%91%E5%95%A4%E9%85%92%E8%8A%82&form=hpcapt&mkt=zh-cn",
                    "title": "为慕尼黑啤酒节干杯！",
                    "quiz": "/search?q=Bing+homepage+quiz&filters=WQOskey:%22HPQuiz_20240920_MunichBeerfest%22&FORM=HPQUIZ",
                    "wp": true,
                    "hsh": "6af59dfd1afc2d0a7d335ea4f2148ce2",
                    "drk": 1,
                    "top": 1,
                    "bot": 1,
                    "hs": []
                }
            ],
            "tooltips": {
                "loading": "正在加载...",
                "previous": "上一个图像",
                "next": "下一个图像",
                "walle": "此图片不能下载用作壁纸。",
                "walls": "下载今日美图。仅限用作桌面壁纸。"
            }
        }
     */
	#endregion

	#region JSON 知识点记录
	/*
     遇到的问题：
                获取到的存入文件后，
                用浏览器打开的内容是："item": "{"f":"raf","v":"1.0","rdr":
                用记事本打开的内容是： "item": "{\u0022f\u0022:\u0022raf\u0022,\u0022v\u0022:\u00221.0\u0022,\u0022rdr\u0022:
                为什么？
     AI回复：
            你遇到的问题是因为 JSON 字符串中的某些字符被转义了，特别是双引号 " 被转义成了 \u0022。这通常发生在嵌套的 JSON 字符串中，即 JSON 字符串中包含了另一个 JSON 字符串。

            问题分析
            浏览器显示：浏览器会自动解析转义的 Unicode 字符，所以你看到的是 "。
            记事本显示：记事本不会解析转义的 Unicode 字符，所以你看到的是 \u0022。
            解决方案
            你需要确保在写入文件之前，嵌套的 JSON 字符串被正确解析和格式化。以下是一些可能的解决方案：

            示例：使用 System.Text.Json

                    private static readonly HttpClient _client = new HttpClient();
                    ……
                    string strJson = string.Empty;
                    ……
                    // 访问Url获取Json数据
                    strJson = await _client.GetStringAsync(strUrl);
                    // 将JSON字符串格式化
	                JsonDocument jsonDoc = JsonDocument.Parse(strJson);
	                // 将JsonElement序列化为格式化的字符串
	                string formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
                    ……

                说明：
                在使用 JsonConvert.SerializeObject() 方法将对象序列化为 JSON 字符串时，如果对象中包含特殊字符（如双引号 "），这些字符会被转义为 \u0022。
                这是为了确保生成的 JSON 字符串在各种环境中都能正确解析和传输。
                为什么会出现转义字符？
                标准合规性：JSON 标准要求某些字符（如双引号 "、反斜杠 \、控制字符等）在 JSON 字符串中必须被转义，以确保 JSON 字符串的合法性。
                安全性：转义字符可以防止注入攻击，确保数据的安全传输。
                如何避免转义字符？
                如果你希望在序列化时避免这些转义字符，可以使用 JsonSerializerSettings 来配置 JsonConvert.SerializeObject() 方法。具体来说，可以设置 EscapeHtml 属性为 false。
    
    Newtonsoft.Json;
                    // 假设这是从 URL 获取到的 JSON 数据
                    string jsonString = @"{""item"": ""{\u0022f\u0022:\u0022raf\u0022,\u0022v\u0022:\u00221.0\u0022,\u0022rdr\u0022:""}""}";

                    // 解析 JSON 字符串
                    var jsonObject = JsonConvert.DeserializeObject(jsonString);

                    // 配置序列化选项，禁用 HTML 转义
                    var settings = new JsonSerializerSettings
                    {
                        EscapeHtml = false,
                        Formatting = Formatting.Indented
                    };

                    // 序列化为格式化的字符串
                    string formattedJson = JsonConvert.SerializeObject(jsonObject, settings);

                    // 写入文件
                    File.WriteAllText("output.json", formattedJson);

    System.Text.Json;
                    
                    // 假设这是从 URL 获取到的 JSON 数据
                    string jsonString = @"{""item"": ""{\u0022f\u0022:\u0022raf\u0022,\u0022v\u0022:\u00221.0\u0022,\u0022rdr\u0022:""}""}";

                    // 解析 JSON 字符串
                    var jsonObject = JsonSerializer.Deserialize<JsonElement>(jsonString);

                    // 解析内部的 JSON 字符串
                    var innerJsonObject = JsonSerializer.Deserialize<JsonElement>(jsonObject.GetProperty("item").GetString());

                    // 创建一个新的 JSON 对象
                    var newJsonObject = new JsonDocument(new JsonObject
                    {
                        { "item", innerJsonObject }
                    });

                    // 配置序列化选项，禁用 HTML 转义
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    // 序列化为格式化的字符串
                    string formattedJson = JsonSerializer.Serialize(newJsonObject.RootElement, options);

                    // 写入文件
                    File.WriteAllText("output.json", formattedJson);

        
    System.Text.Json;
                                string strUrl = "https://example.com/api/data"; // 替换为实际的 URL
                                string strJson = await _client.GetStringAsync(strUrl);

								// 解析 JSON 字符串
								var jsonObject = JsonSerializer.Deserialize<JsonElement>(strJson);

								// 解析内部的 JSON 字符串
								var innerJsonString = jsonObject.GetProperty("item").GetString();
								var innerJsonObject = JsonNode.Parse(innerJsonString).AsObject();

								// 创建一个新的 JSON 对象
								var newJsonObject = new JsonObject
								{
									{ "item", innerJsonObject }
								};

								// 配置序列化选项，禁用 HTML 转义
								var options = new JsonSerializerOptions
								{
									WriteIndented = true,
									Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
								};

								// 序列化为格式化的字符串
								string formattedJsonNew = JsonSerializer.Serialize(newJsonObject, options);

								// 写入文件
								File.WriteAllText("output.json", formattedJson);

								Console.WriteLine("JSON数据已成功写入文件: output.json");


    因：
     //CA1869: 避免为每个序列化操作创建新的“JsonSerializerOptions”实例。请改为缓存和重用实例。
								//// 配置序列化选项，禁用 HTML 转义
								//var options = new JsonSerializerOptions
								//{
								//	WriteIndented = true,
								//	Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
								//};
    System.Text.Json;修改后：
                            using System;
                            using System.IO;
                            using System.Net.Http;
                            using System.Text.Json;
                            using System.Text.Json.Nodes;
                            using System.Threading.Tasks;

                            public class Program
                            {
                                private static readonly HttpClient _client = new HttpClient();
                                private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
                                {
                                    WriteIndented = true,
                                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                };

                                public static async Task Main(string[] args)
                                {
                                    string strUrl = "https://example.com/api/data"; // 替换为实际的 URL
                                    string strJson = await _client.GetStringAsync(strUrl);

                                    // 解析 JSON 字符串
                                    var jsonObject = JsonSerializer.Deserialize<JsonElement>(strJson);

                                    // 解析内部的 JSON 字符串
                                    var innerJsonString = jsonObject.GetProperty("item").GetString();
                                    var innerJsonObject = JsonNode.Parse(innerJsonString).AsObject();

                                    // 创建一个新的 JSON 对象
                                    var newJsonObject = new JsonObject
                                    {
                                        { "item", innerJsonObject }
                                    };

                                    // 序列化为格式化的字符串
                                    string formattedJson = JsonSerializer.Serialize(newJsonObject, _jsonSerializerOptions);

                                    // 写入文件
                                    File.WriteAllText("output.json", formattedJson);

                                    Console.WriteLine("JSON数据已成功写入文件: output.json");
                                }
                            }

    System.Text.Json;再次修改后：
                            上面在处理"item"时还是遇到了问题，因为第一层找不到，存在的 "item" 键，它嵌套在多个层级中。具体来说，"item" 键位于 "batchrsp" 对象的 "items" 数组中的某个对象内。
                            所以需要提取：

                            using System;
                            using System.IO;
                            using System.Net.Http;
                            using System.Text.Json;
                            using System.Text.Json.Nodes;
                            using System.Threading.Tasks;

                            public class Program
                            {
                                private static readonly HttpClient _client = new HttpClient();
                                private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
                                {
                                    WriteIndented = true,
                                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                };

                                public static async Task Main(string[] args)
                                {
                                    string strUrl = "https://example.com/api/data"; // 替换为实际的 URL
                                    string strJson = await _client.GetStringAsync(strUrl);

                                    // 打印获取到的 JSON 数据，确保它包含 "item" 键
                                    Console.WriteLine("获取到的 JSON 数据:");
                                    Console.WriteLine(strJson);

                                    try
                                    {
                                        // 解析 JSON 字符串
                                        var jsonObject = JsonSerializer.Deserialize<JsonElement>(strJson);

                                        // 检查是否包含 "batchrsp" 键
                                        if (jsonObject.TryGetProperty("batchrsp", out JsonElement batchrspElement))
                                        {
                                            // 检查是否包含 "items" 键
                                            if (batchrspElement.TryGetProperty("items", out JsonElement itemsElement))
                                            {
                                                // 遍历 "items" 数组
                                                foreach (var itemElement in itemsElement.EnumerateArray())
                                                {
                                                    // 检查是否包含 "item" 键
                                                    if (itemElement.TryGetProperty("item", out JsonElement innerItemElement))
                                                    {
                                                        // 解析内部的 JSON 字符串
                                                        var innerJsonString = innerItemElement.GetString();
                                                        var innerJsonObject = JsonNode.Parse(innerJsonString).AsObject();

                                                        // 创建一个新的 JSON 对象
                                                        var newJsonObject = new JsonObject
                                                        {
                                                            { "item", innerJsonObject }
                                                        };

                                                        // 序列化为格式化的字符串
                                                        string formattedJson = JsonSerializer.Serialize(newJsonObject, _jsonSerializerOptions);

                                                        // 写入文件
                                                        File.WriteAllText("output.json", formattedJson);

                                                        Console.WriteLine("JSON数据已成功写入文件: output.json");
                                                        return; // 如果只需要处理第一个 "item"，可以在这里返回
                                                    }
                                                }

                                                Console.WriteLine("JSON 数据中未找到 'item' 键。");
                                            }
                                            else
                                            {
                                                Console.WriteLine("JSON 数据中未找到 'items' 键。");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("JSON 数据中未找到 'batchrsp' 键。");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"发生错误: {ex.Message}");
                                        Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                                    }
                                }
                            }
            关键步骤：
                    获取 JSON 数据：使用 HttpClient 从指定的 URL 获取 JSON 数据。
                    打印 JSON 数据：在控制台中打印获取到的 JSON 数据，确保它包含 "item" 键。
                    解析 JSON 字符串：使用 JsonSerializer.Deserialize<JsonElement> 将 JSON 字符串解析为 JsonElement 对象。
                    检查是否包含 "batchrsp" 键：使用 TryGetProperty 方法检查 JsonElement 是否包含 "batchrsp" 键。
                    检查是否包含 "items" 键：使用 TryGetProperty 方法检查 batchrspElement 是否包含 "items" 键。
                    遍历 "items" 数组：使用 EnumerateArray 方法遍历 "items" 数组。
                    检查是否包含 "item" 键：使用 TryGetProperty 方法检查每个 itemElement 是否包含 "item" 键。
                    解析内部的 JSON 字符串：从 itemElement 中提取内部的 JSON 字符串，并使用 JsonNode.Parse 将其解析为 JsonNode 对象。
                    创建新的 JSON 对象：使用 JsonObject 创建一个新的 JSON 对象，并将 JsonNode 添加到其中。
                    序列化为格式化的字符串：使用 JsonSerializer.Serialize 方法，并传入静态的 JsonSerializerOptions 实例。
                    写入文件：将格式化的字符串写入文件。
                    添加错误处理：在 try-catch 块中捕获并处理可能的异常。

     */
	#endregion
}
