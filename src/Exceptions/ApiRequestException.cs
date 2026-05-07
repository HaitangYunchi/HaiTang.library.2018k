using System;

namespace HaiTang.Library.Api2018k.Exceptions
{
    /// <summary>
    /// API请求异常类
    /// </summary>
    public class ApiRequestException : Exception
    {
        /// <summary>
        /// API地址
        /// </summary>
        public string ApiUrl { get; }

        /// <summary>
        /// 是否是开发模式错误
        /// </summary>
        public bool IsDevelopmentMode { get; }

        public ApiRequestException(string message, string apiUrl, bool isDevelopmentMode = false)
            : base(message)
        {
            ApiUrl = apiUrl;
            IsDevelopmentMode = isDevelopmentMode;
        }

        public ApiRequestException(string message, Exception innerException, string apiUrl, bool isDevelopmentMode = false)
            : base(message, innerException)
        {
            ApiUrl = apiUrl;
            IsDevelopmentMode = isDevelopmentMode;
        }
    }
}
