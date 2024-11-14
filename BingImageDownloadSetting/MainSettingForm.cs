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
		/// 递归获取所有指定类型的控件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parent"></param>
		/// <returns></returns>
		private IEnumerable<T> GetAllControls<T>(Control parent) where T : Control
		{
			var controls = new List<T>();

			foreach (Control control in parent.Controls)
			{
				if (control is T)
				{
					controls.Add((T)control);
				}

				if (control.HasChildren)
				{
					controls.AddRange(GetAllControls<T>(control));
				}
			}

			return controls;
		}
		/// <summary>
		/// 设置所有数字控件的最小值
		/// </summary>
		private void SetDefaultMinimumValues()
		{
			// 获取所有 NumericUpDown 控件
			var numericUpDownes = GetAllControls<NumericUpDown>(this);

			foreach (var numericUpDown in numericUpDownes)
			{
				numericUpDown.Value = numericUpDown.Minimum;
			}
		}
		/// <summary>
		/// 设置所有选项框控件的默认选项(值)
		/// </summary>
		private void SetDefaultSelectedValues()
		{
			//// 获取所有 ComboBox 控件
			//var comboBoxes = this.Controls.OfType<ComboBox>();
			///*
			//    如果你发现 var comboBoxes = this.Controls.OfType<ComboBox>(); 
			//    没有获取到任何 ComboBox 控件，可能是因为这些控件嵌套在其他容器控件（如 Panel、GroupBox 等）中。
			//    在这种情况下，你需要递归地遍历所有容器控件来获取所有的 ComboBox 控件。

			//    递归获取所有 ComboBox 控件
			//    你可以编写一个递归方法来遍历所有控件及其子控件，从而获取所有的 ComboBox 控件。
			// */

			// 获取所有 ComboBox 控件
			var comboBoxes = GetAllControls<ComboBox>(this);

			foreach (var comboBox in comboBoxes)
			{
				// 设置初始默认值
				if (comboBox.Items.Count > 0)
				{
					// 选中第一个选项
					comboBox.SelectedIndex = 0;
				}
			}
		}
		/// <summary>
		/// 读取配置文件信息到窗体控件中
		/// </summary>
		private void ReadConfigToControls()
		{
			try
			{
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
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				MessageBox.Show($"是否因为手动改了相关文件？可以删除配置文件重新生成，或根据下方提示修改回原值。\r\n{ex.Message}", "读取服务总配置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			try
			{
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
				MessageBox.Show($"是否因为手动改了相关文件？可以删除配置文件重新生成，或根据下方提示修改回原值。\r\n{ex.Message}", "读取图片下载配置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		/// <summary>
		/// 将窗体控件中信息保存到配置文件
		/// </summary>
		/// <returns></returns>
		private bool ControlsToSaveConfig()
		{
			bool appSettingResult = false;
			bool bingImageSettingResult = false;

			try
			{
				// 设置主程序相关参数配置
				ClassLibrary.Classes.AppSettingClass appSetting = new ClassLibrary.Classes.AppSettingClass();

				//appSetting.MainSettingFile = textBoxMainSettingFile.Text;
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
				if (MyJsonHelper.WriteClassToJsonFile<ClassLibrary.Classes.AppSettingClass>(appSetting, ClassLibrary.ShareClass._mainSettingFile))
				{
					appSettingResult = true;
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				MessageBox.Show($"保存时发生了错误，请检查填写的内容。\r\n{ex.Message}", "服务总配置保存文件失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			try
			{
				// 设置下载图片相关参数配置
				ClassLibrary.Classes.BingImageSettingClass bingImageSetting = new ClassLibrary.Classes.BingImageSettingClass();

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
				if (ClassLibrary.MyJsonHelper.WriteClassToJsonFile<ClassLibrary.Classes.BingImageSettingClass>(bingImageSetting, ClassLibrary.ShareClass._bingImageSettingFile))
				{
					bingImageSettingResult = true;
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				MessageBox.Show($"保存时发生了错误，请检查填写的内容。\r\n{ex.Message}", "图片下载配置文件保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return (appSettingResult && bingImageSettingResult);
		}
		/// <summary>
		/// 浏览选择文件夹目录(通用方法)
		/// </summary>
		/// <param name="initialPath">默认文件夹路径</param>
		/// <param name="description">提示说明</param>
		/// <param name="textBox">将选择的文件夹所在路径值返回控件</param>
		/// <param name="fileName">可选：返回的值加带文件名</param>
		private void BrowseFolder(string initialPath, string description, TextBox textBox, string fileName = null)
		{
			try
			{
				using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
				{
					folderBrowserDialog.SelectedPath = initialPath;
					folderBrowserDialog.Description = description;
					folderBrowserDialog.ShowNewFolderButton = true;
					if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
					{
						textBox.Text = Path.Combine(folderBrowserDialog.SelectedPath, fileName ?? "");
					}
				}
			}
			catch (Exception ex)
			{
				ClassLibrary.MyLogHelper.GetExceptionLocation(ex);
				MessageBox.Show($"出现错误：请将本信息连同日志文件一起反馈。\r\n{ex.Message}", "选择文件夹失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		/// <summary>
		/// 程序打开窗口时加载配置项
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormSettingMain_Load(object sender, EventArgs e)
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

			// 打开就读取配置，所以不用设定默认值

			//// 默认值设定
			//SetDefaultSelectedValues();
			//SetDefaultMinimumValues();

			ReadConfigToControls();

		}
		/// <summary>
		/// 读取按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonRead_Click(object sender, EventArgs e)
		{
			ReadConfigToControls();
		}
		/// <summary>
		/// 保存按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			if (ControlsToSaveConfig())
			{
				MessageBox.Show($"{ClassLibrary.ShareClass._mainSettingFile}\r\n{ClassLibrary.ShareClass._bingImageSettingFile}", "保存成功");
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
		/// <summary>
		/// 是否全量下载选项变更触发
		/// 全量下载意味着自定义下载天数和数量的配置不起作用
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void comboBoxIsFullDownload_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxIsFullDownload.SelectedIndex == 0)
			{
				numericUpDownBingUrlAFewDays.Enabled = false;
				numericUpDownBingUrlDaysAgo.Enabled = false;
			}
			else
			{
				numericUpDownBingUrlAFewDays.Enabled = true;
				numericUpDownBingUrlDaysAgo.Enabled = true;
			}
		}
		/// <summary>
		/// 浏览设定：日志文件所在路径
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonLogPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(ClassLibrary.ShareClass._logPath, "请选择一个文件夹作为保存日志文件的目录", textBoxLogPath);
		}
		/// <summary>
		/// 浏览设定：SQL数据文件存放路径
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonSqliteDataPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(ClassLibrary.ShareClass._sqliteDataPath, "请选择一个文件夹作为保存数据文件的目录", textBoxSqliteDataPath);
		}
		/// <summary>
		/// 浏览设定：需整理图片所在目录源路径、【待整理图片】保存目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonImageFileSavePathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._userPath ?? "D:", "Pictures", "Downloads"), "请选择一个目录作为保存【待整理分类】图片的文件夹", textBoxImageFileSavePath);
		}
		/// <summary>
		/// 浏览选择：Windows系统聚焦图片所在的目录路径
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonWindowsSpotlightPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._systemUserProfilePath, "AppData", "Local", "Packages", "Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy", "LocalState", "Assets"), "请选择Windows系统聚焦图片所在的目录路径", textBoxWindowsSpotlightPath);
		}
		/// <summary>
		/// 浏览选择：需整理图片所在目录源路径、【待整理图片】保存目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonSearchDirectoryPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._userPath ?? "D:", "Pictures", "Downloads"), "请选择一个需要整理分类归档的【待整理图片】所在文件夹目录", textBoxSearchDirectoryPath);
		}
		/// <summary>
		/// 浏览设定：电脑壁纸存储目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonComputerWallpaperPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._userPath ?? "D:", "Pictures", "Computer"), "请选择一个目录作为保存【电脑壁纸】图片的文件夹", textBoxComputerWallpaperPath);
		}
		/// <summary>
		/// 浏览设定：手机壁纸存储目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonMobileWallpaperPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._userPath ?? "D:", "Pictures", "Mobile"), "请选择一个目录作为保存【手机壁纸】图片的文件夹", textBoxMobileWallpaperPath);
		}
		/// <summary>
		/// 浏览设定：视频文件存储目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonVideoWallpaperPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._userPath ?? "D:", "Pictures", "Landscape"), "请选择一个目录作为保存视频的文件夹", textBoxVideoWallpaperPath);
		}
		/// <summary>
		/// 浏览设定：无用文件的丢弃目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonRejectedPathBrowse_Click(object sender, EventArgs e)
		{
			BrowseFolder(Path.Combine(ClassLibrary.ShareClass._userPath ?? "D:", "Pictures", "Rejected"), "请选择一个目录作为存放选择文件的文件夹", textBoxRejectedPath);
		}
	}
}
