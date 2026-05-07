/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library.Utils
 * 唯一标识：0f7476c6-a34d-4e8b-9bc1-7e4af51318a7
 * 文件名：Constants
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 15:17:11
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

using HaiTang.Library.Api2018k.Models;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace HaiTang.Library.Api2018k
{
    /// <summary>
    /// 基础变量和常量类，包含软件验证相关信息和API地址列表。
    /// 所有敏感信息均使用 SecureString 存储，内存无明文。
    /// </summary>
    public static class Constants
    {
        

        #region 非敏感公共字段

        /// <summary>本地机器码，用于绑定和验证软件使用权限</summary>
        public static string LOCAL_MACHINE_CODE = string.Empty;

        /// <summary>软件信息配置对象，包含软件的详细配置信息</summary>
        public static Mysoft softwareInfo = new Mysoft();

        /// <summary>用户邮箱，用于用户登录和相关操作</summary>
        public static string EMAIL = string.Empty;

        /// <summary>用户密码，用于用户登录和相关操作</summary>
        public static string PASSWORD = string.Empty;

        /// <summary>检查状态标志，用于控制某些功能的启用或禁用</summary>
        public static bool CHECK = false;
        /// <summary>
        /// 开发模式标志
        /// <para>当此值为 true 时，系统将使用开发环境的配置</para>
        /// <para>包括使用本地 API 地址、启用调试功能等</para>
        /// </summary>
        /// <remarks>
        /// 在开发过程中，可以将此值设置为 true 以使用开发环境配置
        /// 在生产环境中，应确保此值为 false
        /// </remarks>
        public static bool DEVELOPMENT_MODE = false;

        /// <summary>
        /// 开发环境的 API 服务器地址
        /// </summary>
        /// <remarks>
        /// 此地址指向本地开发服务器
        /// 仅当 DEVELOPMENT_MODE 为 true 时有效
        /// 生产环境应使用正式的 API 地址
        /// </remarks>
        public static string DEVELOPMENT_API_URL = "http://127.0.0.1:3000";
        /// <summary>API服务器地址列表，用于实现多地址故障转移和负载均衡</summary>
        public static readonly string[] ApiAddressList =
        {
            "http://api.2018k.cn",
            "http://api.haitangyunchi.cn",
            "http://api2.2018k.cn",
            "http://api3.2018k.cn",
            "http://api4.2018k.cn"
        };

        /// <summary>SDK 后台 API 基础 URL 地址</summary>
        public static string AdminBaseUrl => DEVELOPMENT_MODE ? $"{DEVELOPMENT_API_URL}/api/adm/" : "https://admin.2018k.cn/api/adm/";

        /// <summary>
        /// 获取当前使用的 API 地址（根据开发模式动态返回）
        /// </summary>
        public static string CurrentApiUrl => DEVELOPMENT_MODE ? DEVELOPMENT_API_URL : ApiAddressList[0];

        #endregion

        
    }
}