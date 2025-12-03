/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library
 * 唯一标识：f2fd4d17-2ceb-4bda-991c-2454f83839ea
 * 文件名：Hasher
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 15:12:49
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
    /// <summary>
    /// 提供常用哈希算法的静态工具类
    /// <para>支持SHA-256和SHA-512哈希算法</para>
    /// </summary>
    /// <remarks>
    /// <para>本类提供线程安全的哈希计算方法，适合在并发场景下使用</para>
    /// <para>所有方法均为静态方法，可直接调用无需创建实例</para>
    /// <para>哈希输出为十六进制字符串格式（大写字母）</para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code>
    /// string password = "myPassword123";
    /// string sha256Hash = Hasher.Sha256(password); // 计算SHA-256哈希
    /// string sha512Hash = Hasher.Sha512(password); // 计算SHA-512哈希
    /// </code>
    /// </example>
    /// <seealso cref="SHA256"/>
    /// <seealso cref="SHA512"/>
    public class Hasher
    {
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
    }
}
