/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library.Bilibili
 * 唯一标识：df44b903-ef7d-45b9-8c9b-03015b724e94
 * 文件名：DeviceInfo
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/10/3 8:55:25
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

using System.Net.NetworkInformation;

namespace HaiTang.Library.Api2018k.Bilibili
{
    /// <summary>
    /// 设备信息模型 - 增强版本，支持B站设备识别
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// 设备标识
        /// </summary>
        public string DeviceId { get; set; } = GenerateBilibiliDeviceId();

        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; } = GetPcDeviceName();

        /// <summary>
        /// 平台类型
        /// </summary>
        public string Platform { get; set; } = "windows";

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; } = "GenshinConvert";

        /// <summary>
        /// 应用版本
        /// </summary>
        public string AppVersion { get; set; } = "1.0.0";

        /// <summary>
        /// 用户代理字符串 - 修改为包含应用信息的格式
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Mihoyo_Tools/2.0";

        /// <summary>
        /// 生成B站格式的设备ID - 修改为更符合B站格式
        /// </summary>
        private static string GenerateBilibiliDeviceId()
        {
            try
            {
                // 使用MAC地址和机器信息生成更稳定的设备ID
                string macAddress = GetMacAddress();
                string machineName = Environment.MachineName;

                // 生成MD5哈希作为设备ID
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    var input = $"{macAddress}-{machineName}";
                    var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch
            {
                // 备用方案：使用GUID
                return Guid.NewGuid().ToString("N").ToLower();
            }
        }

        /// <summary>
        /// 获取MAC地址
        /// </summary>
        private static string GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && !nic.Description.Contains("Virtual"))
                    {
                        return nic.GetPhysicalAddress().ToString();
                    }
                }
                return "UnknownMAC";
            }
            catch
            {
                return "UnknownMAC";
            }
        }

        /// <summary>
        /// 获取PC设备名称
        /// </summary>
        private static string GetPcDeviceName()
        {
            return "Windows_PC";
        }

        /// <summary>
        /// 获取完整的用户代理字符串 - 修改为包含应用信息
        /// </summary>
        public string GetFullUserAgent()
        {
            return $"{UserAgent} {AppName}/{AppVersion} (Device: {DeviceName})";
        }

        /// <summary>
        /// 获取设备信息摘要（用于B站识别）
        /// </summary>
        public string GetDeviceSummary()
        {
            return $"设备:{DeviceName} 应用:{AppName} {AppVersion}";
        }
    }
}