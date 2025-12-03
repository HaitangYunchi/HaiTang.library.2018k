/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library
 * 唯一标识：0a956093-73f2-4926-a70c-6d6a72c6ebf5
 * 文件名：Tools
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 15:24:19
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


using HaiTang.library.Utils;
using System.Management;
using System.Security.Cryptography;


namespace HaiTang.library
{
    public class Tools
    {
        private static readonly Random _random = new();
        #region 本地方法
        /// <summary>
        /// 获取机器码 cpu+主板+64位盐值 进行验证
        /// </summary>
        /// <returns>string 返回20字符串机器码，格式：XXXXX-XXXXX-XXXXX-XXXXX-XXXXX</returns>
        [Obsolete("请使用 GetMachineCodeEx() 以获得更好的机器码，2027年01月01日正式禁用此方法", false)]
        public static string GetMachineCode()
        {
            try
            {
                // 获取硬件信息
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                // 生成机器码
                return GenerateFormattedCode(cpuId, motherboardId);
            }
            catch
            {
                return GenerateErrorCode(); // 如果失败生成错误码 这种几率几乎可以忽略不计
            }
        }

        /// <summary>
        /// 获取机器码 cpu+主板+64位盐值 进行验证
        /// </summary>
        /// <returns>string 返回128字符串机器码</returns>
        public static string GetMachineCodeEx()
        {
            try
            {
                // 获取硬件信息
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                // 生成机器码
                string composite = $"{cpuId}-{motherboardId}-{Constants.MACHINE_CODE_SALT}";

                return Hasher.Sha512(composite);
            }
            catch
            {
                return GenerateErrorCode(); // 如果失败生成错误码 这种几率几乎可以忽略不计
            }
        }
        /// <summary>
        /// 生成随机字符串 使用方法 GenerateRandomString(18, 4)
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="type">模式
        /// 0: 字母+数字
        /// 1: 只有字母
        /// 2: 只有数字
        /// 3: 只有大写字母
        /// 4: 大写字母+数字
        /// </param>
        /// <returns>随机字符串</returns>
        public static string GenerateRandomString(int length, int type = 0)
        {
            string chars = type switch
            {
                1 => "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ",
                2 => "123456789",
                3 => "ABCDEFGHJKLMNPQRSTUVWXYZ",
                4 => "ABCDEFGHJKLMNPQRSTUVWXYZ123456789",
                _ => "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ123456789"
            };

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[_random.Next(chars.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// 生成密码学安全的随机盐值
        /// </summary>
        /// <param name="length">盐值的字节长度，默认为64字节</param>
        /// <returns>返回Base64编码的随机盐值字符串</returns>
        /// <exception cref="ArgumentOutOfRangeException">当length小于等于0时抛出</exception>
        /// <example>
        /// 使用示例：
        /// <code>
        /// string salt = SaltAesEncry.GenerateSalt(); // 生成64字节盐值
        /// string customSalt = SaltAesEncry.GenerateSalt(32); // 生成32字节盐值
        /// </code>
        /// </example>
        public static string GenerateSalt(int length = 64)
        {
            // 验证输入参数
            if (length <= 0)
            {
                Log.Error("生成盐值失败：长度必须大于0");
                throw new ArgumentOutOfRangeException(nameof(length), "盐值长度必须大于0");
            }
                

            // 创建指定长度的随机字节数组
            var randomBytes = new byte[length];

            // 使用密码学安全的随机数生成器
            using (var rng = RandomNumberGenerator.Create())
            {
                // 填充随机字节
                rng.GetBytes(randomBytes);
                // 将随机字节数组转换为Base64字符串返回
                return Convert.ToBase64String(randomBytes);
            }
        }
        // 获取CPU信息
        private static string GetCpuId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                using var collection = searcher.Get();

                var cpuId = collection.Cast<ManagementObject>()
                    .Select(mo => mo["ProcessorId"]?.ToString())
                    .FirstOrDefault(id => !string.IsNullOrEmpty(id));

                return cpuId ?? "UnknownCPU";
            }
            catch
            {
                return "UnknownCPU";
            }
        }
        // 获取主板信息
        private static string GetMotherboardId()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                using var collection = searcher.Get();

                var motherboardSn = collection.Cast<ManagementObject>()
                    .Select(mo => mo["SerialNumber"]?.ToString())
                    .FirstOrDefault(sn => !string.IsNullOrEmpty(sn));

                return motherboardSn ?? "UnknownMotherboard";
            }
            catch
            {
                return "UnknownMotherboard";
            }
        }

        // 生成序列号
        private static string GenerateFormattedCode(string cpuId, string motherboardId)
        {
            // 组合硬件信息
            string composite = $"{cpuId}_{motherboardId}_{Constants.MACHINE_CODE_SALT}";
            // 格式化输出
            return FormatMachineCode(Hasher.Sha256(composite));

        }
        // 格式化机器码
        private static string FormatMachineCode(string hash)
        {
            // 确保20字符长度
            hash = hash.Length >= 20 ?
                   hash.Substring(0, 20) :
                   hash.PadRight(25, '0');

            // 5字符分段格式化
            return $"{hash.Substring(0, 5)}-{hash.Substring(5, 5)}-{hash.Substring(10, 5)}-{hash.Substring(15, 5)}";
        }


        private static string GenerateErrorCode()
        {

            string timestamp = DateTime.Now.ToString("yyyy-MMdd-HHmm");
            return $"ERR-{timestamp.Substring(0, 9)}-{Guid.NewGuid().ToString("N").Substring(0, 5)}";
        }
        #endregion
    }
}
