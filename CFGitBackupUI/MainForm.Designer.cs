namespace CFGitBackupUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            statusStrip1 = new StatusStrip();
            tsslStatus = new ToolStripStatusLabel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            dgvGitRepoBackupConfig = new DataGridView();
            tabPage2 = new TabPage();
            toolStrip1 = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            backUpNowToolStripMenuItem = new ToolStripMenuItem();
            debugListAllReposToolStripMenuItem = new ToolStripMenuItem();
            cancelBackupsToolStripMenuItem = new ToolStripMenuItem();
            niNotify = new NotifyIcon(components);
            statusStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGitRepoBackupConfig).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsslStatus });
            statusStrip1.Location = new Point(0, 567);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(965, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsslStatus
            // 
            tsslStatus.Name = "tsslStatus";
            tsslStatus.Size = new Size(118, 17);
            tsslStatus.Text = "toolStripStatusLabel1";
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(0, 28);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(965, 536);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(dgvGitRepoBackupConfig);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(957, 508);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Git Repos";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvGitRepoBackupConfig
            // 
            dgvGitRepoBackupConfig.AllowUserToAddRows = false;
            dgvGitRepoBackupConfig.AllowUserToDeleteRows = false;
            dgvGitRepoBackupConfig.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGitRepoBackupConfig.Dock = DockStyle.Fill;
            dgvGitRepoBackupConfig.Location = new Point(3, 3);
            dgvGitRepoBackupConfig.Name = "dgvGitRepoBackupConfig";
            dgvGitRepoBackupConfig.ReadOnly = true;
            dgvGitRepoBackupConfig.RowHeadersVisible = false;
            dgvGitRepoBackupConfig.Size = new Size(951, 502);
            dgvGitRepoBackupConfig.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(957, 508);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Log";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(965, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { backUpNowToolStripMenuItem, debugListAllReposToolStripMenuItem, cancelBackupsToolStripMenuItem });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(54, 22);
            toolStripDropDownButton1.Text = "File";
            // 
            // backUpNowToolStripMenuItem
            // 
            backUpNowToolStripMenuItem.Name = "backUpNowToolStripMenuItem";
            backUpNowToolStripMenuItem.Size = new Size(180, 22);
            backUpNowToolStripMenuItem.Text = "Back up now";
            backUpNowToolStripMenuItem.Click += backUpNowToolStripMenuItem_Click;
            // 
            // debugListAllReposToolStripMenuItem
            // 
            debugListAllReposToolStripMenuItem.Name = "debugListAllReposToolStripMenuItem";
            debugListAllReposToolStripMenuItem.Size = new Size(180, 22);
            debugListAllReposToolStripMenuItem.Text = "Debug list all repos";
            debugListAllReposToolStripMenuItem.Visible = false;
            debugListAllReposToolStripMenuItem.Click += debugListAllReposToolStripMenuItem_Click;
            // 
            // cancelBackupsToolStripMenuItem
            // 
            cancelBackupsToolStripMenuItem.Name = "cancelBackupsToolStripMenuItem";
            cancelBackupsToolStripMenuItem.Size = new Size(180, 22);
            cancelBackupsToolStripMenuItem.Text = "Cancel backups";
            cancelBackupsToolStripMenuItem.Visible = false;
            cancelBackupsToolStripMenuItem.Click += cancelBackupsToolStripMenuItem_Click;
            // 
            // niNotify
            // 
            niNotify.Text = "notifyIcon1";
            niNotify.Visible = true;
            niNotify.MouseDoubleClick += niNotify_MouseDoubleClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(965, 589);
            Controls.Add(toolStrip1);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Git Backup";
            FormClosing += MainForm_FormClosing;
            Resize += MainForm_Resize;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGitRepoBackupConfig).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslStatus;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private ToolStrip toolStrip1;
        private DataGridView dgvGitRepoBackupConfig;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem backUpNowToolStripMenuItem;
        private NotifyIcon niNotify;
        private ToolStripMenuItem debugListAllReposToolStripMenuItem;
        private ToolStripMenuItem cancelBackupsToolStripMenuItem;
    }
}
