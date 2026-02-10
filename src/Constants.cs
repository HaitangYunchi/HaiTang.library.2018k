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
using System.Collections.Generic;
using System.Text;

namespace HaiTang.Library.Api2018k
{
    /// <summary>
    ///基础变量和常量类，包含软件验证相关信息和API地址列表
    /// </summary>
    public static class Constants
    {

        /// <summary>
        /// 软件实例ID，用于标识不同的软件实例
        /// </summary>
        public static string SOFTWARE_ID = string.Empty;

        /// <summary>
        /// 开发者密钥，用于API请求的身份验证和加密
        /// </summary>
        public static string DEVELOPER_KEY = string.Empty;

        /// <summary>
        /// 本地机器码，用于绑定和验证软件使用权限
        /// </summary>
        public static string LOCAL_MACHINE_CODE = string.Empty;

        /// <summary>
        /// 软件信息配置对象，包含软件的详细配置信息
        /// </summary>
        public static Mysoft softwareInfo = new();

        /// <summary>
        /// 用户邮箱，用于用户登录和相关操作
        /// </summary>
        public static string EMAIL = string.Empty;

        /// <summary>
        /// 用户密码，用于用户登录和相关操作
        /// </summary>
        public static string PASSWORD = string.Empty;

        /// <summary>
        /// 检查状态标志，用于控制某些功能的启用或禁用
        /// </summary>
        public static bool CHECK = false;


        /// <summary>
        /// API服务器地址列表，用于实现多地址故障转移和负载均衡
        /// </summary>
        /// <remarks>
        /// 地址列表按优先级排序，当主地址不可用时，会自动尝试下一个可用地址。
        /// 每个地址都会进行健康检查，确保可用性。
        /// </remarks>
        public static readonly string[] ApiAddressList =
        {
            "http://api.2018k.cn",
            "http://api.haitangyunchi.cn",
            "http://api2.2018k.cn",
            "http://api3.2018k.cn",
            "http://api4.2018k.cn"
        };

        /// <summary>
        /// SDK 后台 API 基础 URL 地址，用于所有 API 请求的根路径
        /// </summary>
        public const string BaseUrl = "https://admin.2018k.cn/api/adm/";
    }
}
