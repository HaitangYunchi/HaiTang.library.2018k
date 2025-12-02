using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace upgrade
{
    public partial class Update : Form
    {
        // 定义委托和事件用于跨线程更新UI
        private delegate void UpdateProgressDelegate(int value, string status);
        private delegate void UpdateStatusDelegate(string status);

        // 下载相关变量
        private string _downloadUrl;
        private string _mainAssemblyPath;
        private string _tempDownloadPath;
        private string _tempExtractPath;
        private CancellationTokenSource _cancellationTokenSource;

        // 窗体拖动相关变量
        private bool _isDragging = false;
        private Point _startPoint = Point.Empty;

        public Update(string downloadUrl, string mainAssemblyPath)
        {
            InitializeComponent();

            // 保存参数
            _downloadUrl = downloadUrl;
            _mainAssemblyPath = mainAssemblyPath;

            // 初始化临时路径
            _tempDownloadPath = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid():N}.zip");
            _tempExtractPath = Path.Combine(Path.GetTempPath(), $"update_extract_{Guid.NewGuid():N}");

            // 设置窗体无边框
            this.FormBorderStyle = FormBorderStyle.None;

            // 初始化取消令牌源
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void FormUpdate_Load(object sender, EventArgs e)
        {
            // 窗体加载后开始下载
            StartDownload();
        }

        private async void StartDownload()
        {
            try
            {
                // 更新状态
                UpdateStatus("正在准备下载更新...");

                // 开始下载
                await DownloadFileAsync(_downloadUrl, _tempDownloadPath, _cancellationTokenSource.Token);

                // 下载完成后解压
                UpdateStatus("下载完成，正在解压文件...");
                await Task.Run(() => ExtractUpdatePackage());

                // 复制文件到主程序目录
                UpdateStatus("正在复制更新文件...");
                await Task.Run(() => CopyUpdateFiles());

                // 更新完成
                UpdateStatus("更新完成！");
                progressBar.Value = 100;

                // 等待2秒后启动主程序
                await Task.Delay(2000);

                // 启动主程序
                StartMainApplication();

                // 关闭更新程序
                this.Close();
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("更新已取消");
                MessageBox.Show("更新已取消", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
            catch (Exception ex)
            {
                UpdateStatus("更新失败");
                MessageBox.Show($"更新过程中发生错误：{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private async Task DownloadFileAsync(string url, string destinationPath, CancellationToken cancellationToken)
        {
            using (WebClient client = new WebClient())
            {
                // 设置下载进度事件
                client.DownloadProgressChanged += (s, e) =>
                {
                    UpdateProgress(e.ProgressPercentage, $"正在下载: {e.BytesReceived / 1024 / 1024}MB / {e.TotalBytesToReceive / 1024 / 1024}MB");
                };

                // 创建TaskCompletionSource用于异步等待
                var tcs = new TaskCompletionSource<bool>();

                // 设置下载完成事件
                client.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Cancelled)
                    {
                        tcs.SetCanceled();
                    }
                    else if (e.Error != null)
                    {
                        tcs.SetException(e.Error);
                    }
                    else
                    {
                        tcs.SetResult(true);
                    }
                };

                // 开始异步下载
                client.DownloadFileAsync(new Uri(url), destinationPath);

                // 等待下载完成或取消
                await tcs.Task;
            }
        }

        private void ExtractUpdatePackage()
        {
            try
            {
                // 确保解压目录存在
                if (Directory.Exists(_tempExtractPath))
                {
                    Directory.Delete(_tempExtractPath, true);
                }
                Directory.CreateDirectory(_tempExtractPath);

                // 解压ZIP文件
                ZipFile.ExtractToDirectory(_tempDownloadPath, _tempExtractPath);

                // 删除临时下载文件
                File.Delete(_tempDownloadPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"解压失败: {ex.Message}");
            }
        }

        private void CopyUpdateFiles()
        {
            try
            {
                // 获取主程序目录
                string mainAppDir = Path.GetDirectoryName(_mainAssemblyPath);

                // 遍历解压目录中的所有文件
                string[] files = Directory.GetFiles(_tempExtractPath, "*.*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    // 获取相对路径
                    string relativePath = file.Substring(_tempExtractPath.Length + 1);
                    string destPath = Path.Combine(mainAppDir, relativePath);

                    // 确保目标目录存在
                    string destDir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    // 复制文件（覆盖现有文件）
                    File.Copy(file, destPath, true);

                    // 在主线程更新状态
                    this.Invoke(new MethodInvoker(() =>
                    {
                        lblFileName.Text = $"正在更新: {Path.GetFileName(file)}";
                    }));
                }

                // 删除临时解压目录
                Directory.Delete(_tempExtractPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"文件复制失败: {ex.Message}");
            }
        }

        private void StartMainApplication()
        {
            try
            {
                if (File.Exists(_mainAssemblyPath))
                {
                    Process.Start(_mainAssemblyPath);
                }
                else
                {
                    MessageBox.Show($"找不到主程序: {_mainAssemblyPath}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动主程序失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 更新进度条和状态
        private void UpdateProgress(int value, string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateProgressDelegate(UpdateProgress), value, status);
                return;
            }

            progressBar.Value = value;
            lblStatus.Text = status;
            lblPercentage.Text = $"{value}%";
        }

        private void UpdateStatus(string status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new UpdateStatusDelegate(UpdateStatus), status);
                return;
            }

            lblStatus.Text = status;
        }

        // 窗体拖动相关事件
        private void FormUpdate_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _startPoint = new Point(e.X, e.Y);
            }
        }

        private void FormUpdate_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point newPoint = this.PointToScreen(new Point(e.X, e.Y));
                newPoint.Offset(-_startPoint.X, -_startPoint.Y);
                this.Location = newPoint;
            }
        }

        private void FormUpdate_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        // 关闭按钮点击事件
        private void btnClose_Click(object sender, EventArgs e)
        {
            // 询问是否取消更新
            if (MessageBox.Show("确定要取消更新吗？", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _cancellationTokenSource.Cancel();
                Application.Exit();
            }
        }

        // 窗体关闭事件
        private void FormUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 清理临时文件
            try
            {
                if (File.Exists(_tempDownloadPath))
                {
                    File.Delete(_tempDownloadPath);
                }
                if (Directory.Exists(_tempExtractPath))
                {
                    Directory.Delete(_tempExtractPath, true);
                }
            }
            catch { }

            _cancellationTokenSource?.Dispose();
        }
    }
}