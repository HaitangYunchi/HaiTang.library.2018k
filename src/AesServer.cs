/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library
 * 唯一标识：5f3ea9e8-75e3-4ba8-b21b-2edd3fe37098
 * 文件名：AesServer
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 14:07:33
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


using System.Security.Cryptography;
using System.Text;

namespace HaiTang.library
{
    // <summary>
    /// 基于AES加密算法和盐值的加密工具类
    /// </summary>
    #region 
    #endregion
    #region AES加密类 自动IV
    public class AutoAesHelper
    {
        /// <summary>
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
        /// <param name="combinedData">Base64编码的加密数据（包含IV和密文）</param>
        /// <param name="key">AES密钥（十六进制字符串）</param>
        /// <returns>解密后的明文</returns>
        public static string Decrypt(string combinedData, string key)
        {
            byte[] fullData = Convert.FromBase64String(combinedData);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);

                // 提取IV（前16字节）
                byte[] iv = new byte[16];
                Array.Copy(fullData, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                // 提取密文（剩余部分）
                byte[] cipherText = new byte[fullData.Length - 16];
                Array.Copy(fullData, 16, cipherText, 0, cipherText.Length);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
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
    }
    #endregion
    #region AES加密类 自动IV带盐值和密码
    public static class SaltAesHelper
    {
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

        
    }
    #endregion
}
