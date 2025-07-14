using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortPop
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            cbLuncherEnable.Checked = Properties.Settings.Default.isLuncherEnable;
            tbLuncherExePath.Text = Properties.Settings.Default.LuncherExePath;
            tbLuncherArgument.Text = Properties.Settings.Default.LuncherArgument;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.isLuncherEnable = cbLuncherEnable.Checked;
            Properties.Settings.Default.LuncherExePath = tbLuncherExePath.Text;
            Properties.Settings.Default.LuncherArgument = tbLuncherArgument.Text;
            Properties.Settings.Default.Save();
        }

        private void btnLuncherFileDialog_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                FileName = tbLuncherExePath.Text,
                Title = "ファイルを選択してください",
                Filter = "実行ファイル (*.exe)|*.exe|全て (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tbLuncherExePath.Text = dialog.FileName;
            }
        }
    }
}
