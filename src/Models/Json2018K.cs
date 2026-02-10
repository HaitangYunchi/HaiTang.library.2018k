/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library.Models
 * 唯一标识：21246f58-bab9-4892-8185-3d4cca4398dd
 * 文件名：Json2018K
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 14:40:01
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


namespace HaiTang.Library.Api2018k.Models
{
    /// <summary>
    /// 网络获取的JSON数据
    /// </summary>
    public class Json2018K
    {
        /// <summary>
        /// 作者信息
        /// </summary>
        public string author { get; set; } = string.Empty;

        /// <summary>
        /// 是否强制更新
        /// </summary>
        public string mandatoryUpdate { get; set; } = string.Empty;

        /// <summary>
        /// 软件的MD5值
        /// </summary>
        public string softwareMd5 { get; set; } = string.Empty;

        /// <summary>
        /// 软件名称
        /// </summary>
        public string softwareName { get; set; } = string.Empty;

        /// <summary>
        /// 公告或通知内容
        /// </summary>
        public string notice { get; set; } = string.Empty;

        /// <summary>
        /// 版本信息
        /// </summary>
        public string versionInformation { get; set; } = string.Empty;

        /// <summary>
        /// 软件ID
        /// </summary>
        public string softwareId { get; set; } = string.Empty;

        /// <summary>
        /// 下载链接
        /// </summary>
        public string downloadLink { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public string versionNumber { get; set; } = string.Empty;

        /// <summary>
        /// 访问次数
        /// </summary>
        public string numberOfVisits { get; set; } = string.Empty;

        /// <summary>
        /// 最低支持版本
        /// </summary>
        public string miniVersion { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        public string timeStamp { get; set; } = string.Empty;

        /// <summary>
        /// 网络验证ID
        /// </summary>
        public string networkVerificationId { get; set; } = string.Empty;

        /// <summary>
        /// 是否有效
        /// </summary>
        public string isItEffective { get; set; } = string.Empty;

        /// <summary>
        /// 有效天数
        /// </summary>
        public string numberOfDays { get; set; } = string.Empty;

        /// <summary>
        /// 网络验证备注
        /// </summary>
        public string networkVerificationRemarks { get; set; } = string.Empty;

        /// <summary>
        /// 过期日期
        /// </summary>
        public string expirationDate { get; set; } = string.Empty;

        /// <summary>
        /// Bilibili链接
        /// </summary>
        public string bilibiliLink { get; set; } = string.Empty;

        /// <summary>
        /// 数据内容
        /// </summary>
        public string data { get; set; } = string.Empty;

        /// <summary>
        /// 作者信息
        /// </summary>
        public string user { get; set; } = string.Empty;

        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string message { get; set; } = string.Empty;

    }
    /// <summary>
    /// 转换后的Mysoft配置模型
    /// </summary>
    /// 
    public class Mysoft
    {
        /// <summary>
        /// 作者信息
        /// </summary>
        public string author { get; set; } = string.Empty;

        /// <summary>
        /// 软件名称
        /// </summary>
        public string softwareName { get; set; } = string.Empty;

        /// <summary>
        /// 软件的MD5值，用于验证文件的完整性
        /// </summary>
        public string softwareMd5 { get; set; } = string.Empty;

        /// <summary>
        /// 软件唯一标识符
        /// </summary>
        public string softwareId { get; set; } = string.Empty;

        /// <summary>
        /// 软件版本号
        /// </summary>
        public string versionNumber { get; set; } = string.Empty;

        /// <summary>
        /// 是否强制更新
        /// </summary>
        public bool mandatoryUpdate { get; set; } = false;

        /// <summary>
        /// 最低支持的软件版本号
        /// </summary>
        public string miniVersion { get; set; } = string.Empty;

        /// <summary>
        /// 软件访问次数统计
        /// </summary>
        public int numberOfVisits { get; set; } = 0;

        /// <summary>
        /// 时间戳，通常用于记录创建或更新时间
        /// </summary>
        public long timeStamp { get; set; } = 0;

        /// <summary>
        /// 网络验证是否有效
        /// </summary>
        public bool isItEffective { get; set; } = false;

        /// <summary>
        /// 网络验证的唯一标识符
        /// </summary>
        public string networkVerificationId { get; set; } = string.Empty;

        /// <summary>
        /// 网络验证的备注信息
        /// </summary>
        public string networkVerificationRemarks { get; set; } = string.Empty;

        /// <summary>
        /// 有效天数
        /// </summary>
        public int numberOfDays { get; set; } = 0;

        /// <summary>
        /// 过期时间戳
        /// </summary>
        public long expirationDate { get; set; } = 0;

        /// <summary>
        /// 软件下载链接
        /// </summary>
        public string downloadLink { get; set; } = string.Empty;

        /// <summary>
        /// 公告或通知内容
        /// </summary>
        public string notice { get; set; } = string.Empty;

        /// <summary>
        /// 版本详细信息说明
        /// </summary>
        public string versionInformation { get; set; } = string.Empty;

        /// <summary>
        /// Bilibili空间链接，默认值指向官方B站空间
        /// </summary>
        public string bilibiliLink { get; set; } = "https://space.bilibili.com/3493128132626725";


    }
    /// <summary>
    /// 用户信息模型
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 客户唯一标识符
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// 用户头像的URL地址
        /// </summary>
        public string AvatarUrl { get; set; } = string.Empty;

        /// <summary>
        /// 用户昵称显示名称
        /// </summary>
        public string Nickname { get; set; } = string.Empty;

        /// <summary>
        /// 用户电子邮箱地址
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 用户账户余额（单位：分）
        /// </summary>
        public int Balance { get; set; } = 0;

        /// <summary>
        /// 用户许可证或授权码
        /// </summary>
        public string License { get; set; } = string.Empty;

        /// <summary>
        /// 时间加密字符串，用于验证时间戳的有效性
        /// </summary>
        public string TimeCrypt { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳，通常用于记录操作时间
        /// </summary>
        public string Timestamp { get; set; } = string.Empty;

    }

    /// <summary>
    /// 用户相关操作的API响应类
    /// </summary>
    public class JsonUser
    {

        /// <summary>
        /// 响应状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应消息描述
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 用户信息数据对象
        /// 包含用户的详细信息，如ID、昵称、邮箱等
        /// </summary>
        public UserInfo Data { get; set; } = new(); // 注意：这里是一个类
    }

}
