// シリアルポート＆ドライブの抜き差し通知
// Copyright (c) tomozh
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        /// DBT_DEVTYP_PORT イベント
        /// </summary>
        /// <param name="isArrival">true:追加 / false:削除</param>
        /// <param name="m"></param>
        private void onEvent_DBT_DEVTYP_PORT(bool isAdded, Message m)
        {
            var port = (DEV_BROADCAST_PORT)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_PORT));

            if (isAdded)
            {
                if (Properties.Settings.Default.isPopupPortAdded)
                {
                    notifyIcon.BalloonTipTitle = "シリアルポートが接続されました";
                    notifyIcon.BalloonTipText = port.dbcp_name;
                    notifyIcon.ShowBalloonTip(getBalloonTimeout());
                }
            }
            else
            {
                if (Properties.Settings.Default.isPopupPortRemoved)
                {
                    notifyIcon.BalloonTipTitle = "シリアルポートが取り外されました";
                    notifyIcon.BalloonTipText = port.dbcp_name;
                    notifyIcon.ShowBalloonTip(getBalloonTimeout());
                }
            }
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
    }
}
