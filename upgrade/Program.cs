using System;
using System.Windows.Forms;

namespace upgrade
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 检查参数数量
            if (args.Length < 2)
            {
                MessageBox.Show("使用方法: upgrade.exe <下载链接> <主程序路径>",
                    "参数错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string downloadUrl = args[0];
            string mainAssemblyPath = args[1];

            // 验证参数
            if (string.IsNullOrWhiteSpace(downloadUrl) ||
                string.IsNullOrWhiteSpace(mainAssemblyPath))
            {
                MessageBox.Show("参数不能为空", "参数错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 创建并显示主窗体
            Application.Run(new Update(downloadUrl, mainAssemblyPath));
        }
    }
}