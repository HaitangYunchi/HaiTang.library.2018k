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


using System;
using System.Collections.Generic;
using System.Text;

namespace HaiTang.library.Models
{
    public class Json2018K
    {
        /// <summary>
        /// 原始JSON数据模型（从网络获取）
        /// </summary>
        public string author { get; set; } = string.Empty;
        public string mandatoryUpdate { get; set; } = string.Empty;
        public string softwareMd5 { get; set; } = string.Empty;
        public string softwareName { get; set; } = string.Empty;
        public string notice { get; set; } = string.Empty;
        public string versionInformation { get; set; } = string.Empty;
        public string softwareId { get; set; } = string.Empty;
        public string downloadLink { get; set; } = string.Empty;
        public string versionNumber { get; set; } = string.Empty;
        public string numberOfVisits { get; set; } = string.Empty;
        public string miniVersion { get; set; } = string.Empty;
        public string timeStamp { get; set; } = string.Empty;
        public string networkVerificationId { get; set; } = string.Empty;
        public string isItEffective { get; set; } = string.Empty;
        public string numberOfDays { get; set; } = string.Empty;
        public string networkVerificationRemarks { get; set; } = string.Empty;
        public string expirationDate { get; set; } = string.Empty;
        public string bilibiliLink { get; set; } = string.Empty;
        public string data { get; set; } = string.Empty;
        public string user { get; set; } = string.Empty;
        public int code { get; set; }
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
    }
    /// <summary>
    /// 转换后的Mysoft配置模型
    /// </summary>
    public class Mysoft
    {
        public string author { get; set; } = string.Empty;
        public string softwareName { get; set; } = string.Empty;
        public string softwareMd5 { get; set; } = string.Empty;
        public string softwareId { get; set; } = string.Empty;
        public string versionNumber { get; set; } = string.Empty;
        public bool mandatoryUpdate { get; set; } = false;
        public string miniVersion { get; set; } = string.Empty;
        public int numberOfVisits { get; set; } = 0;
        public long timeStamp { get; set; } = 0;
        public bool isItEffective { get; set; } = false;
        public string networkVerificationId { get; set; } = string.Empty;
        public string networkVerificationRemarks { get; set; } = string.Empty;
        public int numberOfDays { get; set; } = 0;
        public long expirationDate { get; set; } = 0;
        public string downloadLink { get; set; } = string.Empty;
        public string notice { get; set; } = string.Empty;
        public string versionInformation { get; set; } = string.Empty;
        public string bilibiliLink { get; set; } = "https://space.bilibili.com/3493128132626725";

    }
    public class UserInfo
    {
        public string CustomerId { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Balance { get; set; } = 0;
        public string License { get; set; } = string.Empty; 
        public string TimeCrypt { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
    public class JsonUser
    {
        public int Code { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserInfo Data { get; set; }  // 注意：这里不是 string，而是另一个类
    }

}
