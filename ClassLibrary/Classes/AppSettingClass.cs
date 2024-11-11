using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes
{
	public class AppSettingClass
	{
		/// <summary>
		/// 配置文件及路径
		/// </summary>
		public string MainSettingFile { get; set; }
		/// <summary>
		/// 日志保存目录
		/// </summary>
		public string LogPath { get; set; }
		/// <summary>
		/// 默认日志类型
		/// </summary>
		public string LogType { get; set; }
		/// <summary>
		/// 日志新建周期
		/// </summary>
		public string LogCycle { get; set; }
		/// <summary>
		/// 调试日志开关
		/// </summary>
		public bool LogIsDebug { get; set; }
		/// <summary>
		/// 首次服务启动后等待时间再执行任务
		/// </summary>
		public int ServiceRunningFirstWaitTime { get; set; }
		/// <summary>
		/// 服务运行间隔等待时间
		/// </summary>
		public int ServiceRunningWaitTime { get; set; }
		/// <summary>
		/// 指定循环记录日志次数
		/// </summary>
		public int LogCounter { get; set; }
		/// <summary>
		/// 必应每日壁纸下载服务开关
		/// </summary>
		public bool BingImageDownloadService { get; set; }
		/// <summary>
		/// Windows聚集图片复制服务开关
		/// </summary>
		public bool WindowsSpotlightCopyService { get; set; }
		/// <summary>
		/// 文件分类归档服务开关
		/// </summary>
		public bool CategorizeAndMoveFileService { get; set; }
		/// <summary>
		/// 网络连接测试服务开关
		/// </summary>
		public bool NetworkStateTestService { get; set; }
		/// <summary>
		/// Windows聚集API图片下载服务开关
		/// </summary>
		public bool WindowsSpotlightDownloadService { get; set; }
		/// <summary>
		/// 数据库文件存储路径
		/// </summary>
		public string SqliteDataPath { get; set; }
	}
}
