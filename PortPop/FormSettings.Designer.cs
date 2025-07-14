namespace PortPop
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnLuncherFileDialog = new System.Windows.Forms.Button();
            this.tbLuncherExePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLuncherArgument = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbLuncherEnable = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(470, 356);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 31);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK(&O)";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(564, 356);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 31);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "閉じる(&C)";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnLuncherFileDialog
            // 
            this.btnLuncherFileDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLuncherFileDialog.Location = new System.Drawing.Point(552, 75);
            this.btnLuncherFileDialog.Name = "btnLuncherFileDialog";
            this.btnLuncherFileDialog.Size = new System.Drawing.Size(88, 31);
            this.btnLuncherFileDialog.TabIndex = 3;
            this.btnLuncherFileDialog.Text = "参照(&R)";
            this.btnLuncherFileDialog.UseVisualStyleBackColor = true;
            this.btnLuncherFileDialog.Click += new System.EventHandler(this.btnLuncherFileDialog_Click);
            // 
            // tbLuncherExePath
            // 
            this.tbLuncherExePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLuncherExePath.Location = new System.Drawing.Point(27, 80);
            this.tbLuncherExePath.Name = "tbLuncherExePath";
            this.tbLuncherExePath.Size = new System.Drawing.Size(519, 22);
            this.tbLuncherExePath.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "実行ファイル:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "引数:";
            // 
            // tbLuncherArgument
            // 
            this.tbLuncherArgument.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLuncherArgument.Location = new System.Drawing.Point(27, 134);
            this.tbLuncherArgument.Name = "tbLuncherArgument";
            this.tbLuncherArgument.Size = new System.Drawing.Size(519, 22);
            this.tbLuncherArgument.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Location = new System.Drawing.Point(27, 182);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(523, 94);
            this.label3.TabIndex = 6;
            this.label3.Text = "引数の中に変数を指定すると、選択したシリアルポートの情報に置き換えられます。\r\n\r\n%D ... ポート番号 (ex. 1,2,3, ...)\r\n%P ... ポ" +
    "ート名 (ex. COM1, COM2, ...)\r\n%F ... フレンドリ名 (ex. Silicon Labs CP210x ...)";
            // 
            // cbLuncherEnable
            // 
            this.cbLuncherEnable.AutoSize = true;
            this.cbLuncherEnable.Location = new System.Drawing.Point(27, 30);
            this.cbLuncherEnable.Name = "cbLuncherEnable";
            this.cbLuncherEnable.Size = new System.Drawing.Size(59, 19);
            this.cbLuncherEnable.TabIndex = 0;
            this.cbLuncherEnable.Text = "有効";
            this.cbLuncherEnable.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbLuncherEnable);
            this.groupBox1.Controls.Add(this.btnLuncherFileDialog);
            this.groupBox1.Controls.Add(this.tbLuncherExePath);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbLuncherArgument);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(650, 320);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ランチャー";
            // 
            // FormSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(674, 410);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FormSettings";
            this.Text = "設定";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLuncherFileDialog;
        private System.Windows.Forms.TextBox tbLuncherExePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLuncherArgument;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbLuncherEnable;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}