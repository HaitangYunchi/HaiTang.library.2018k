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


using HaiTang.library.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HaiTang.library
{
    public static class Constants
    {
  
        // 软件验证相关变量
        public static string SOFTWARE_ID =string.Empty;
        public static string DEVELOPER_KEY = string.Empty;
        public static string LOCAL_MACHINE_CODE = string.Empty;
        public static Mysoft softwareInfo = new Mysoft();
        public static string EMAIL = string.Empty;
        public static string PASSWORD = string.Empty;
        public static bool CHECK = false;


        // 可用的API地址列表，用于故障转移
        public static readonly string[] ApiAddressList =
        {
            "http://api.2018k.cn",
            "http://api.haitangyunchi.cn",
            "http://api2.2018k.cn",
            "http://api3.2018k.cn",
            "http://api4.2018k.cn"
        };
    }
}
