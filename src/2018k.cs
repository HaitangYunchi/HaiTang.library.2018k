/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间：HaiTang.library
 * 唯一标识：62a74e2f-ef1d-4b37-956d-2e572887051c
 * 文件名：2018k
 * 
 * 创建者：海棠云螭
 * 电子邮箱：haitangyunchi@126.com
 * 创建时间：2025/12/1 14:04:17
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
//using Formatting = Newtonsoft.Json.Formatting;


namespace HaiTang.library
{
    /// <summary>
    /// 提供与软件更新、用户管理、卡密验证、云变量操作等相关的 API 封装方法。
    /// 支持多 API 地址故障转移、健康检测、加密解密等功能。
    /// </summary>
    public class Update
    {
        #region 常量定义

        /// <summary>
        /// 通用错误信息字符串,表示空或无效结果。
        /// </summary>
        private static readonly string _error = "<空>";
        /// <summary>
        /// 通用错误信息字符串,表示空或无效结果。
        /// </summary>
        private static readonly string _worring = "错误：无法获取用户相关信息,请检查登录信息和系统时间是否正确";
        private readonly HttpClient _httpClient = new();
        private const string DefaultApiUrl = "http://api.2018k.cn";
        private static string OpenApiUrl = DefaultApiUrl;
        private static string LocalApiUrl = "127.0.0.1";

        // 用于存储当前API地址的索引
        private static int currentApiIndex = 0;
        // 记录每个API地址的健康状态和最后检测时间
        private static readonly Dictionary<string, ApiHealthStatus> apiHealthStatus = new Dictionary<string, ApiHealthStatus>();
        // 健康状态缓存时间（5分钟）
        private static readonly TimeSpan healthCacheDuration = TimeSpan.FromMinutes(5);
        // 健康检测超时时间（5秒）
        private static readonly TimeSpan healthCheckTimeout = TimeSpan.FromSeconds(5);
        // 锁对象,确保线程安全
        private static readonly object lockObject = new object();
        // 用于健康检测的HttpClient
        private static readonly HttpClient healthCheckClient = new HttpClient() { Timeout = healthCheckTimeout };
        #endregion

        #region 静态缓存字段和方法
        // 静态缓存字段
        private static Mysoft? _cachedSoftwareInfo = null;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly object _cacheLock = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private static UserInfo? _cachedUserInfo = null;
        private static DateTime _userLastCacheTime = DateTime.MinValue;
        private static readonly object _UserCacheLock = new object();
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 获取缓存的软件信息（线程安全）
        /// </summary>
        public static Mysoft GetCachedSoftwareInfo()
        {
            lock (_cacheLock)
            {
                // 保证不返回 null，若为 null 则返回一个空对象
                return _cachedSoftwareInfo ?? new Mysoft();
            }
        }

        /// <summary>
        /// 设置缓存的软件信息（线程安全）
        /// </summary>
        public static void SetCachedSoftwareInfo(Mysoft softwareInfo)
        {
            lock (_cacheLock)
            {
                _cachedSoftwareInfo = softwareInfo;
                _lastCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 检查缓存是否有效（线程安全）
        /// </summary>
        public static bool IsCacheValid()
        {
            lock (_cacheLock)
            {
                return _cachedSoftwareInfo != null &&
                       DateTime.Now - _lastCacheTime < CacheDuration;
            }
        }

        /// <summary>
        /// 清除缓存（线程安全）
        /// </summary>
        public static void ClearStaticCache()
        {
            lock (_cacheLock)
            {
                _cachedSoftwareInfo = null;
                _lastCacheTime = DateTime.MinValue;
            }
        }





        /// <summary>
        /// 获取缓存的用户信息（线程安全）
        /// </summary>
        public static UserInfo GetCachedUserInfo()
        {
            lock (_UserCacheLock)
            {
                return _cachedUserInfo ?? new UserInfo(); ;
            }
        }

        /// <summary>
        /// 设置缓存的软件信息（线程安全）
        /// </summary>
        public static void SetCachedUserInfo(UserInfo userInfo)
        {
            lock (_UserCacheLock)
            {
                _cachedUserInfo = userInfo;
                _userLastCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 检查缓存是否有效（线程安全）
        /// </summary>
        public static bool IsUserCacheValid()
        {
            lock (_UserCacheLock)
            {
                return _cachedUserInfo != null &&
                       DateTime.Now - _userLastCacheTime < UserCacheDuration;
            }
        }

        /// <summary>
        /// 清除缓存（线程安全）
        /// </summary>
        public static void ClearUserCache()
        {
            lock (_UserCacheLock)
            {
                _cachedUserInfo = null;
                _userLastCacheTime = DateTime.MinValue;
            }
        }
        #endregion

        #region 软件实例方法
        

        /// <summary>
        /// 检测实例是否正常 （ 程序实例ID,机器码 [null] ）
        /// </summary>
        /// <param name="ID">程序实例ID</param>
        /// <param name="key">OpenID</param>
        /// <param name="Code">机器码,可以省略</param>
        /// <returns>返回布尔值 如果 Code 为空,机器码为空时,使用自带的机器码</returns>
        public async Task<bool> GetSoftCheck(string ID, string key, string? Code = null)
        {
            string _result;
            if (string.IsNullOrEmpty(Code))
            {
                Code = Tools.GetMachineCodeEx();
            }
            Constants.SOFTWARE_ID = ID;        // 设置
            Constants.DEVELOPER_KEY = key;     // 设置
            Constants.LOCAL_MACHINE_CODE = Code;
            _result = await ExecuteApiRequest(async (apiUrl) =>
            {
                using (HttpClient httpClient = new())
                {
                    // 构建请求URL
                    string requestUrl = $"{apiUrl}/v3/obtainSoftware?softwareId={ID}&machineCode={Code}&isAPI=y";
                    // 发送GET请求
                    HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        return "false";
                    }
                    else
                    {
                        // 读取响应内容
                        string jsonString = await response.Content.ReadAsStringAsync();
                        Json2018K? _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);

                        try
                        {
                            // 尝试解密数据,失败则直接返回 false
                            string JsonData = _JsonData?.data != null ? AesDecrypt(_JsonData.data, key) : string.Empty;
                            Json2018K? _Data = JsonConvert.DeserializeObject<Json2018K>(JsonData);
                            if (_Data != null)
                            {
                                Mysoft config = ConvertToMysoftConfig(_Data);
                                return (_Data != null && _Data.user != null) ? "true" : "false";
                            }
                            else
                            {
                                return "false";
                            }
                        }
                        catch
                        {
                            return "false";
                        }
                    }
                }
            });

            return bool.TryParse(_result, out bool result) && result; // 解析失败也返回 false
        }

        /// <summary>
        /// 初始化 后续方法都是在初始化后调用 ( 程序实例ID,OpenID,机器码 [null] )
        /// </summary>
        /// <param name="ID">程序实例ID</param>
        /// <param name="key">OpenID</param>
        /// <param name="Code">机器码,可以省略</param>
        /// <returns>返回 Mysoft类 机器码为空时,使用自带的机器码</returns>
        public async Task<Mysoft> InitializationAsync(string ID, string key, string? Code = null)
        {
            if (string.IsNullOrEmpty(Code))
            {
                Code = Tools.GetMachineCodeEx();
            }
            Constants.SOFTWARE_ID = ID;        // 设置
            Constants.DEVELOPER_KEY = key;     // 设置
            Constants.LOCAL_MACHINE_CODE = Code;
            // 检查缓存是否有效
            if (IsCacheValid())
            {
                return GetCachedSoftwareInfo();
            }

            // 获取新数据
            var softwareInfo = await GetSoftwareInfoAsync();

            if (softwareInfo != null)
            {
                SetCachedSoftwareInfo(softwareInfo);
            }

            return softwareInfo;
        }

        /// <summary>
        /// 获取软件全部信息
        /// </summary>
        /// <returns>返回 Json 字符串</returns>
        public async Task<string> GetSoftAll()
        {
            // 获取Mysoft对象
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);

            if (softwareInfo == null)
            {
                return _error;
            }

            // 将Mysoft对象序列化为格式化的JSON字符串
            return JsonConvert.SerializeObject(softwareInfo, Formatting.Indented);
        }

        /// <summary>
        /// 获取软件实例ID
        /// </summary>
        /// <returns>string 返回实例ID</returns>
        public async Task<string> GetSoftwareID()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.softwareId ?? _error;
        }

        /// <summary>
        /// 获取软件版本
        /// </summary>
        /// <returns>string 返回软件版本号</returns>
        public async Task<string> GetVersionNumber()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.versionNumber ?? _error;
        }

        /// <summary>
        /// 获取软件名称
        /// </summary>
        /// <returns>string 返回软件名称</returns>
        public async Task<string> GetSoftwareName()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.softwareName ?? _error;
        }

        /// <summary>
        /// 获取软件更新内容
        /// </summary>
        /// <returns>string 返回软件更新信息</returns>
        public async Task<string> GetVersionInformation()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.versionInformation ?? _error;
        }

        /// <summary>
        /// 获取软件公告
        /// </summary>
        /// <returns>string 返回软件公告信息</returns>
        public async Task<string> GetNotice()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.notice ?? _error;
        }

        /// <summary>
        /// 获取软件下载链接
        /// </summary>
        /// <returns>string 返回软件下载链接</returns>
        public async Task<string> GetDownloadLink()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.downloadLink ?? _error;
        }

        /// <summary>
        /// 获取软件访问量
        /// </summary>

        /// <returns>string 返回软件访问量数据 非实时</returns>
        public async Task<string> GetNumberOfVisits()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.numberOfVisits.ToString() ?? _error;
        }

        /// <summary>
        /// 获取软件最低版本号
        /// </summary>
        /// <returns>string 返回软件最低版本号,机器码可空</returns>
        public async Task<string> GetMiniVersion()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.miniVersion ?? _error;
        }

        /// <summary>
        /// 获取卡密状
        /// </summary>
        /// <returns>bool 返回卡密当前状态是否有效, 一般为判断软件是否注册 True  , False </returns>
        public async Task<bool> GetIsItEffective()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.isItEffective ?? false;
        }

        /// <summary>
        /// 获取卡密过期时间戳
        /// </summary>
        /// <returns>string 返回软件卡密时间戳</returns>
        public async Task<string> GetExpirationDate()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.expirationDate.ToString() ?? _error;
        }

        /// <summary>
        /// 获取卡密备注
        /// </summary>
        /// <returns>string 返回卡密备注</returns>
        public async Task<string> GetRemarks()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.networkVerificationRemarks ?? _error;
        }

        /// <summary>
        /// 获取卡密有效期类型
        /// </summary>
        /// <returns>string 返回卡密有效期类型, 卡密有效期天数</returns>
        public async Task<string> GetNumberOfDays()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.numberOfDays.ToString() ?? _error;
        }

        /// <summary>
        /// 获取卡密ID
        /// </summary>
        /// <returns>string 返回卡密ID</returns>
        public async Task<string> GetNetworkVerificationId()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.networkVerificationId ?? _error;
        }

        /// <summary>
        /// 获取服务器时间 
        /// </summary>
        /// <returns>string 返回服务器时间, 时间戳</returns>
        public async Task<string> GetTimeStamp()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.timeStamp.ToString() ?? _error;
        }

        /// <summary>
        /// 获取软件是否强制更新
        /// </summary>
        /// <returns>bool 返回软件是否强制更新</returns>
        public async Task<bool> GetMandatoryUpdate()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.mandatoryUpdate ?? false;
        }

        /// <summary>
        /// 获取软件MD5 
        /// </summary>
        /// <returns>string 返回软件MD5</returns>
        public async Task<string> GetSoftwareMd5()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);
            return softwareInfo?.softwareMd5 ?? _error;
        }

        /// <summary>
        /// 获取云变量 （ 程序实例ID,OpenID,云端变量名称 ）
        /// </summary>
        /// <param name="VarName">云端变量名称</param>
        /// <returns>string 返回云变量的值</returns>
        public async Task<string> GetCloudVariables(string VarName)
        {
            bool _Check = await GetSoftCheck(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY);
            if (_Check == false)
            {
                return _error;
            }
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                using (HttpClient httpClient = new())
                {
                    // 构建API请求URL
                    string requestUrl = $"{apiUrl}/v3/getCloudVariables?softwareId={Constants.SOFTWARE_ID}&isAPI=y";

                    // 发送GET请求
                    HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);
                    // 解密数据
                    string JsonData = _JsonData?.data != null ? AesDecryptData(_JsonData.data, Constants.DEVELOPER_KEY) : string.Empty;

                    // 解析JSON数组
                    JArray jsonArray = JArray.Parse(JsonData);
                    List<KeyValuePair<string, string>> configList = new List<KeyValuePair<string, string>>();

                    // 遍历JSON数据
                    foreach (JObject item in jsonArray)
                    {
                        string CloudKey = item["key"]?.ToString() ?? string.Empty;
                        string CloudValue = item["value"]?.ToString() ?? string.Empty;
                        configList.Add(new KeyValuePair<string, string>(CloudKey, CloudValue));
                    }

                    // 查找指定变量名
                    var _Var = configList.FirstOrDefault(p => p.Key == VarName);
                    string CloudVar = _Var.Value;
                    return CloudVar;
                }
            });
        }

        /// <summary>
        /// 修改云变量,如果变量不存在则新增
        /// </summary>
        /// <param name="VarKey">云端变量名称</param>
        /// <param name="Value">要设置的变量值</param>
        /// <returns> 返回bool success 和 string message </returns>
        public async Task<(bool success, string message)> updateCloudVariables(string VarKey, string Value)
        {
            try
            {
                bool _response = await ExecuteApiRequest(async (apiUrl) =>
                {
                    // 构建JSON对象
                    var data = new JObject
                    {
                        ["key"] = VarKey,
                        ["value"] = Value
                    };

                    string encryptedData = AesEncrypt(data, Constants.DEVELOPER_KEY);
                    // 构建请求URL
                    string requestUrl = $"{apiUrl}/v3/updateCloudVariables?info={Uri.EscapeDataString(encryptedData)}&softwareId={Constants.SOFTWARE_ID}&isAPI=y";
                    HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                    // 确保请求成功
                    response.EnsureSuccessStatusCode();
                    // 获取响应内容并格式化
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(responseContent);
                    return (_JsonData?.success ?? false) ? "true" : "false";
                }) == "true";
                return (_response, _response ? $"{{\"key\":\"{VarKey}\",\"value\":\"{Value}\"}}" : $"失败");
            }
            catch
            {
                Log.Error("修改云变量失败,网络异常或程序错误");
                return (false, "失败,网络异常或程序错误");
            }
        }

        /// <summary>
        /// 激活软件  ( 卡密ID )
        /// </summary>
        /// <param name="authId">卡密ID</param>
        /// <returns>返回bool success 和 string message</returns>
        public async Task<(bool success, string message)> ActivationKey(string authId)
        {
            try
            {
                bool _response = await ExecuteApiRequest(async (apiUrl) =>
                {
                    string url = $"{apiUrl}/v3/activation?authId={authId}&softwareId={Constants.SOFTWARE_ID}&machineCode={Constants.LOCAL_MACHINE_CODE}&isAPI=y";
                    // 发送 GET 请求
                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(responseContent);
                    return (_JsonData?.success ?? false) ? "true" : "false";
                }) == "true";
                return (_response, _response ? "授权成功" : "授权失败,请检查卡密是否正确或已被使用");
            }
            catch
            {
                Log.Error("授权失败,网络异常或程序错误");
                return (false, "失败,网络异常或程序错误");
            }
        }

        /// <summary>
        /// 发送消息  ( 要发送的消息 )
        /// </summary>
        /// <param name="message">要发送的消息</param>
        /// <returns>返回json</returns>
        public async Task<string> MessageSend(string message)
        {
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                message = Uri.EscapeDataString(message);
                string url = $"{apiUrl}/v3/messageSend?softwareId={Constants.SOFTWARE_ID}&message={message}&isAPI=y";
                // 发送 GET 请求
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                // 获取响应内容并格式化
                string responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    // 尝试将响应内容解析为 JSON 对象并格式化
                    var jsonObject = JsonConvert.DeserializeObject(responseContent);
                    return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                }
                catch
                {
                    // 如果解析失败,返回原始内容
                    return responseContent;
                }
            });
        }

        /// <summary>
        /// 创建卡密  （ 卡密天数,卡密备注,程序实例ID）
        /// </summary>
        /// <param name="day">卡密天数</param>
        /// <param name="remark">卡密备注</param>
        /// <param name="ID">程序实例ID</param>
        /// <returns>返回JSON</returns>
        public async Task<string> CreateNetworkAuthentication(int day, string remark,string ID)
        {
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                // 构建请求数据
                var data = new
                {
                    day,
                    remark,
                    times = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds
                };

                // 加密数据
                string encodedCiphertext = AesEncrypt(data, Constants.DEVELOPER_KEY);

                // 发送请求
                string url = $"{apiUrl}/v3/createNetworkAuthentication?info={Uri.EscapeDataString(encodedCiphertext)}&softwareId={ID}&isAPI=y";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // 获取响应内容并格式化
                string responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    // 尝试将响应内容解析为 JSON 对象并格式化
                    var jsonObject = JsonConvert.DeserializeObject(responseContent);
                    return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                }
                catch
                {
                    // 如果解析失败,返回原始内容
                    return responseContent;
                }
            });
        }

        /// <summary>
        /// 解绑、换绑 机器码为空则解绑 (卡密ID,机器码)
        /// </summary>
        /// <param name="AuthId">卡密ID</param>
        /// <param name="Code">机器码</param>
        /// <returns>返回bool success 和 string message</returns>
        public async Task<(bool success, string message)> ReplaceBind(string AuthId, string? Code = null)
        {
            try
            {
                bool _response = await ExecuteApiRequest(async (apiUrl) =>
                {
                    // 构建请求数据
                    var data = new
                    {
                        authId = AuthId,
                        machineCode = Code
                    };
                    // 加密数据
                    string encodedCiphertext = AesEncrypt(data, Constants.DEVELOPER_KEY);
                    // 发送请求
                    string url = $"{apiUrl}/v3/replaceBind?softwareId={Constants.SOFTWARE_ID}&info={Uri.EscapeDataString(encodedCiphertext)}&isAPI=y";
                    HttpResponseMessage response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // 获取响应内容并格式化
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(responseContent);
                    return (_JsonData?.success ?? false) ? "true" : "false";
                }) == "true";
                return (_response, _response ? "解|换绑成功" : "解|换绑失败,请检查卡密ID是否正确");
            }
            catch
            {
                Log.Error("解绑|换绑失败,网络异常或程序错误");
                return (false, "解绑|换绑失败,网络异常或程序错误");
            }
        }

        /// <summary>
        /// 获取剩余使用时间
        /// </summary>
        /// <returns>长整数类型long 永久返回-1,过期返回0,未注册返回1,其余返回时间戳,</returns>
        public async Task<long> GetRemainingUsageTime()
        {
            var softwareInfo = await InitializationAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.LOCAL_MACHINE_CODE);

            if (softwareInfo == null)
            {
                return 1; // 未注册
            }

            try
            {
                if (softwareInfo.isItEffective && softwareInfo.expirationDate == 7258089599000)
                {
                    return -1; // 永久
                }
                else if (softwareInfo.isItEffective && softwareInfo.expirationDate > 0)
                {
                    long currentTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    long timestamp = softwareInfo.expirationDate - currentTimestamp;
                    if (timestamp > 0)
                    {
                        return timestamp;
                    }
                    else
                    {
                        return 0; // 已过期
                    }
                }
                else
                {
                    return 1; // 未注册
                }
            }
            catch
            {
                return 0; // 异常情况
            }
        }

        /// <summary>
        /// 获取网络验证码
        /// </summary>
        /// <returns>string 返回验证码</returns>
        public async Task<string> GetNetworkCode()
        {
            bool _logon = await GetSoftCheck(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY);
            if (_logon == false)
            {
                return _error;
            }
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                var requestData = new
                {
                    softwareId = Constants.SOFTWARE_ID
                };
                // 序列化为 JSON
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);

                // 使用 HttpClient 发送 POST 请求
                using (HttpClient client = new HttpClient())
                {
                    // 设置请求头（Content-Type）
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // 构造 StringContent（请求体）
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(apiUrl + "/v3/captcha", content);
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);
                    string JsonData = _JsonData?.data != null ? AesDecryptData(_JsonData.data, Constants.DEVELOPER_KEY) : string.Empty;
                    return JsonData;
                }
            });
        }
        #endregion

        #region 用户方法
        /// <summary>
        /// 用户注册  ( 程序实例ID,邮箱,密码,昵称,验证码 )
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="password">密码</param>
        /// <param name="avatarUrl">用户头像</param>
        /// <param name="nickName">昵称</param>
        /// <param name="captcha">验证码</param>
        /// <returns>返回 布尔类型 True 或 Fales [昵称,头像,验证码]可空</returns>
        public async Task<bool> CustomerRegister(string email, string password, string? nickName = null, string? avatarUrl = null, string? captcha = null)
        {

            string _data = await ExecuteApiRequest(async (apiUrl) =>
            {
                var requestData = new
                {
                    softwareId = Constants.SOFTWARE_ID,
                    email,
                    password,
                    avatarUrl,
                    nickName,
                    captcha

                };

                // 序列化为 JSON
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);

                // 使用 HttpClient 发送 POST 请求
                using (HttpClient client = new HttpClient())
                {
                    // 设置请求头（Content-Type）
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // 构造 StringContent（请求体）
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(apiUrl + "/v3/customerRegister", content);
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);
                    string JsonData = (_JsonData != null ? _JsonData.success.ToString() : "false");
                    return JsonData;
                }
            });
            return bool.TryParse(_data, out var result) && result;
        }


        /// <summary>
        /// 初始化 USER 后续方法都是在初始化后调用 ( 程序实例ID,OpenID,Email,PassWord )
        /// </summary>
        /// <param name="ID">程序实例ID</param>
        /// <param name="key">OpenID</param>
        /// <param name="email">EMAIL</param>
        /// <param name="password">PASSWORD</param>
        /// <returns>返回 User 类</returns>
        public async Task<UserInfo> InitializationUserAsync(string ID, string key, string email, string password)
        {
            Constants.SOFTWARE_ID = ID;        // 设置
            Constants.DEVELOPER_KEY = key;     // 设置
            Constants.LOCAL_MACHINE_CODE = Tools.GetMachineCodeEx();
            Constants.EMAIL = email;
            Constants.PASSWORD = password;

            // 检查缓存是否有效
            if (IsUserCacheValid())
            {
                return GetCachedUserInfo();
            }

            // 获取新数据
            var userInfo = await GetUserInfoAsync();

            if (userInfo != null)
            {
                SetCachedUserInfo(userInfo);
            }

            return userInfo;
        }

        /// <summary>
        /// 获取用户所有信息
        /// </summary>
        /// <returns>返回JSON</returns>
        public async Task<string> GetUserInfo()
        {
             var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            if (userInfo == null)
            {
                return _worring;
            }
            return JsonConvert.SerializeObject(userInfo, Formatting.Indented);
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <returns>返回string类型</returns>
        public async Task<string> GetUserId()
        {
            var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            return userInfo.CustomerId ?? _worring;
        }

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <returns>返回string类型</returns>
        public async Task<string> GetUserAvatar()
        {
           var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            return userInfo.AvatarUrl ?? _worring;
        }

        /// <summary>
        /// 获取用户昵称
        /// </summary>
        /// <returns>返回string类型</returns>
        public async Task<string> GetUserNickname()
        {
            var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            return userInfo.Nickname ?? _worring;
        }

        /// <summary>
        /// 获取用户邮箱
        /// </summary>
        /// <returns>返回string类型</returns>
        public async Task<string> GetUserEmail()
        {
            var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            return userInfo.Email ?? _worring;
        }

        /// <summary>
        /// 获取账户剩余时长
        /// </summary>
        /// <returns>返回string类型</returns>
        public async Task<string> GetUserBalance()
        {
            var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            return userInfo.Balance.ToString() ?? _worring;
        }

        /// <summary>
        /// 是否授权
        /// </summary>
        /// <returns>返回布尔类型</returns>
        public async Task<bool> GetUserLicense()
        {
            var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            if (userInfo.License == "y")
            {
                return true;
            }
            else
            {
              return false;
            }
        }

        /// <summary>
        /// 获取用户登录时间戳
        /// </summary>
        /// <returns>string 返回时间戳</returns>
        public async Task<string> GetUserTimeCrypt()
        {
            var userInfo = await InitializationUserAsync(Constants.SOFTWARE_ID, Constants.DEVELOPER_KEY, Constants.EMAIL, Constants.PASSWORD);
            return userInfo.TimeCrypt ?? _worring;
        }

        /// <summary>
        /// 卡密充值  ( 卡密ID )
        /// </summary>
        /// <param name="AuthId">卡密ID</param>
        /// <returns>string 返回验证码</returns>
        public async Task<string> Recharge(string AuthId)
        {
            var _customerId = await GetUserId();
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                var requestData = new
                {
                    customerId = _customerId.ToString(),
                    authId = AuthId
                };

                // 序列化为 JSON
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);

                // 使用 HttpClient 发送 POST 请求
                using (HttpClient client = new HttpClient())
                {
                    // 设置请求头（Content-Type）
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // 构造 StringContent（请求体）
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(apiUrl + "/v3/customerRecharge", content);
                    string jsonString = await response.Content.ReadAsStringAsync();
                    //var _JsonData = JsonConvert.DeserializeObject<Json>(jsonString);
                    return jsonString;

                }
            });
        }
        #endregion

        #region 加密解密
        /// <summary>
        /// 使用AES算法加密指定的数据对象。
        /// </summary>
        /// <param name="data">要加密的数据对象,将被序列化为JSON字符串。</param>
        /// <param name="key">加密密钥,十六进制字符串。</param>
        /// <returns>加密后的Base64字符串。</returns>
        public string AesEncrypt(object data, string key)
        {
            // 将数据转换为JSON字符串
            string plaintext = JsonConvert.SerializeObject(data);

            // 使用AES加密
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);
                aesAlg.IV = new byte[16]; // 16字节全零IV
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 创建加密器
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // 加密数据
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plaintext);
                        }
                        byte[] encrypted = msEncrypt.ToArray();

                        // 转换为Base64字符串
                        return Convert.ToBase64String(encrypted);
                    }
                }
            }
        }

        /// <summary>
        /// 使用AES算法解密指定的Base64加密字符串。
        /// </summary>
        /// <param name="encryptedData">加密后的Base64字符串。</param>
        /// <param name="key">解密密钥,十六进制字符串。</param>
        /// <returns>解密后的字符串,如果解密失败则返回异常信息。</returns>
        public string AesDecrypt(string encryptedData, string key)
        {

            try
            {
                // 将Base64密文转换为字节数组
                byte[] cipherBytes = Convert.FromBase64String(encryptedData);

                // 创建AES解密器
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = HexStringToByteArray(key); ;
                    aesAlg.IV = new byte[16];
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    // 创建解密器
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // 执行解密
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                // 返回解密后的UTF8字符串
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"程序异常: {ex.Message}");
                return $"程序异常: {ex.Message}";
            }

        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 获取软件所有信息并转换为Mysoft对象
        /// </summary>
        /// <returns>返回Mysoft对象,如果获取失败返回null</returns>
        private async Task<Mysoft> GetSoftwareInfoAsync()
        {
            try
            {
                // 先获取JSON字符串
                string jsonResult = await ExecuteApiRequest(async (apiUrl) =>
                {
                    using HttpClient httpClient = new();
                    // 构建请求URL
                    string requestUrl = $"{apiUrl}/v3/obtainSoftware?softwareId={Constants.SOFTWARE_ID}&machineCode={Constants.LOCAL_MACHINE_CODE}&isAPI=y";

                    // 发送GET请求
                    HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    return await response.Content.ReadAsStringAsync();
                });

                if (string.IsNullOrEmpty(jsonResult))
                {
                    return new Mysoft();
                }

                // 反序列化外层JSON
                var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonResult);
                if (_JsonData == null)
                {
                    return new Mysoft();
                }

                // 解密数据
                string JsonData = _JsonData.data != null ? AesDecrypt(_JsonData.data, Constants.DEVELOPER_KEY) : string.Empty;
                if (string.IsNullOrEmpty(JsonData))
                {
                    return new Mysoft();
                }

                // 反序列化内层JSON
                var json2018K = JsonConvert.DeserializeObject<Json2018K>(JsonData);
                if (json2018K == null)
                {
                    return new Mysoft();
                }

                // 转换为Mysoft对象
                var result = ConvertToMysoftConfig(json2018K);
                return result ?? new Mysoft();
            }
            catch (Exception ex)
            {
                // 记录错误日志（如果需要）
                Log.Error($"获取软件信息失败: {ex.Message}");
                Console.WriteLine($"获取软件信息失败: {ex.Message}");
                return new Mysoft();
            }
        }
        /// <summary>
        /// 转换源数据到Mysoft配置
        /// </summary>
        private Mysoft ConvertToMysoftConfig(Json2018K source)
        {
            if (source == null)
            {
                return new Mysoft();
            }

            long _expriationDate;
            int _numberOfDays;
            if (source.isItEffective == "y" && string.IsNullOrEmpty(source.expirationDate))
            {
                _expriationDate = 7258089599000;
            }
            else
            {
                _expriationDate = long.TryParse(source.expirationDate, out long expiration) ? expiration : 0;
            }
            if (source.isItEffective == "y" && string.IsNullOrEmpty(source.numberOfDays))
            {
                _numberOfDays = 99999;
            }
            else
            {
                _numberOfDays = int.TryParse(source.numberOfDays, out int days) ? days : 0;
            }

            return new Mysoft
            {
                author = source.user ?? string.Empty,
                softwareMd5 = source.softwareMd5 ?? string.Empty,
                softwareName = source.softwareName ?? string.Empty,
                softwareId = source.softwareId ?? string.Empty,
                versionNumber = source.versionNumber ?? string.Empty,
                mandatoryUpdate = source.mandatoryUpdate?.ToLower() == "y",
                numberOfVisits = int.TryParse(source.numberOfVisits, out int visits) ? visits : 0,
                miniVersion = source.miniVersion ?? string.Empty,
                timeStamp = long.TryParse(source.timeStamp, out long timestamp) ? timestamp : 0,
                networkVerificationId = source.networkVerificationId ?? string.Empty,
                isItEffective = source.isItEffective?.ToLower() == "y",
                numberOfDays = _numberOfDays,
                networkVerificationRemarks = source.networkVerificationRemarks ?? string.Empty,
                expirationDate = _expriationDate,
                downloadLink = source.downloadLink ?? string.Empty,
                notice = source.notice ?? string.Empty,
                versionInformation = source.versionInformation ?? string.Empty,
                bilibiliLink = "https://space.bilibili.com/3493128132626725"
            };
        }

        /// <summary>
        /// 获取用户所有信息并转换为USER对象
        /// </summary>
        /// <returns>返回Mysoft对象,如果获取失败返回null</returns>
        private async Task<UserInfo> GetUserInfoAsync()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var userinfo = await ExecuteApiRequest(async (apiUrl) =>
            {
                var requestData = new
                {
                    softwareId = Constants.SOFTWARE_ID,
                    email = Constants.EMAIL,
                    password = Constants.PASSWORD,
                    timeStamp = timestamp

                };

                // 序列化为 JSON
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);

                // 使用 HttpClient 发送 POST 请求
                using (HttpClient client = new HttpClient())
                {
                    // 设置请求头（Content-Type）
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    // 构造 StringContent（请求体）
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(apiUrl + "/v3/customerLogin", content);
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<JsonUser>(jsonString);
                    if (_JsonData?.Data != null)
                    {
                        string result = JsonConvert.SerializeObject(_JsonData.Data, Formatting.Indented);
                        return result;
                    }
                    else
                    {
                        // 返回空对象的 JSON 字符串，避免返回 null
                        return JsonConvert.SerializeObject(new UserInfo(), Formatting.Indented);
                    }

                }
            });
            var UserJsonData = JsonConvert.DeserializeObject<UserInfo>(userinfo);
            // 保证返回非 null
            if(UserJsonData == null)
            {
                return new UserInfo();
            }
            return ConvertToUserConfig(UserJsonData);
        }
        /// <summary>
        /// 转换源数据到USER配置
        /// </summary>
        private UserInfo ConvertToUserConfig(UserInfo source)
        {
            if (source == null)
            {
                return new UserInfo();
            }
            return new UserInfo
            {
                CustomerId = source.CustomerId,
                AvatarUrl = source.AvatarUrl,
                Nickname = source.Nickname,
                Email = source.Email,
                Balance = source.Balance,
                License = source.License,
                TimeCrypt = source.TimeCrypt,
                Timestamp = source.Timestamp
            };
        }
        /// <summary>
        /// 检查网络连接是否可用
        /// </summary>
        /// <returns>如果网络可用返回true,否则返回false</returns>
        private static bool IsNetworkAvailable()
        {
            try
            {
                // 使用NetworkInterface检查网络连接状态
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch
            {
                // 如果检查过程中出现异常,保守返回false
                Log.Error("检查网络连接状态时出现异常");
                return false;
            }
        }

        // API健康状态类
        private class ApiHealthStatus
        {
            public bool IsHealthy { get; set; } = true;
            public DateTime LastChecked { get; set; } = DateTime.MinValue;
            public Exception? LastError { get; set; }
            public bool IsChecking { get; set; } // 防止重复检测
        }

        /// <summary>
        /// 初始化健康状态字典
        /// </summary>
        static Update()
        {
            foreach (var apiUrl in Constants.ApiAddressList)
            {
                apiHealthStatus[apiUrl] = new ApiHealthStatus();
            }

            // 启动后台健康检测任务
            StartBackgroundHealthCheck();
        }

        /// <summary>
        /// 启动后台健康检测任务
        /// </summary>
        private static void StartBackgroundHealthCheck()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await CheckAllApisHealthAsync();
                        // 每30秒检测一次
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                    catch
                    {
                        // 忽略后台检测任务的异常
                        await Task.Delay(TimeSpan.FromSeconds(60));
                    }
                }
            });
        }

        /// <summary>
        /// 异步检测所有API的健康状态
        /// </summary>
        private static async Task CheckAllApisHealthAsync()
        {
            var tasks = new List<Task>();

            foreach (var apiUrl in Constants.ApiAddressList)
            {
                tasks.Add(CheckApiHealthAsync(apiUrl));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 异步检测单个API的健康状态
        /// </summary>
        private static async Task CheckApiHealthAsync(string apiUrl)
        {
            var status = apiHealthStatus[apiUrl];

            // 如果正在检测中,跳过
            if (status.IsChecking)
                return;

            lock (lockObject)
            {
                if (status.IsChecking)
                    return;
                status.IsChecking = true;
            }

            try
            {
                bool isHealthy = await PerformHealthCheckAsync(apiUrl);

                lock (lockObject)
                {
                    status.IsHealthy = isHealthy;
                    status.LastChecked = DateTime.Now;
                    status.LastError = isHealthy ? null : new Exception("健康检测失败");
                    status.IsChecking = false;
                }
            }
            catch (Exception ex)
            {
                lock (lockObject)
                {
                    status.IsHealthy = false;
                    status.LastChecked = DateTime.Now;
                    status.LastError = ex;
                    status.IsChecking = false;
                }
            }
        }

        /// <summary>
        /// 执行实际的健康检测
        /// </summary>
        private static async Task<bool> PerformHealthCheckAsync(string apiUrl)
        {
            try
            {
                // 尝试访问API的健康检查端点或根路径
                var healthCheckUrls = new[]
                {
            $"{apiUrl}/health",
            $"{apiUrl}/api/health",
            $"{apiUrl}/"
        };

                foreach (var checkUrl in healthCheckUrls)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(healthCheckTimeout);
                        var response = await healthCheckClient.GetAsync(checkUrl, cts.Token);

                        // 如果返回2xx状态码,认为API健康
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // 尝试下一个URL
                        continue;
                    }
                }

                return false;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // 超时
                Log.Error("API健康检测超时");
                return false;
            }
            catch (HttpRequestException)
            {
                // 网络请求异常
                Log.Error("API健康检测网络请求异常");
                return false;
            }
            catch
            {
                // 其他异常
                Log.Error("API健康检测出现未知异常");
                return false;
            }
        }

        /// <summary>
        /// 获取当前可用的最佳API地址
        /// </summary>
        private static string GetBestAvailableApiUrl()
        {
            lock (lockObject)
            {
                // 首先检查网络是否可用
                if (!IsNetworkAvailable())
                {
                    return DefaultApiUrl;
                }

                // 首先检查当前地址是否健康
                if (IsApiHealthy(OpenApiUrl))
                {
                    return OpenApiUrl;
                }

                // 当前地址不健康,寻找下一个健康地址
                for (int i = 0; i < Constants.ApiAddressList.Length; i++)
                {
                    var index = (currentApiIndex + i + 1) % Constants.ApiAddressList.Length;
                    var apiUrl = Constants.ApiAddressList[index];

                    if (IsApiHealthy(apiUrl))
                    {
                        currentApiIndex = index;
                        OpenApiUrl = apiUrl;
                        return apiUrl;
                    }
                }

                // 所有备用地址都不健康,回退到默认地址
                OpenApiUrl = LocalApiUrl;
                return OpenApiUrl;
            }
        }

        /// <summary>
        /// 检查API地址是否健康（带缓存和实际检测）
        /// </summary>
        private static bool IsApiHealthy(string apiUrl)
        {
            // 默认地址总是被认为是健康的
            if (apiUrl == DefaultApiUrl)
            {
                return true;
            }

            var status = apiHealthStatus[apiUrl];

            // 如果缓存未过期,直接返回缓存状态
            if (DateTime.Now - status.LastChecked < healthCacheDuration)
            {
                return status.IsHealthy;
            }

            // 缓存过期,触发异步重新检测（不等待结果,使用上次的状态）
            // 检测会在后台进行,下次调用时会使用新的检测结果
            _ = Task.Run(() => CheckApiHealthAsync(apiUrl));

            return status.IsHealthy; // 返回当前状态,可能不是最新的
        }

        /// <summary>
        /// 标记API地址为不健康
        /// </summary>
        private static void MarkApiAsUnhealthy(string apiUrl, Exception error)
        {
            if (apiUrl == DefaultApiUrl) return;

            lock (lockObject)
            {
                var status = apiHealthStatus[apiUrl];
                status.IsHealthy = false;
                status.LastError = error;
                status.LastChecked = DateTime.Now;
            }

            // 触发异步重新检测
            _ = Task.Run(() => CheckApiHealthAsync(apiUrl));
        }

        /// <summary>
        /// 执行API请求,使用最佳可用地址
        /// </summary>
        private static async Task<string> ExecuteApiRequest(Func<string, Task<string>> requestFunc)
        {
            Exception lastException;
            string bestApiUrl = GetBestAvailableApiUrl();
            // 如果检测到使用本地地址,直接返回空字符串,避免返回 null
            if (bestApiUrl == LocalApiUrl)
            {
                // 直接返回,不执行请求
                return string.Empty;
            }

            try
            {
                // 使用最佳可用地址执行请求
                var result = await requestFunc(bestApiUrl);
                // 如果 result 可能为 null,则返回空字符串
                return result ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                if (IsNetworkAvailable())
                {
                    lastException = ex;
                    // 标记当前地址为不健康
                    MarkApiAsUnhealthy(bestApiUrl, ex);

                    // 尝试使用下一个可用地址重试一次
                    bestApiUrl = GetBestAvailableApiUrl();
                    if (bestApiUrl != OpenApiUrl) // 确保不是同一个地址
                    {
                        try
                        {
                            var retryResult = await requestFunc(bestApiUrl);
                            return retryResult ?? string.Empty;
                        }
                        catch (Exception retryEx)
                        {
                            lastException = retryEx;
                            MarkApiAsUnhealthy(bestApiUrl, retryEx);
                        }
                    }
                }
                // 所有远程地址都失败,返回空字符串
                return string.Empty;
            }
            catch (Exception ex)
            {
                lastException = ex;
                return string.Empty;
            }

        }


        private string AesDecryptData(string encryptedText, string secret)
        {
            // Base64解码
            byte[] cipherData = Convert.FromBase64String(encryptedText);
            if (cipherData.Length < 16)
            {
                Log.Error("无效的加密文本");
                throw new ArgumentException("Invalid encrypted text");
            }             

            // 提取salt（8字节,从索引8开始）
            byte[] saltData = new byte[8];
            Array.Copy(cipherData, 8, saltData, 0, 8);

            // 生成密钥和IV
            GenerateKeyAndIV(saltData, Encoding.Default.GetBytes(secret), out byte[] key, out byte[] iv);

            // 提取加密数据（从第16字节开始）
            byte[] data = new byte[cipherData.Length - 16];
            Array.Copy(cipherData, 16, data, 0, data.Length);

            // AES解密
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(data))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                {
                    // 将UTF8解码结果转换为系统默认编码
                    var utf8Result = sr.ReadToEnd();
                    byte[] ansiBytes = Encoding.Default.GetBytes(utf8Result);
                    var json = Encoding.Default.GetString(ansiBytes);
                    var parsedJson = JsonConvert.DeserializeObject(json);
                    return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                }
            }
        }

        // 辅助方法：将十六进制字符串转换为字节数组
        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                Log.Error("十六进制字符串长度必须是偶数");
                throw new ArgumentException("十六进制字符串长度必须是偶数");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private void GenerateKeyAndIV(byte[] saltData, byte[] password, out byte[] key, out byte[] iv)
        {
            StringBuilder str = new StringBuilder();
            string md5str = "";

            // 三次MD5迭代
            for (int i = 0; i < 3; i++)
            {
                // 组合前次MD5结果+密码+salt
                byte[] previousMd5 = HexStringToBytes(md5str);
                byte[] combined = CombineBytes(previousMd5, password, saltData);

                // 计算MD5
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(combined);
                    md5str = BytesToHexString(hash);
                    str.Append(md5str);
                }
            }

            // 生成最终字节数组
            byte[] resultBytes = HexStringToBytes(str.ToString());

            // 提取密钥和IV
            key = new byte[32];
            iv = new byte[16];
            Array.Copy(resultBytes, 0, key, 0, 32);
            Array.Copy(resultBytes, 32, iv, 0, 16);
        }

        // 辅助函数：十六进制字符串转字节数组
        private byte[] HexStringToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hexadecimal string must have even length");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        // 辅助函数：字节数组转十六进制字符串
        private string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        // 辅助函数：组合多个字节数组
        private byte[] CombineBytes(params byte[][] arrays)
        {
            int length = 0;
            foreach (byte[] array in arrays)
                length += array.Length;

            byte[] combined = new byte[length];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Array.Copy(array, 0, combined, offset, array.Length);
                offset += array.Length;
            }
            return combined;
        }
        #endregion
    }
}