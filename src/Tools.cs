/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间： HaiTang.Library.Api2018k
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

using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace HaiTang.Library.Api2018k
{
    public static class Tools
    {
        private static readonly Random _random = new Random();
        // 敏感信息：软件实例ID、开发者密钥、机器码盐值
        private static readonly SecureString _softwareId = new SecureString();
        private static readonly SecureString _developerKey = new SecureString();
        private static readonly SecureString _machineCodeSalt = new SecureString();

        // 静态构造函数：从安全源加载盐值（此处为示例，实际应读取加密配置）
        static Tools()
        {
            // 注意：实际生产环境应从加密配置文件或环境变量中读取，此处仅为演示。
            // 即使硬编码，字符串也仅在静态构造函数中短暂存在，随即存入 SecureString 并允许 GC 回收。
            string salt = "k3apRuJR2j388Yy5CWxfnXrHkwg3AvUntgVhuUMWBDXDEsyaeX7Ze3QbvmejbqSz";
            foreach (char c in salt)
                _machineCodeSalt.AppendChar(c);
            _machineCodeSalt.MakeReadOnly();
        }

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

                if (!File.Exists(updaterExePath))
                    return;

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
        /// 获取机器码（旧版，已过时）
        /// </summary>
        [Obsolete("请使用 GetMachineCodeEx() 以获得更好的机器码，2027年01月01日正式禁用此方法", false)]
        public static string GetMachineCode()
        {
            try
            {
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                return GenerateFormattedCode(cpuId, motherboardId);
            }
            catch
            {
                return GenerateErrorCode();
            }
        }

        /// <summary>
        /// 获取机器码（新版，使用安全盐值）
        /// </summary>
        /// <returns>128字符长度的十六进制字符串</returns>
        public static string GetMachineCodeEx()
        {
            try
            {
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                // 从 Constants 安全获取盐值
                string salt = Tools.ExecuteWithMachineCodeSalt(s => s);
                string composite = $"{cpuId}-{motherboardId}-{salt}";
                return Sha512(composite);
            }
            catch
            {
                return GenerateErrorCode();
            }
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <param name="type">模式：0=字母+数字，1=字母，2=数字，3=大写字母，4=大写字母+数字</param>
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
                result[i] = chars[_random.Next(chars.Length)];
            return new string(result);
        }

        /// <summary>
        /// 生成密码学安全的随机盐值
        /// </summary>
        /// <param name="length">盐值的字节长度，默认为64字节</param>
        public static string GenerateSalt(int length = 64)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "盐值长度必须大于0");
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        /// <summary>
        /// 计算输入字符串的SHA-256哈希值
        /// </summary>
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
        /// AES加密（简单模式，IV随机）
        /// </summary>
        public static string Encrypt(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);
                aesAlg.GenerateIV();
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText);
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// AES解密（简单模式）
        /// </summary>
        public static string Decrypt(string cipherText, string key)
        {
            byte[] fullData = Convert.FromBase64String(cipherText);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);
                byte[] iv = new byte[16];
                Array.Copy(fullData, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;
                byte[] _cipherText = new byte[fullData.Length - 16];
                Array.Copy(fullData, 16, _cipherText, 0, _cipherText.Length);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(_cipherText))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    return srDecrypt.ReadToEnd();
            }
        }

        /// <summary>
        /// AES加密（密码+盐模式）
        /// </summary>
        public static string Encrypt(string plainText, string password, string salt)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            using (var aes = Aes.Create())
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
                byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 10000, HashAlgorithmName.SHA512, 32);
                aes.Key = key;
                aes.GenerateIV();
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                        sw.Write(plainText);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// AES解密（密码+盐模式）
        /// </summary>
        public static string Decrypt(string cipherText, string password, string salt)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            var fullCipher = Convert.FromBase64String(cipherText);
            using (var aes = Aes.Create())
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
                byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 10000, HashAlgorithmName.SHA512, 32);
                aes.Key = key;
                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                    return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// 尝试将字符串转换为布尔值，支持多种常见格式
        /// </summary>
        public static bool ToBoolean(string value, out bool result)
        {
            result = false;
            if (string.IsNullOrWhiteSpace(value)) return false;
            string normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "true":
                case "1":
                case "yes":
                case "y":
                case "on":
                case "enable":
                case "enabled":
                case "active":
                case "t":
                case "ok":
                case "okay":
                case "correct":
                case "right":
                case "positive":
                case "affirmative":
                case "aye":
                case "si":
                case "da":
                case "ja":
                case "はい":
                case "是":
                case "真":
                case "✓":
                case "√":
                    result = true; return true;
                case "false":
                case "0":
                case "no":
                case "n":
                case "off":
                case "disable":
                case "disabled":
                case "inactive":
                case "f":
                case "cancel":
                case "wrong":
                case "incorrect":
                case "negative":
                case "nay":
                case "no way":
                case "non":
                case "nein":
                case "нет":
                case "いいえ":
                case "否":
                case "假":
                case "✗":
                case "×":
                    result = false; return true;
                default:
                    return bool.TryParse(normalized, out result);
            }
        }

        /// <summary>
        /// 生成固定的盐值（SHA256哈希）
        /// </summary>
        public static string GetFixedSalt()
        {
            string[] mathConstants =
            {
                Math.PI.ToString("F15"),
                Math.E.ToString("F15"),
                ((1 + Math.Sqrt(5)) / 2 - 1).ToString("F15"),
                Math.Sqrt(2).ToString("F15"),
                Math.Log(2).ToString("F15")
            };
            StringBuilder seedBuilder = new StringBuilder();
            seedBuilder.Append("FIXED_SALT:");
            int maxLength = mathConstants.Max(c => c.Length);
            for (int i = 0; i < maxLength; i++)
            {
                for (int j = 0; j < mathConstants.Length; j++)
                {
                    if (i < mathConstants[j].Length)
                        seedBuilder.Append(mathConstants[j][i]);
                    if ((i * mathConstants.Length + j) % 5 == 0)
                    {
                        int charIndex = (i + j) % 52;
                        char mixedChar = charIndex < 26 ? (char)('A' + charIndex) : (char)('a' + charIndex - 26);
                        seedBuilder.Append(mixedChar);
                    }
                }
            }
            string baseSeed = seedBuilder.ToString();
            uint checksum = CalculateSecureChecksum(baseSeed);
            seedBuilder.Append($"|CS:{checksum:X}|VER:2.0");
            string seed = seedBuilder.ToString();
            return Sha256(seed);
        }

        // +++ 新增 RSA2 密钥对生成相关方法 +++
        /// <summary>
        /// RSA2 密钥对信息
        /// </summary>
        public class Rsa2KeyPair
        {
            /// <summary>公钥 (PEM 格式)</summary>
            public string PublicKeyPem { get; internal set; }

            /// <summary>私钥 (PEM 格式)</summary>
            public string PrivateKeyPem { get; internal set; }

            /// <summary>公钥 (XML 格式)</summary>
            public string PublicKeyXml { get; internal set; }

            /// <summary>私钥 (XML 格式)</summary>
            public string PrivateKeyXml { get; internal set; }

            /// <summary>密钥长度 (位)</summary>
            public int KeySize { get; internal set; }

            /// <summary>生成时间 (UTC)</summary>
            public DateTime GeneratedAt { get; internal set; } = DateTime.UtcNow;
        }

        /// <summary>
        /// 生成 RSA2 密钥对（同时返回 PEM 和 XML 格式）
        /// </summary>
        /// <param name="keySize">密钥长度，推荐 2048，范围 512~16384</param>
        /// <returns>包含公私钥信息的 Rsa2KeyPair 对象</returns>
        /// <exception cref="ArgumentException">密钥长度无效时抛出</exception>
        public static Rsa2KeyPair GenerateRsa2KeyPair(int keySize = 2048)
        {
            if (keySize < 512 || keySize > 16384)
                throw new ArgumentException("密钥长度必须在512~16384之间", nameof(keySize));

            using var rsa = RSA.Create(keySize);
            var pair = new Rsa2KeyPair
            {
                KeySize = keySize,
                PublicKeyPem = rsa.ExportRSAPublicKeyPem(),
                PrivateKeyPem = rsa.ExportRSAPrivateKeyPem(),
                PublicKeyXml = rsa.ToXmlString(false),
                PrivateKeyXml = rsa.ToXmlString(true)
            };
            return pair;
        }

        /// <summary>
        /// 生成 RSA2 密钥对，仅返回 PEM 格式的字符串元组
        /// </summary>
        /// <param name="privateKeyPem">私钥 PEM</param>
        /// <param name="publicKeyPem">公钥 PEM</param>
        /// <param name="keySize">密钥长度</param>
        public static void GenerateRsa2PemKeys(out string privateKeyPem, out string publicKeyPem, int keySize = 2048)
        {
            var pair = GenerateRsa2KeyPair(keySize);
            privateKeyPem = pair.PrivateKeyPem;
            publicKeyPem = pair.PublicKeyPem;
        }

        /// <summary>
        /// 生成 RSA2 密钥对，仅返回 XML 格式的字符串元组
        /// </summary>
        /// <param name="privateKeyXml">私钥 XML</param>
        /// <param name="publicKeyXml">公钥 XML</param>
        /// <param name="keySize">密钥长度</param>
        public static void GenerateRsa2XmlKeys(out string privateKeyXml, out string publicKeyXml, int keySize = 2048)
        {
            var pair = GenerateRsa2KeyPair(keySize);
            privateKeyXml = pair.PrivateKeyXml;
            publicKeyXml = pair.PublicKeyXml;
        }

        /// <summary>
        /// 保存 RSA2 密钥对到文件（PEM 格式）
        /// </summary>
        /// <param name="privateKeyFilePath">私钥文件路径</param>
        /// <param name="publicKeyFilePath">公钥文件路径</param>
        /// <param name="keySize">密钥长度</param>
        public static void SaveRsa2PemToFile(string privateKeyFilePath, string publicKeyFilePath, int keySize = 2048)
        {
            var pair = GenerateRsa2KeyPair(keySize);
            File.WriteAllText(privateKeyFilePath, pair.PrivateKeyPem, Encoding.UTF8);
            File.WriteAllText(publicKeyFilePath, pair.PublicKeyPem, Encoding.UTF8);
        }

        /// <summary>
        /// 保存 RSA2 密钥对到文件（XML 格式）
        /// </summary>
        /// <param name="privateKeyFilePath">私钥文件路径</param>
        /// <param name="publicKeyFilePath">公钥文件路径</param>
        /// <param name="keySize">密钥长度</param>
        public static void SaveRsa2XmlToFile(string privateKeyFilePath, string publicKeyFilePath, int keySize = 2048)
        {
            var pair = GenerateRsa2KeyPair(keySize);
            File.WriteAllText(privateKeyFilePath, pair.PrivateKeyXml, Encoding.UTF8);
            File.WriteAllText(publicKeyFilePath, pair.PublicKeyXml, Encoding.UTF8);
        }

        // +++ 结束 +++

        #region 设置方法（安全重载）

        /// <summary>
        /// 从 SecureString 设置软件ID，全程不产生托管字符串。
        /// </summary>
        /// <param name="secureValue">包含软件ID的 SecureString</param>
        /// <exception cref="ArgumentNullException">当 secureValue 为 null 时抛出</exception>
        public static void SetSoftwareId(SecureString secureValue)
        {
            if (secureValue == null)
                throw new ArgumentNullException(nameof(secureValue));

            IntPtr ptr = IntPtr.Zero;
            char[] buffer = null;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureValue);
                int length = secureValue.Length;
                buffer = new char[length];
                Marshal.Copy(ptr, buffer, 0, length);

                lock (_softwareId)
                {
                    _softwareId.Clear();
                    foreach (char c in buffer)
                        _softwareId.AppendChar(c);
                    _softwareId.MakeReadOnly();
                }
            }
            finally
            {
                if (buffer != null)
                    Array.Clear(buffer, 0, buffer.Length);
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        /// <summary>
        /// 从 SecureString 设置开发者密钥，全程不产生托管字符串。
        /// </summary>
        /// <param name="secureValue">包含开发者密钥的 SecureString</param>
        /// <exception cref="ArgumentNullException">当 secureValue 为 null 时抛出</exception>
        public static void SetDeveloperKey(SecureString secureValue)
        {
            if (secureValue == null)
                throw new ArgumentNullException(nameof(secureValue));

            IntPtr ptr = IntPtr.Zero;
            char[] buffer = null;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureValue);
                int length = secureValue.Length;
                buffer = new char[length];
                Marshal.Copy(ptr, buffer, 0, length);

                lock (_developerKey)
                {
                    _developerKey.Clear();
                    foreach (char c in buffer)
                        _developerKey.AppendChar(c);
                    _developerKey.MakeReadOnly();
                }
            }
            finally
            {
                if (buffer != null)
                    Array.Clear(buffer, 0, buffer.Length);
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }


        /// <summary>
        /// 将普通字符串转换为 SecureString
        /// </summary>
        /// <param name="input">普通字符串</param>
        public static SecureString CreateSecureString(string input)
        {

            if (string.IsNullOrEmpty(input))
                return null;

            var secureString = new SecureString();
            foreach (char c in input)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly(); // 设置为只读状态，防止后续修改
            return secureString;

        }

        #endregion

        #region 安全访问委托（获取敏感信息明文，用完自动擦除）

        /// <summary>
        /// 安全执行：获取软件ID明文，在委托内使用，用完自动擦除内存。
        /// </summary>
        public static T ExecuteWithSoftwareId<T>(Func<string, T> action)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(_softwareId);
                string plain = Marshal.PtrToStringUni(ptr);
                return action(plain);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        /// <summary>
        /// 安全执行：获取开发者密钥明文，在委托内使用，用完自动擦除内存。
        /// </summary>
        public static T ExecuteWithDeveloperKey<T>(Func<string, T> action)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(_developerKey);
                string plain = Marshal.PtrToStringUni(ptr);
                return action(plain);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        /// <summary>
        /// 安全执行：同时获取软件ID和开发者密钥明文。
        /// </summary>
        public static T ExecuteWithBoth<T>(Func<string, string, T> action)
        {
            IntPtr ptrId = IntPtr.Zero, ptrKey = IntPtr.Zero;
            try
            {
                ptrId = Marshal.SecureStringToGlobalAllocUnicode(_softwareId);
                ptrKey = Marshal.SecureStringToGlobalAllocUnicode(_developerKey);
                string softwareId = Marshal.PtrToStringUni(ptrId);
                string developerKey = Marshal.PtrToStringUni(ptrKey);
                return action(softwareId, developerKey);
            }
            finally
            {
                if (ptrId != IntPtr.Zero) Marshal.ZeroFreeGlobalAllocUnicode(ptrId);
                if (ptrKey != IntPtr.Zero) Marshal.ZeroFreeGlobalAllocUnicode(ptrKey);
            }
        }

        /// <summary>
        /// 安全执行：获取机器码盐值明文。
        /// </summary>
        public static T ExecuteWithMachineCodeSalt<T>(Func<string, T> action)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(_machineCodeSalt);
                string plain = Marshal.PtrToStringUni(ptr);
                return action(plain);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        #endregion

        #endregion

        #region 私有方法

        private static uint CalculateSecureChecksum(string input)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentException("输入不能为空");
            const uint fnvPrime = 0x01000193;
            const uint fnvOffsetBasis = 0x811c9dc5;
            uint hash = fnvOffsetBasis;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            foreach (byte b in bytes)
            {
                hash ^= b;
                hash *= fnvPrime;
            }
            hash ^= hash >> 16;
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;
            return hash;
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

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
            catch { return "UnknownMotherboard"; }
        }

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
            catch { return "UnknownCPU"; }
        }

        private static string GenerateFormattedCode(string cpuId, string motherboardId)
        {
            // 注意：此方法仅供旧版 GetMachineCode 使用，新版已通过 Constants 安全获取盐值
            string salt = Tools.ExecuteWithMachineCodeSalt(s => s);
            string composite = $"{cpuId}_{motherboardId}_{salt}";
            return FormatMachineCode(Sha256(composite));
        }

        private static string FormatMachineCode(string hash)
        {
            hash = hash.Length >= 20 ? hash.Substring(0, 20) : hash.PadRight(25, '0');
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