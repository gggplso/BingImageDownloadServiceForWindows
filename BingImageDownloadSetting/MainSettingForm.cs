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
			ClassLibrary.Classes.BingImageSettingClass bingSetting = ClassLibrary.ShareClass.GetBingImageSettingClass();
			if (bingSetting != null)
			{
				comboBoxNetworkStateTestSwitch.SelectedIndex = bingSetting.NetworkStateTestSwitch ? 0 : 1;
				comboBoxBingImageDownloadSwitch.SelectedIndex = bingSetting.BingImageDownloadSwitch ? 0 : 1;
				comboBoxWindowsSpotlightCopySwitch.SelectedIndex = bingSetting.WindowsSpotlightCopySwitch ? 0 : 1;
				comboBoxWindowsSpotlightDownloadSwitch.SelectedIndex = bingSetting.WindowsSpotlightDownloadSwitch ? 0 : 1;
				comboBoxCategorizeAndMoveFileSwitch.SelectedIndex = bingSetting.CategorizeAndMoveFileSwitch ? 0 : 1;
				numericUpDownAppAutoExitWaitTime.Value = bingSetting.AppAutoExitWaitTime;
				comboBoxOverwrite.SelectedIndex = bingSetting.Overwrite ? 0 : 1;
				textBoxImageFileSavePath.Text = bingSetting.ImageFileSavePath;
				textBoxDnsServerAddress.Text = bingSetting.NetworkInformation.DnsServerAddress;
				numericUpDownSocketSendTimeout.Value = bingSetting.NetworkInformation.SocketSendTimeout;
				numericUpDownSocketReceiveTimeout.Value = bingSetting.NetworkInformation.SocketReceiveTimeout;
				textBoxPingReplyDomain.Text = bingSetting.NetworkInformation.PingReplyDomain;

			}
		}
		/// <summary>
		/// 保存按钮
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonSave_Click(object sender, EventArgs e)
		{

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
