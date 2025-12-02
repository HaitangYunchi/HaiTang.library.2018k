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


using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HaiTang.library
{
    public class Hasher
    {
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
