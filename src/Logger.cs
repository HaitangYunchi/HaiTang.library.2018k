/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library
 * 唯一标识：4197c1b0-4a31-4760-8d63-39e3acd25f9f
 * 文件名：Logger
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 15:02:46
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/


using NLog;
using NLog.Config;
using NLog.Targets;

namespace HaiTang.library
{
    public static class Log
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static Log()
        {
            var config = new LoggingConfiguration();

            // 文件目标 - 按天分割
            var fileTarget = new FileTarget
            {
                Name = "file",
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "${shortdate}.log"),
                Layout = "${longdate} [${level:uppercase=true}] ${callsite:className=true:fileName=false:includeSourcePath=false:methodName=true} - ${message} ${exception:format=tostring}"
            };

            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        public static void Debug(string message) => Logger.Debug(message);
        public static void Info(string message) => Logger.Info(message);
        public static void Warn(string message) => Logger.Warn(message);
        public static void Error(string message) => Logger.Error(message);
        public static void Error(Exception ex, string message) => Logger.Error(ex, message);
        public static void Fatal(string message) => Logger.Fatal(message);
    }
}
