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

namespace HaiTang.Library.Api2018k
{
    /// <summary>
    /// 提供应用程序日志记录功能的静态类
    /// <para>封装了NLog日志库，提供按天分割的日志文件记录功能</para>
    /// </summary>
    /// <remarks>
    /// <para>日志文件默认存储在应用程序根目录下的Logs文件夹中</para>
    /// <para>日志文件名格式：yyyy-MM-dd.log（按日期分割）</para>
    /// <para>日志格式包含：时间戳、日志级别、调用位置、消息内容和异常信息</para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code>
    /// Log.Info("应用程序启动");
    /// Log.Error(ex, "数据处理失败");
    /// </code>
    /// </example>
    public static class Log
    {
        /// <summary>
        /// NLog日志记录器实例
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 静态构造函数，初始化日志配置
        /// </summary>
        /// <remarks>
        /// 配置包含：
        /// <list type="bullet">
        /// <item>日志文件目标：按天分割日志文件</item>
        /// <item>日志格式：包含时间戳、级别、调用位置等信息</item>
        /// <item>日志级别：从Debug到Fatal的所有级别</item>
        /// </list>
        /// </remarks>
        static Log()
        {
            var config = new LoggingConfiguration();

            // 文件目标 - 按天分割
            var fileTarget = new FileTarget
            {
                Name = "file",
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "${shortdate}.log"),
                
                // 显示调用方法的完整信息（包括类名和方法名 本类）
                // Layout = "${longdate} [${level:uppercase=true}] ${callsite:className=true:fileName=false:includeSourcePath=false:methodName=true} - ${message} ${exception:format=tostring}"
                
                // 显示调用方法的上一层调用信息 (调用方的类名和方法名)
                Layout = "${longdate} [${level:uppercase=true}] ${callsite:className=true:fileName=false:includeSourcePath=false:methodName=true:skipFrames=1} - ${message} ${exception:format=tostring}"
            };

            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        /// <summary>
        /// 记录Debug级别的日志消息
        /// </summary>
        /// <param name="message">要记录的调试信息</param>
        /// <remarks>通常用于开发阶段的调试信息记录</remarks>
        public static void Debug(string message) => Logger.Debug(message);

        /// <summary>
        /// 记录Info级别的日志消息
        /// </summary>
        /// <param name="message">要记录的一般信息</param>
        /// <remarks>用于记录应用程序的正常运行状态信息</remarks>
        public static void Info(string message) => Logger.Info(message);

        /// <summary>
        /// 记录Warn级别的日志消息
        /// </summary>
        /// <param name="message">要记录的警告信息</param>
        /// <remarks>用于记录可能需要关注的潜在问题或异常情况</remarks>
        public static void Warn(string message) => Logger.Warn(message);

        /// <summary>
        /// 记录Error级别的日志消息
        /// </summary>
        /// <param name="message">要记录的错误信息</param>
        /// <remarks>用于记录不影响应用程序继续运行的错误</remarks>
        public static void Error(string message) => Logger.Error(message);

        /// <summary>
        /// 记录Error级别的日志消息，包含异常信息
        /// </summary>
        /// <param name="ex">相关的异常对象</param>
        /// <param name="message">要记录的错误描述信息</param>
        /// <remarks>用于记录包含异常详细信息的错误</remarks>
        public static void Error(Exception ex, string message) => Logger.Error(ex, message);

        /// <summary>
        /// 记录Fatal级别的日志消息
        /// </summary>
        /// <param name="message">要记录的严重错误信息</param>
        /// <remarks>用于记录导致应用程序无法继续运行的严重错误</remarks>
        public static void Fatal(string message) => Logger.Fatal(message);
    }
}
