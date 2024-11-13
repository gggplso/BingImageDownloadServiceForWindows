using ClassLibrary;
using ClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BingImageDownloadSetting
{
	public partial class FormSettingMain : Form
	{
		public FormSettingMain()
		{
			InitializeComponent();
		}
		/// <summary>
		/// 程序打开窗口时加载配置项
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormSettingMain_Load(object sender, EventArgs e)
		{
			// 日志循环设定
			if (comboBoxLogCycle.Items.Count == 0)
			{
				foreach (var cycleOption in ClassLibrary.ShareClass.LogCycleConverter.CycleOptions)
				{
					comboBoxLogCycle.Items.Add(cycleOption.DisplayText);
				}
			}
			// 文件归档移动类型
			if (comboBoxFileOperationType.Items.Count == 0)
			{
				var fileOperation = Enum.GetValues(typeof(ClassLibrary.ShareClass.FileOperationType))
									.Cast<object>()
									.ToArray();
				comboBoxFileOperationType.Items.AddRange(fileOperation);
			}
		}
		/// <summary>
		/// 读取按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonRead_Click(object sender, EventArgs e)
		{
			try
			{

				// 打开配置参数的窗口时，判断配置文件是否存在，若不存在则初始化生成配置文件
				// 主配置文件
				if (!File.Exists(ClassLibrary.ShareClass._mainSettingFile))
				{
					ClassLibrary.ShareClass.ReadConfig();
				}
				// 图片下载配置文件
				if (!File.Exists(ClassLibrary.ShareClass._bingImageSettingFile))
				{
					ClassLibrary.Services.BingImageDownloadService bingImageDownloadService = new ClassLibrary.Services.BingImageDownloadService();
				}

				// 获取主程序相关参数配置文件信息
				ClassLibrary.Classes.AppSettingClass appSetting = ClassLibrary.ShareClass.GetAppSettingClass();
				if (appSetting != null)
				{
					textBoxMainSettingFile.Text = appSetting.MainSettingFile;
					textBoxLogPath.Text = appSetting.LogPath;
					textBoxLogType.Text = appSetting.LogType;
					// 日志循环设定("yyyyMMdd")
					string logCycle = appSetting.LogCycle;
					logCycle = ClassLibrary.ShareClass.LogCycleConverter.VerifyValidFormatString(logCycle);
					comboBoxLogCycle.Text = ClassLibrary.ShareClass.LogCycleConverter.ConvertToText(logCycle);
					numericUpDownServiceRunningFirstWaitTime.Value = appSetting.ServiceRunningFirstWaitTime;
					numericUpDownServiceRunningWaitTime.Value = appSetting.ServiceRunningWaitTime;
					numericUpDownLogCounter.Value = appSetting.LogCounter;
					comboBoxNetworkStateTestService.SelectedIndex = appSetting.NetworkStateTestService ? 0 : 1;
					comboBoxBingImageDownloadService.SelectedIndex = appSetting.BingImageDownloadService ? 0 : 1;
					comboBoxWindowsSpotlightCopyService.SelectedIndex = appSetting.WindowsSpotlightCopyService ? 0 : 1;
					comboBoxWindowsSpotlightDownloadService.SelectedIndex = appSetting.WindowsSpotlightDownloadService ? 0 : 1;
					comboBoxCategorizeAndMoveFileService.SelectedIndex = appSetting.CategorizeAndMoveFileService ? 0 : 1;
					textBoxSqliteDataPath.Text = appSetting.SqliteDataPath;
					comboBoxLogIsDebug.SelectedIndex = appSetting.LogIsDebug ? 0 : 1;
				}

				// 获取下载图片相关参数配置文件信息
				ClassLibrary.Classes.BingImageSettingClass bingImageSetting = ClassLibrary.ShareClass.GetBingImageSettingClass();
				if (bingImageSetting != null)
				{
					comboBoxNetworkStateTestSwitch.SelectedIndex = bingImageSetting.NetworkStateTestSwitch ? 0 : 1;
					comboBoxBingImageDownloadSwitch.SelectedIndex = bingImageSetting.BingImageDownloadSwitch ? 0 : 1;
					comboBoxWindowsSpotlightCopySwitch.SelectedIndex = bingImageSetting.WindowsSpotlightCopySwitch ? 0 : 1;
					comboBoxWindowsSpotlightDownloadSwitch.SelectedIndex = bingImageSetting.WindowsSpotlightDownloadSwitch ? 0 : 1;
					comboBoxCategorizeAndMoveFileSwitch.SelectedIndex = bingImageSetting.CategorizeAndMoveFileSwitch ? 0 : 1;
					numericUpDownAppAutoExitWaitTime.Value = bingImageSetting.AppAutoExitWaitTime;
					comboBoxOverwrite.SelectedIndex = bingImageSetting.Overwrite ? 0 : 1;
					textBoxImageFileSavePath.Text = bingImageSetting.ImageFileSavePath;
					textBoxDnsServerAddress.Text = bingImageSetting.NetworkInformation.DnsServerAddress;
					numericUpDownSocketSendTimeout.Value = bingImageSetting.NetworkInformation.SocketSendTimeout;
					numericUpDownSocketReceiveTimeout.Value = bingImageSetting.NetworkInformation.SocketReceiveTimeout;
					textBoxPingReplyDomain.Text = bingImageSetting.NetworkInformation.PingReplyDomain;
					numericUpDownNetRetryCount.Value = bingImageSetting.NetworkInformation.NetRetryCount;
					numericUpDownNetWaitTime.Value = bingImageSetting.NetworkInformation.NetWaitTime;
					numericUpDownBingUrlDaysAgo.Value = bingImageSetting.BingImageApi.BingApiRequestParams.Query3Value;
					numericUpDownBingUrlAFewDays.Value = bingImageSetting.BingImageApi.BingApiRequestParams.Query4Value;
					comboBoxPixelResolutionIsUHD.SelectedIndex = bingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsUHD ? 0 : 1;
					comboBoxPixelResolutionIsMobile.SelectedIndex = bingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsMobile ? 0 : 1;
					comboBoxFileNameLanguageIsChinese.SelectedIndex = bingImageSetting.BingImageApi.BingApiRequestParams.FileNameLanguageIsChinese ? 0 : 1;
					comboBoxFileNameAddTitle.SelectedIndex = bingImageSetting.BingImageApi.BingApiRequestParams.FileNameAddTitle ? 0 : 1;
					comboBoxFileNameAddDate.SelectedIndex = bingImageSetting.BingImageApi.BingApiRequestParams.FileNameAddDate ? 0 : 1;
					comboBoxIsFullDownload.SelectedIndex = bingImageSetting.BingImageApi.BingApiRequestParams.IsFullDownload ? 0 : 1;
					textBoxWindowsSpotlightPath.Text = bingImageSetting.WindowsSpotlightSetting.WindowsSpotlightPath;
					numericUpDownWindowsSpotlightAPIRepeatLimit.Value = bingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIRepeatLimit;
					comboBoxFileOperationType.SelectedIndex = (int)bingImageSetting.CategorizeAndMoveSetting.FileOperationType;
					textBoxSearchDirectoryPath.Text = bingImageSetting.CategorizeAndMoveSetting.SearchDirectoryPath;
					textBoxComputerWallpaperPath.Text = bingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath;
					textBoxMobileWallpaperPath.Text = bingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath;
					textBoxVideoWallpaperPath.Text = bingImageSetting.CategorizeAndMoveSetting.VideoWallpaperPath;
					textBoxRejectedPath.Text = bingImageSetting.CategorizeAndMoveSetting.RejectedPath;
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				MessageBox.Show($"是否因为手动改了相关文件？可以删除配置文件重新生成，或根据下方提示修改回原值。\r\n{ex.Message}", "读取配置文件失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		/// <summary>
		/// 保存按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			ClassLibrary.Classes.AppSettingClass appSetting = new ClassLibrary.Classes.AppSettingClass();
			if (MyJsonHelper.WriteClassToJsonFile<ClassLibrary.Classes.AppSettingClass>(appSetting, ))
			{
				appSetting.MainSettingFile = textBoxMainSettingFile.Text;
				appSetting.LogPath = textBoxLogPath.Text;
				appSetting.LogType = textBoxLogType.Text;
				// 日志循环设定("yyyyMMdd")
				string logCycle = ClassLibrary.ShareClass.LogCycleConverter.GetFormatStringFromText(comboBoxLogCycle.Text);
				logCycle = ClassLibrary.ShareClass.LogCycleConverter.VerifyValidFormatString(logCycle);
				appSetting.LogCycle = logCycle;
				appSetting.ServiceRunningFirstWaitTime = (int)numericUpDownServiceRunningFirstWaitTime.Value;
				appSetting.ServiceRunningWaitTime = (int)numericUpDownServiceRunningWaitTime.Value;
				appSetting.LogCounter = (int)numericUpDownLogCounter.Value;
				appSetting.NetworkStateTestService = comboBoxNetworkStateTestService.SelectedIndex == 0;
				appSetting.BingImageDownloadService = comboBoxBingImageDownloadService.SelectedIndex == 0;
				appSetting.WindowsSpotlightCopyService = comboBoxWindowsSpotlightCopyService.SelectedIndex == 0;
				appSetting.WindowsSpotlightDownloadService = comboBoxWindowsSpotlightDownloadService.SelectedIndex == 0;
				appSetting.CategorizeAndMoveFileService = comboBoxCategorizeAndMoveFileService.SelectedIndex == 0;
				appSetting.SqliteDataPath = textBoxSqliteDataPath.Text;
				appSetting.LogIsDebug = comboBoxLogIsDebug.SelectedIndex == 0;
			}

			ClassLibrary.Classes.BingImageSettingClass bingImageSetting = new ClassLibrary.Classes.BingImageSettingClass();
			if (ClassLibrary.MyJsonHelper.WriteClassToJsonFile<ClassLibrary.Classes.BingImageSettingClass>(bingImageSetting, ))
			{
				bingImageSetting.NetworkStateTestSwitch = comboBoxNetworkStateTestSwitch.SelectedIndex == 0;
				bingImageSetting.BingImageDownloadSwitch = comboBoxBingImageDownloadSwitch.SelectedIndex == 0;
				bingImageSetting.WindowsSpotlightCopySwitch = comboBoxWindowsSpotlightCopySwitch.SelectedIndex == 0;
				bingImageSetting.WindowsSpotlightDownloadSwitch = comboBoxWindowsSpotlightDownloadSwitch.SelectedIndex == 0;
				bingImageSetting.CategorizeAndMoveFileSwitch = comboBoxCategorizeAndMoveFileSwitch.SelectedIndex == 0;
				bingImageSetting.AppAutoExitWaitTime = (int)numericUpDownAppAutoExitWaitTime.Value;
				bingImageSetting.Overwrite = comboBoxOverwrite.SelectedIndex == 0;
				bingImageSetting.ImageFileSavePath = textBoxImageFileSavePath.Text;
				bingImageSetting.NetworkInformation.DnsServerAddress = textBoxDnsServerAddress.Text;
				bingImageSetting.NetworkInformation.SocketSendTimeout = (int)numericUpDownSocketSendTimeout.Value;
				bingImageSetting.NetworkInformation.SocketReceiveTimeout = (int)numericUpDownSocketReceiveTimeout.Value;
				bingImageSetting.NetworkInformation.PingReplyDomain = textBoxPingReplyDomain.Text;
				bingImageSetting.NetworkInformation.NetRetryCount = (int)numericUpDownNetRetryCount.Value;
				bingImageSetting.NetworkInformation.NetWaitTime = (int)numericUpDownNetWaitTime.Value;
				bingImageSetting.BingImageApi.BingApiRequestParams.Query3Value = (int)numericUpDownBingUrlDaysAgo.Value;
				bingImageSetting.BingImageApi.BingApiRequestParams.Query4Value = (int)numericUpDownBingUrlAFewDays.Value;
				bingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsUHD = comboBoxPixelResolutionIsUHD.SelectedIndex == 0;
				bingImageSetting.BingImageApi.BingApiRequestParams.PixelResolutionIsMobile = comboBoxPixelResolutionIsMobile.SelectedIndex == 0;
				bingImageSetting.BingImageApi.BingApiRequestParams.FileNameLanguageIsChinese = comboBoxFileNameLanguageIsChinese.SelectedIndex == 0;
				bingImageSetting.BingImageApi.BingApiRequestParams.FileNameAddTitle = comboBoxFileNameAddTitle.SelectedIndex == 0;
				bingImageSetting.BingImageApi.BingApiRequestParams.FileNameAddDate = comboBoxFileNameAddDate.SelectedIndex == 0;
				bingImageSetting.BingImageApi.BingApiRequestParams.IsFullDownload = comboBoxIsFullDownload.SelectedIndex == 0;
				bingImageSetting.WindowsSpotlightSetting.WindowsSpotlightPath = textBoxWindowsSpotlightPath.Text;
				bingImageSetting.WindowsSpotlightSetting.WindowsSpotlightAPIRepeatLimit = (int)numericUpDownWindowsSpotlightAPIRepeatLimit.Value;
				bingImageSetting.CategorizeAndMoveSetting.FileOperationType = (ClassLibrary.ShareClass.FileOperationType)comboBoxFileOperationType.SelectedIndex;
				bingImageSetting.CategorizeAndMoveSetting.SearchDirectoryPath = textBoxSearchDirectoryPath.Text;
				bingImageSetting.CategorizeAndMoveSetting.ComputerWallpaperPath = textBoxComputerWallpaperPath.Text;
				bingImageSetting.CategorizeAndMoveSetting.MobileWallpaperPath = textBoxMobileWallpaperPath.Text;
				bingImageSetting.CategorizeAndMoveSetting.VideoWallpaperPath = textBoxVideoWallpaperPath.Text;
				bingImageSetting.CategorizeAndMoveSetting.RejectedPath = textBoxRejectedPath.Text;
			}
		}
		/// <summary>
		/// 双击Tab页签时，展现/隐藏 部分选项参数
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tabControlSettingMain_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			// SQL数据文件存放路径：
			labelSqliteDataPath.Visible = labelSqliteDataPath.Visible == true ? false : true;
			textBoxSqliteDataPath.Visible = textBoxSqliteDataPath.Visible == true ? false : true;
			buttonSqliteDataPathBrowse.Visible = buttonSqliteDataPathBrowse.Visible == true ? false : true;
			labelSqliteDataPathExplanation.Visible = labelSqliteDataPathExplanation.Visible == true ? false : true;

			// 调试日志显示开关：
			labelLogIsDebug.Visible = labelLogIsDebug.Visible == true ? false : true;
			comboBoxLogIsDebug.Visible = comboBoxLogIsDebug.Visible == true ? false : true;
			labelLogIsDebugUnit.Visible = labelLogIsDebugUnit.Visible == true ? false : true;
			labelLogIsDebugExplanation.Visible = labelLogIsDebugExplanation.Visible == true ? false : true;
		}
	}
}
