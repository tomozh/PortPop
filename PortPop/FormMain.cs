// シリアルポート＆ドライブの抜き差し通知
// Copyright (c) tomozh
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortPop
{
    public partial class FormMain : Form
    {
        private const int WM_DEVICECHANGE = 0x0219;

        private const int DBT_DEVICEARRIVAL = 0x8000;  // system detected a new device
        private const int DBT_DEVICEQUERYREMOVE = 0x8001;  // wants to remove, may fail
        private const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;  // removal aborted
        private const int DBT_DEVICEREMOVEPENDING = 0x8003;  // about to remove, still avail.
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;  // device is gone
        private const int DBT_DEVICETYPESPECIFIC = 0x8005;  // type specific event

        private const int DBT_DEVTYP_OEM = 0x00000000;  // oem-defined device type
        private const int DBT_DEVTYP_DEVNODE = 0x00000001;  // devnode number
        private const int DBT_DEVTYP_VOLUME = 0x00000002;  // logical volume
        private const int DBT_DEVTYP_PORT = 0x00000003;  // serial, parallel
        private const int DBT_DEVTYP_NET = 0x00000004;  // network resource

        private const int DBTF_MEDIA = 0x0001;  // Change affects media in drive. If not set, change affects physical device or drive.
        private const int DBTF_NET = 0x0002;    // Indicated logical volume is a network volume.

        // http://www.pinvoke.net/default.aspx
        [StructLayout(LayoutKind.Sequential)]
        struct DEV_BROADCAST_HDR
        {
            public uint dbch_Size;
            public uint dbch_DeviceType;
            public uint dbch_Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct DEV_BROADCAST_PORT
        {
            public int dbcp_size;
            public int dbcp_devicetype;
            public int dbcp_reserved; // MSDN say "do not use"
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public String dbcp_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DEV_BROADCAST_VOLUME
        {
            public uint dbch_Size;
            public uint dbch_Devicetype;
            public uint dbch_Reserved;
            public uint dbch_Unitmask;
            public ushort dbch_Flags;
        }

        /// <summary>
        /// シリアルポートの情報
        /// </summary>
        public class SerialPortInfo
        {
            /// <summary>
            /// シリアルポート番号
            /// </summary>
            public int PortNumber { get; set; }

            /// <summary>
            /// シリアルポート名 (ex.COM1, COM2, ...)
            /// </summary>
            public string PortName { get; set; }

            /// <summary>
            /// シリアルポートのフレンドリ名 (ex.Silicon Labs CP10x USB ...)
            /// </summary>
            public string FriendlyName { get; set; }
        }

        /// <summary>
        /// 接続済みのポート
        /// </summary>
        private Dictionary<string, SerialPortInfo> _connectedPort = new Dictionary<string, SerialPortInfo>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            notifyPortAddedToolStripMenuItem.Checked = Properties.Settings.Default.isPopupPortAdded;
            notifyPortRemovedToolStripMenuItem.Checked = Properties.Settings.Default.isPopupPortRemoved;
            notifyDriveAddedToolStripMenuItem.Checked = Properties.Settings.Default.isPopupDriveAdded;
            notifyDriveRemovedToolStripMenuItem.Checked = Properties.Settings.Default.isPopupDriveRemoved;
        }

        /// <summary>
        /// フォームの非表示化
        /// </summary>
        protected override CreateParams CreateParams
        {
            [System.Security.Permissions.SecurityPermission(
                System.Security.Permissions.SecurityAction.LinkDemand,
                Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                const int WS_EX_TOOLWINDOW = 0x80;
                const long WS_POPUP = 0x80000000L;
                const int WS_VISIBLE = 0x10000000;
                const int WS_SYSMENU = 0x80000;
                const int WS_MAXIMIZEBOX = 0x10000;

                var cp = base.CreateParams;

                cp.ExStyle = WS_EX_TOOLWINDOW;
                cp.Style = unchecked((int)WS_POPUP) | WS_VISIBLE | WS_SYSMENU | WS_MAXIMIZEBOX;
                cp.Width = 0;
                cp.Height = 0;

                return cp;
            }
        }

        /// <summary>
        /// ウィンドウメッセージの処理
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_DEVICECHANGE)
            {
                if (((int)m.WParam == DBT_DEVICEARRIVAL) || ((int)m.WParam == DBT_DEVICEREMOVECOMPLETE))
                {
                    var hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_HDR));
                    bool isAdded = ((int)m.WParam == DBT_DEVICEARRIVAL);

                    switch (hdr.dbch_DeviceType)
                    {
                        case DBT_DEVTYP_VOLUME:
                            onEvent_DBT_DEVTYP_VOLUME(isAdded, m);
                            break;

                        case DBT_DEVTYP_PORT:
                            onEvent_DBT_DEVTYP_PORT(isAdded, m);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// バルーン表示時間を返す
        /// </summary>
        /// <returns>バルーン表示時間(ms)</returns>
        private int getBalloonTimeout()
        {
            return Properties.Settings.Default.balloonTimeout * 1000;
        }

        /// <summary>
        /// 接続されているシリアルポートを列挙
        /// </summary>
        /// <returns>接続されているシリアルポート</returns>
        private List<SerialPortInfo> querySerialPorts()
        {
            var ports = new List<SerialPortInfo>();

            string query = $"SELECT * FROM Win32_PnPEntity WHERE PNPClass='Ports'";

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (var device in searcher.Get())
                {
                    string friendlyName = device["Name"]?.ToString();
                    var match = Regex.Match(friendlyName, @"\(COM(\d+)\)$");

                    if (match.Success)
                    {
                        int portNumber = int.Parse(match.Groups[1].Value);

                        ports.Add(new SerialPortInfo
                        {
                            PortNumber = portNumber,
                            PortName = $"COM{portNumber}",
                            FriendlyName = friendlyName,
                        });
                    }
                }
            }

            return ports;
        }

        /// <summary>
        /// シリアルポート名からSerialPortInfoを取得
        /// </summary>
        /// <param name="portName">シリアルポート名</param>
        /// <returns>SerialPortInfo</returns>
        private SerialPortInfo getSerialPortInfo(string portName)
        {
            string query = $"SELECT * FROM Win32_PnPEntity WHERE (PNPClass='Ports') AND (Name LIKE '%({portName})%')";

            // 取れなかった場合のリトライ
            for (int i = 0; i < 3; i++)
            {
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (var device in searcher.Get())
                    {
                        string friendlyName = device["Name"]?.ToString();

                        if (!string.IsNullOrEmpty(friendlyName))
                        {
                            var match = Regex.Match(friendlyName, @"\(COM(\d+)\)$");

                            if (match.Success)
                            {
                                return new SerialPortInfo
                                {
                                    PortNumber = int.Parse(match.Groups[1].Value),
                                    PortName = portName,
                                    FriendlyName = friendlyName,
                                };
                            }
                        }
                    }
                }

                Thread.Sleep(100);
            }

            return null;

        }

        /// <summary>
        /// DBT_DEVTYP_PORT イベント
        /// </summary>
        /// <param name="isArrival">true:追加 / false:削除</param>
        /// <param name="m"></param>
        private void onEvent_DBT_DEVTYP_PORT(bool isAdded, Message m)
        {
            var port = (DEV_BROADCAST_PORT)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_PORT));
            string portName = port.dbcp_name;

            Task.Factory.StartNew(() =>
            {
                lock (_connectedPort)
                {
                    notifyIcon.Tag = null;

                    if (isAdded)
                    {
                        // 接続時
                        var info = getSerialPortInfo(portName);

                        if (info != null)
                        {
                            _connectedPort.Remove(portName);
                            _connectedPort.Add(portName, info);

                            if (Properties.Settings.Default.isPopupPortAdded)
                            {
                                notifyIcon.Tag = info;  // クリック時のため
                                notifyIcon.BalloonTipTitle = $"シリアルポートが接続されました";
                                notifyIcon.BalloonTipText = info.FriendlyName;
                                notifyIcon.ShowBalloonTip(getBalloonTimeout());
                            }
                        }
                    }
                    else
                    {
                        // 切断時
                        // フレンドリ名が取れないのでキャッシュから取得する
                        if (_connectedPort.ContainsKey(portName))
                        {
                            var info = _connectedPort[portName];
                            _connectedPort.Remove(portName);

                            if (Properties.Settings.Default.isPopupPortRemoved)
                            {
                                notifyIcon.BalloonTipTitle = "シリアルポートが取り外されました";
                                notifyIcon.BalloonTipText = info.FriendlyName;
                                notifyIcon.ShowBalloonTip(getBalloonTimeout());
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// DBT_DEVTYP_VOLUME イベント
        /// </summary>
        /// <param name="isArrival">true:追加 / false:削除</param>
        /// <param name="m"></param>
        private void onEvent_DBT_DEVTYP_VOLUME(bool isAdded, Message m)
        {
            var vol = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_VOLUME));

            // ドライブ名文字列を作成
            var drives = new List<string>();
            int i;

            for (i = 0; i < 32; i++)
            {
                if ((vol.dbch_Unitmask & (1 << i)) != 0)
                {
                    drives.Add(((char)('A' + i)) + ":");
                }
            }

            notifyIcon.BalloonTipText = string.Join(", ", drives.ToArray());
            notifyIcon.Tag = null;

            if (isAdded)
            {
                if (Properties.Settings.Default.isPopupDriveAdded)
                {
                    notifyIcon.BalloonTipTitle = "ドライブが接続されました";
                    notifyIcon.ShowBalloonTip(getBalloonTimeout());
                }
            }
            else
            {
                if (Properties.Settings.Default.isPopupDriveRemoved)
                {
                    notifyIcon.BalloonTipTitle = "ドライブが取り外されました";
                    notifyIcon.ShowBalloonTip(getBalloonTimeout());
                }
            }
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Visible = false;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// メニュー：終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// メニュー：ポートの追加を通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyPortAddedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            item.Checked = !item.Checked;
            Properties.Settings.Default.isPopupPortAdded = item.Checked;
        }

        /// <summary>
        /// メニュー：ポートの取外しを通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyPortRemovedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            item.Checked = !item.Checked;
            Properties.Settings.Default.isPopupPortRemoved = item.Checked;
        }

        /// <summary>
        /// メニュー：ドライブの追加を通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyDriveAddedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            item.Checked = !item.Checked;
            Properties.Settings.Default.isPopupDriveAdded = item.Checked;
        }

        /// <summary>
        /// メニュー：ドライブの追加を通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyDriveRemovedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            item.Checked = !item.Checked;
            Properties.Settings.Default.isPopupDriveRemoved = item.Checked;
        }

        /// <summary>
        /// メニュー：設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new FormSettings();
            dialog.ShowDialog();
        }

        /// <summary>
        /// ランチャーアプリを実行
        /// </summary>
        /// <param name="info">シリアルポート情報</param>
        private void lunchProcess(SerialPortInfo info)
        {
            if (Properties.Settings.Default.isLuncherEnable)
            {
                string exePath = Properties.Settings.Default.LuncherExePath;
                string arguments = Properties.Settings.Default.LuncherArgument;

                // 変数を置換
                arguments = arguments
                    .Replace("%N", info.PortNumber.ToString())
                    .Replace("%P", info.PortName)
                    .Replace("%F", info.FriendlyName);

                // プロセス情報の設定
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                };

                // プロセス開始
                Process.Start(startInfo);
            }
        }

        /// <summary>
        /// バルーンがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            if (sender is NotifyIcon icon)
            {
                if (icon.Tag is SerialPortInfo info)
                {
                    lunchProcess(info);
                }
            }
        }

        /// <summary>
        /// タスクトレイアイコン上のMouseUp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            // 参考
            // https://omoisan.hatenablog.com/entry/2017/07/15/203838
            // このやり方じゃないと左クリックで出したバルーンが消えない

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // 左クリック
                notifyIcon.ContextMenuStrip = contextMenuStripPortList;

                // 接続中のシリアルポートを取得して番号順にソート
                var items = querySerialPorts().OrderBy(p => p.PortNumber).ToList();

                contextMenuStripPortList.Items.Clear();

                foreach (var item in items)
                {
                    var menu = new ToolStripMenuItem(item.FriendlyName);
                    menu.Click += (_sender, _e) => lunchProcess(item);
                    contextMenuStripPortList.Items.Add(menu);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                // 右クリック
                notifyIcon.ContextMenuStrip = contextMenuStrip;
            }

            if (notifyIcon.ContextMenuStrip != null)
            {
                var method = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                method.Invoke(notifyIcon, null);
                notifyIcon.ContextMenuStrip = null;
            }
        }
    }
}
