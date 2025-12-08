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
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Text;


namespace HaiTang.library
{
    public class Tools
    {
        private static readonly Random _random = new();
        #region 公有方法

        /// <summary>
        /// 更新程序
        /// </summary>
        /// <param name="downloadUrl">下载地址</param>
        public static void upgrade(string downloadUrl)
        {
            try
            {
                string mainAssemblyPath = Process.GetCurrentProcess().MainModule.FileName;
                string updaterExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upgrade.exe");

                // 检查更新程序是否存在
                if (!System.IO.File.Exists(updaterExePath))
                {
                    return;
                }

                // 启动更新程序，传递参数
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = updaterExePath,
                    Arguments = $"\"{downloadUrl}\" \"{mainAssemblyPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动更新程序失败");
                throw new Exception($"启动更新程序失败: {ex.Message}");
            }
        }

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

                return Tools.Sha512(composite);
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
        
        /// <summary>
        /// 计算输入字符串的SHA-256哈希值
        /// </summary>
        /// <param name="input">要计算哈希的输入字符串</param>
        /// <returns>64个字符的十六进制字符串，表示SHA-256哈希值</returns>
        /// <exception cref="ArgumentNullException">当<paramref name="input"/>为null或空字符串时抛出</exception>
        /// <remarks>
        /// <para>使用UTF-8编码将字符串转换为字节数组</para>
        /// <para>哈希输出为64个字符的十六进制字符串（大写）</para>
        /// <para>示例：对于输入"hello"，返回"2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824"</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// string hash = Hasher.Sha256("password123");
        /// Console.WriteLine(hash); // 输出：EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F
        /// </code>
        /// </example>
        public static string Sha256(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        /// <summary>
        /// 计算输入字符串的SHA-512哈希值
        /// </summary>
        /// <param name="input">要计算哈希的输入字符串</param>
        /// <returns>128个字符的十六进制字符串，表示SHA-512哈希值</returns>
        /// <exception cref="ArgumentNullException">当<paramref name="input"/>为null或空字符串时抛出</exception>
        /// <remarks>
        /// <para>使用UTF-8编码将字符串转换为字节数组</para>
        /// <para>哈希输出为128个字符的十六进制字符串（大写）</para>
        /// <para>SHA-512比SHA-256提供更强的安全性，但生成的哈希值更长</para>
        /// <para>示例：对于输入"hello"，返回"9B71D224BD62F3785D96D46AD3EA3D73319BFBC2890CAADA2F2D80E0F2DCEB48C57C21D6D1E2F9B1B0B68A8A1E9AB1B8C0D1C6E2D2E4F7C8A1B6C8D2E5F"</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// string hash = Hasher.Sha512("password123");
        /// Console.WriteLine(hash); // 输出：长度128位的十六进制字符串
        /// </code>
        /// </example>
        public static string Sha512(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha512.ComputeHash(bytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">AES密钥（十六进制字符串）</param>
        /// <returns>Base64编码的加密数据（包含IV和密文）</returns>
        public static string Encrypt(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);
                aesAlg.GenerateIV(); // 随机生成IV

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // 先写入IV（16字节）
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                    // 再写入加密数据
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">Base64编码的加密数据（包含IV和密文）</param>
        /// <param name="key">AES密钥（十六进制字符串）</param>
        /// <returns>解密后的明文</returns>
        public static string Decrypt(string cipherText, string key)
        {
            byte[] fullData = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);

                // 提取IV（前16字节）
                byte[] iv = new byte[16];
                Array.Copy(fullData, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                // 提取密文（剩余部分）
                byte[] _cipherText = new byte[fullData.Length - 16];
                Array.Copy(fullData, 16, _cipherText, 0, cipherText.Length);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(_cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 使用AES算法加密明文字符串
        /// </summary>
        /// <param name="plainText">要加密的明文字符串</param>
        /// <param name="password">加密密码，用于生成加密密钥</param>
        /// <param name="salt">盐值，用于增强密码安全性，防止彩虹表攻击</param>
        /// <returns>返回Base64格式的加密字符串，包含IV和加密数据</returns>
        /// <exception cref="ArgumentNullException">当plainText为null时抛出</exception>
        /// <exception cref="CryptographicException">当加密过程中出现加密错误时抛出</exception>
        /// <example>
        /// 使用示例：
        /// <code>
        /// string encrypted = SaltAesEncry.Encrypt("敏感数据", "myPassword", "saltValue");
        /// </code>
        /// </example>
        public static string Encrypt(string plainText, string password, string salt)
        {
            // 输入验证：如果明文字符串为空或null，直接返回原值
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // 使用AES算法实例进行加密操作
            using (var aes = Aes.Create())
            {
                // 使用PBKDF2密钥派生函数从密码和盐值生成加密密钥
                // 增强安全性：使用高迭代次数和强哈希算法
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
                byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,           // 用户提供的密码
                    salt: saltBytes,              // 盐值字节数组
                    iterations: 10000,            // 迭代次数，增加暴力破解难度
                    hashAlgorithm: HashAlgorithmName.SHA512, // 使用SHA512哈希算法
                    outputLength: 32              // 生成256位（32字节）AES密钥
                );

                // 设置AES加密密钥
                aes.Key = key;

                // 自动生成随机初始化向量(IV)
                // 重要：每次加密都生成不同的IV，防止模式分析攻击
                aes.GenerateIV();

                // 使用内存流存储加密结果
                using (var ms = new MemoryStream())
                {
                    // 首先将IV写入输出流的前16字节
                    // 解密时需要从密文开头读取IV
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    // 创建加密转换器和加密流
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        // 将明文字符串写入加密流，自动进行加密
                        sw.Write(plainText);
                    }
                    // 注意：CryptoStream会自动刷新和处置

                    // 将内存流中的加密数据转换为Base64字符串返回
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密使用AES算法加密的字符串
        /// </summary>
        /// <param name="cipherText">Base64格式的加密字符串，必须包含IV和加密数据</param>
        /// <param name="password">解密密码，必须与加密时使用的密码相同</param>
        /// <param name="salt">盐值，必须与加密时使用的盐值相同</param>
        /// <returns>返回解密后的明文字符串</returns>
        /// <exception cref="ArgumentNullException">当cipherText为null或空时抛出</exception>
        /// <exception cref="FormatException">当cipherText不是有效的Base64字符串时抛出</exception>
        /// <exception cref="CryptographicException">
        /// <example>
        /// 使用示例：
        /// <code>
        /// string decrypted = SaltAesEncry.Decrypt(encryptedString, "myPassword", "saltValue");
        /// </code>
        /// </example>
        public static string Decrypt(string cipherText, string password, string salt)
        {
            // 输入验证：如果密文字符串为空或null，直接返回原值
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            // 将Base64格式的密文字符串解码为字节数组
            var fullCipher = Convert.FromBase64String(cipherText);

            // 使用AES算法实例进行解密操作
            using (var aes = Aes.Create())
            {
                // 使用与加密时相同的PBKDF2参数生成密钥
                // 重要：必须使用相同的密码、盐值、迭代次数和哈希算法
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
                byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,           // 必须与加密密码相同
                    salt: saltBytes,              // 必须与加密盐值相同
                    iterations: 10000,            // 必须与加密迭代次数相同
                    hashAlgorithm: HashAlgorithmName.SHA512, // 必须与加密哈希算法相同
                    outputLength: 32              // 必须与加密密钥长度相同
                );

                // 设置AES解密密钥
                aes.Key = key;

                // 从密文开头提取IV（初始化向量）
                // AES IV固定为16字节长度
                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // 创建解密转换器
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                // 创建内存流，跳过前16字节的IV，只处理加密数据部分
                using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                // 创建解密流
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                // 使用流读取器读取解密后的文本
                using (var sr = new StreamReader(cs))
                {
                    // 读取所有解密内容并返回
                    return sr.ReadToEnd();
                }
            }
        }


        #endregion

        #region 私有方法

        // <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        private static byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
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
        // 生成序列号
        private static string GenerateFormattedCode(string cpuId, string motherboardId)
        {
            // 组合硬件信息
            string composite = $"{cpuId}_{motherboardId}_{Constants.MACHINE_CODE_SALT}";
            // 格式化输出
            return FormatMachineCode(Tools.Sha256(composite));

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
