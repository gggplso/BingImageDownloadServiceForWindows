using ClassLibrary.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

            }

            // 获取下载图片相关参数配置文件信息
            ClassLibrary.Classes.BingImageSettingClass bingSetting = ClassLibrary.ShareClass.GetBingImageSettingClass();
            if (bingSetting != null)
            {

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
    }
}
