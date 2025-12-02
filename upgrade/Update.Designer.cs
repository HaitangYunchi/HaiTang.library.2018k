namespace upgrade
{
    partial class Update
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelTitle;
        private Label lblTitle;
        private Button btnClose;
        private Panel panelContent;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Label lblFileName;
        private Label lblPercentage;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTitle = new Panel();
            lblTitle = new Label();
            btnClose = new Button();
            panelContent = new Panel();
            lblPercentage = new Label();
            lblFileName = new Label();
            lblStatus = new Label();
            progressBar = new ProgressBar();
            panelTitle.SuspendLayout();
            panelContent.SuspendLayout();
            SuspendLayout();
            // 
            // panelTitle
            // 
            panelTitle.BackColor = Color.FromArgb(30, 30, 30);
            panelTitle.Controls.Add(lblTitle);
            panelTitle.Controls.Add(btnClose);
            panelTitle.Dock = DockStyle.Top;
            panelTitle.Location = new Point(0, 0);
            panelTitle.Name = "panelTitle";
            panelTitle.Size = new Size(500, 40);
            panelTitle.TabIndex = 0;
            panelTitle.MouseDown += FormUpdate_MouseDown;
            panelTitle.MouseMove += FormUpdate_MouseMove;
            panelTitle.MouseUp += FormUpdate_MouseUp;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(12, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(88, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "正在更新中...";
            // 
            // btnClose
            // 
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold, GraphicsUnit.Point, 134);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(460, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(40, 40);
            btnClose.TabIndex = 1;
            btnClose.Text = "×";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // panelContent
            // 
            panelContent.BackColor = Color.White;
            panelContent.Controls.Add(lblPercentage);
            panelContent.Controls.Add(lblFileName);
            panelContent.Controls.Add(lblStatus);
            panelContent.Controls.Add(progressBar);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 40);
            panelContent.Name = "panelContent";
            panelContent.Padding = new Padding(20);
            panelContent.Size = new Size(500, 160);
            panelContent.TabIndex = 1;
            // 
            // lblPercentage
            // 
            lblPercentage.AutoSize = true;
            lblPercentage.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lblPercentage.ForeColor = Color.FromArgb(0, 122, 204);
            lblPercentage.Location = new Point(440, 115);
            lblPercentage.Name = "lblPercentage";
            lblPercentage.Size = new Size(35, 22);
            lblPercentage.TabIndex = 3;
            lblPercentage.Text = "0%";
            lblPercentage.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblFileName
            // 
            lblFileName.AutoSize = true;
            lblFileName.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblFileName.ForeColor = Color.FromArgb(100, 100, 100);
            lblFileName.Location = new Point(20, 50);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new Size(89, 17);
            lblFileName.TabIndex = 2;
            lblFileName.Text = "等待下载开始...";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblStatus.ForeColor = Color.FromArgb(64, 64, 64);
            lblStatus.Location = new Point(20, 20);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(130, 20);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "正在准备下载更新...";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(20, 80);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(460, 30);
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 0;
            // 
            // FormUpdate
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(500, 200);
            Controls.Add(panelContent);
            Controls.Add(panelTitle);
            DoubleBuffered = true;
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormUpdate";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "更新程序";
            FormClosing += FormUpdate_FormClosing;
            Load += FormUpdate_Load;
            MouseDown += FormUpdate_MouseDown;
            MouseMove += FormUpdate_MouseMove;
            MouseUp += FormUpdate_MouseUp;
            panelTitle.ResumeLayout(false);
            panelTitle.PerformLayout();
            panelContent.ResumeLayout(false);
            panelContent.PerformLayout();
            ResumeLayout(false);
        }
    }
}