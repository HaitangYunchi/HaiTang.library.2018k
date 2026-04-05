/*----------------------------------------------------------------
 * 版权所有 (c) 2025 HaiTangYunchi  保留所有权利
 * CLR版本：4.0.30319.42000
 * 公司名称：HaiTangYunchi
 * 命名空间： HaiTang.Library.Api2018k
 * 唯一标识：62a74e2f-ef1d-4b37-956d-2e572887051c
 * 文件名：Api2018k
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

using HaiTang.Library.Api2018k.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HaiTang.Library.Api2018k
{
    /// <summary>
    /// 提供与软件更新、用户管理、卡密验证、云变量操作等相关的 API 封装方法。
    /// 支持多 API 地址故障转移、健康检测、加密解密等功能。
    /// 此类是密封的，实现 IDisposable，敏感信息通过 Constants 安全委托访问。
    /// </summary>
    public sealed class Update : IDisposable
    {
        #region 常量定义

        private static readonly string _error = "<空>";
        private static readonly string _worring = "<错误>";
        private const string DefaultApiUrl = "http://api.2018k.cn";
        private static string OpenApiUrl = DefaultApiUrl;
        private static string LocalApiUrl = "127.0.0.1";

        private static int currentApiIndex = 0;
        private static readonly Dictionary<string, ApiHealthStatus> apiHealthStatus = new Dictionary<string, ApiHealthStatus>();
        private static readonly TimeSpan healthCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan healthCheckTimeout = TimeSpan.FromSeconds(5);
        private static readonly object lockObject = new object();

        // 全局单例 HttpClient
        private static readonly HttpClient _httpClient;
        private static readonly HttpClient healthCheckClient;
        private bool _disposed = false;

        #endregion

        #region 静态构造函数（初始化HttpClient和健康状态）

        static Update()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                MaxConnectionsPerServer = 10,
                EnableMultipleHttp2Connections = true
            };
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;

            healthCheckClient = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
            {
                Timeout = healthCheckTimeout
            };

            foreach (var apiUrl in Constants.ApiAddressList)
                apiHealthStatus[apiUrl] = new ApiHealthStatus();

            StartBackgroundHealthCheck();
        }

        #endregion

        #region 缓存相关

        private static Mysoft _cachedSoftwareInfo = new Mysoft();
        private static bool _isSoftwareSuccess = false;
        private static DateTime _lastCacheTime = DateTime.MinValue;
        private static readonly object _cacheLock = new object();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private static UserInfo _cachedUserInfo = new UserInfo();
        private static DateTime _userLastCacheTime = DateTime.MinValue;
        private static readonly object _UserCacheLock = new object();
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// 获取缓存的软件信息
        /// </summary>
        /// <returns>返回缓存的软件信息对象</returns>
        public static Mysoft GetCachedSoftwareInfo()
        {
            lock (_cacheLock) { return _cachedSoftwareInfo ?? new Mysoft(); }
        }

        /// <summary>
        /// 设置缓存的软件信息
        /// </summary>
        /// <param name="success">是否成功获取软件信息</param>
        /// <param name="softwareInfo">要缓存的软件信息对象</param>
        public static void SetCachedSoftwareInfo(bool success, Mysoft softwareInfo)
        {
            lock (_cacheLock)
            {
                _cachedSoftwareInfo = softwareInfo;
                _isSoftwareSuccess = success;
                _lastCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 检查软件信息缓存是否有效
        /// </summary>
        /// <returns>如果缓存有效返回true，否则返回false</returns>
        public static bool IsCacheValid()
        {
            lock (_cacheLock)
            {
                return _cachedSoftwareInfo != null && DateTime.Now - _lastCacheTime < CacheDuration;
            }
        }

        /// <summary>
        /// 清除软件信息缓存
        /// </summary>
        public static void ClearStaticCache()
        {
            lock (_cacheLock) { _cachedSoftwareInfo = new Mysoft(); _lastCacheTime = DateTime.MinValue; }
        }

        /// <summary>
        /// 获取缓存的用户信息
        /// </summary>
        /// <returns>返回缓存的用户信息对象</returns>
        public static UserInfo GetCachedUserInfo()
        {
            lock (_UserCacheLock) { return _cachedUserInfo ?? new UserInfo(); }
        }

        /// <summary>
        /// 设置缓存的用户信息
        /// </summary>
        /// <param name="userInfo">要缓存的用户信息对象</param>
        public static void SetCachedUserInfo(UserInfo userInfo)
        {
            lock (_UserCacheLock)
            {
                _cachedUserInfo = userInfo;
                _userLastCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 检查用户信息缓存是否有效
        /// </summary>
        /// <returns>如果缓存有效返回true，否则返回false</returns>
        public static bool IsUserCacheValid()
        {
            lock (_UserCacheLock)
            {
                return _cachedUserInfo != null && DateTime.Now - _userLastCacheTime < UserCacheDuration;
            }
        }

        /// <summary>
        /// 清除用户信息缓存
        /// </summary>
        public static void ClearUserCache()
        {
            lock (_UserCacheLock) { _cachedUserInfo = new UserInfo(); _userLastCacheTime = DateTime.MinValue; }
        }

        #endregion

        #region 软件实例方法

        /// <summary>
        /// 初始化并检测实例是否正常
        /// </summary>
        /// <param name="ID">软件ID，可选参数。如果提供，将通过安全方式设置到Constants中</param>
        /// <param name="key">开发者密钥，可选参数。如果提供，将通过安全方式设置到Constants中</param>
        /// <param name="Code">机器码，可选参数。如果不提供，将自动获取</param>
        /// <returns>返回一个元组，包含是否成功和软件配置信息</returns>
        public async Task<(bool Success, Mysoft? config)> InitializationAsync(string ID = null, string key = null, string? Code = null)
        {
            // 如果参数非空，则通过安全方式设置 Constants（仅当未通过 SecureString 设置时）
            if (!string.IsNullOrEmpty(ID))
            {
                var secureId = Tools.CreateSecureString(ID);
                Tools.SetSoftwareId(secureId);
            }
            if (!string.IsNullOrEmpty(key))
            {
                var secureKey = Tools.CreateSecureString(key);
                Tools.SetDeveloperKey(secureKey);
            }
            Constants.LOCAL_MACHINE_CODE = Code ?? Tools.GetMachineCodeEx();

            if (IsCacheValid())
                return (GetCachedSoftwareInfo().author != null, GetCachedSoftwareInfo());

            try
            {
                var softwareInfo = await GetSoftwareInfoAsync().ConfigureAwait(false);
                if (softwareInfo?.author != null)
                    SetCachedSoftwareInfo(true, softwareInfo);
                return (softwareInfo?.author != null, softwareInfo);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取软件信息时发生异常");
                return (false, null);
            }
        }

        /// <summary>
        /// 检查软件是否已可用
        /// </summary>
        /// <returns>成功返回true，否则返回false</returns>
        [Obsolete("方法已经合并到 InitializationAsync()，2027年01月01日正式禁用此方法", false)]
        public async Task<bool> GetSoftCheck()
        {
            var (success, _) = await InitializationAsync();
            return success;
        }

        /// <summary>
        /// 获取软件的全部配置信息
        /// </summary>
        /// <returns>返回软件配置信息对象</returns>
        public async Task<Mysoft> GetSoftAll()
        {
            var (_, softwareInfo) = await InitializationAsync();
            return softwareInfo ?? new Mysoft();
        }

        /// <summary>
        /// 获取软件ID
        /// </summary>
        /// <returns>返回软件ID字符串</returns>
        public async Task<string> GetSoftwareID()
        {
            var (_, info) = await InitializationAsync();
            return info?.softwareId ?? _error;
        }

        // <summary>
        /// 获取软件版本号
        /// </summary>
        /// <returns>返回版本号字符串</returns>
        public async Task<string> GetVersionNumber()
        {
            var (_, info) = await InitializationAsync();
            return info?.versionNumber ?? _error;
        }

        /// <summary>
        /// 获取软件名称
        /// </summary>
        /// <returns>返回软件名称字符串</returns>
        public async Task<string> GetSoftwareName()
        {
            var (_, info) = await InitializationAsync();
            return info?.softwareName ?? _error;
        }

        /// <summary>
        /// 获取版本信息
        /// </summary>
        /// <returns>返回版本信息字符串</returns>
        public async Task<string> GetVersionInformation()
        {
            var (_, info) = await InitializationAsync();
            return info?.versionInformation ?? _error;
        }

        /// <summary>
        /// 获取软件公告
        /// </summary>
        /// <returns>返回公告内容字符串</returns>
        public async Task<string> GetNotice()
        {
            var (_, info) = await InitializationAsync();
            return info?.notice ?? _error;
        }

        /// <summary>
        /// 获取软件下载链接
        /// </summary>
        /// <returns>返回下载链接字符串</returns>
        public async Task<string> GetDownloadLink()
        {
            var (_, info) = await InitializationAsync();
            return info?.downloadLink ?? _error;
        }

        /// <summary>
        /// 获取软件访问次数
        /// </summary>
        /// <returns>返回访问次数字符串</returns>
        public async Task<string> GetNumberOfVisits()
        {
            var (_, info) = await InitializationAsync();
            return info?.numberOfVisits.ToString() ?? _error;
        }

        /// <summary>
        /// 获取软件最低版本要求
        /// </summary>
        /// <returns>返回最低版本号字符串</returns>
        public async Task<string> GetMiniVersion()
        {
            var (_, info) = await InitializationAsync();
            return info?.miniVersion ?? _error;
        }

        /// <summary>
        /// 检查软件是否有效
        /// </summary>
        /// <returns>如果软件有效返回true，否则返回false</returns>
        public async Task<bool> GetIsItEffective()
        {
            var (_, info) = await InitializationAsync();
            return info?.isItEffective ?? false;
        }

        /// <summary>
        /// 获取软件过期日期
        /// </summary>
        /// <returns>返回过期日期字符串</returns>
        public async Task<string> GetExpirationDate()
        {
            var (_, info) = await InitializationAsync();
            return info?.expirationDate.ToString() ?? _error;
        }

        /// <summary>
        /// 获取软件备注信息
        /// </summary>
        /// <returns>返回备注信息字符串</returns>
        public async Task<string> GetRemarks()
        {
            var (_, info) = await InitializationAsync();
            return info?.networkVerificationRemarks ?? _error;
        }

        /// <summary>
        /// 获取软件剩余使用天数
        /// </summary>
        /// <returns>返回剩余天数</returns>
        public async Task<int> GetNumberOfDays()
        {
            var (_, info) = await InitializationAsync();
            return info?.numberOfDays ?? 0;
        }

        /// <summary>
        /// 获取网络验证ID
        /// </summary>
        /// <returns>返回网络验证ID字符串</returns>
        public async Task<string> GetNetworkVerificationId()
        {
            var (_, info) = await InitializationAsync();
            return info?.networkVerificationId ?? _error;
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns>返回时间戳数值</returns>
        public async Task<long> GetTimeStamp()
        {
            var (_, info) = await InitializationAsync();
            return info?.timeStamp ?? 0;
        }

        /// <summary>
        /// 检查是否需要强制更新
        /// </summary>
        /// <returns>如果需要强制更新返回true，否则返回false</returns>
        public async Task<bool> GetMandatoryUpdate()
        {
            var (_, info) = await InitializationAsync();
            return info?.mandatoryUpdate ?? false;
        }

        /// <summary>
        /// 获取软件MD5值
        /// </summary>
        /// <returns>返回MD5字符串</returns>
        public async Task<string> GetSoftwareMd5()
        {
            var (_, info) = await InitializationAsync();
            return info?.softwareMd5 ?? _error;
        }

        /// <summary>
        /// 获取指定名称的云变量值
        /// </summary>
        /// <param name="VarName">云变量名称</param>
        /// <returns>返回云变量值字符串</returns>
        public async Task<string> GetCloudVariables(string VarName)
        {
            var (JsonData, Success) = await GetCloudVariablesData();
            if (!Success) return _error;
            JArray jsonArray = JArray.Parse(JsonData);
            List<KeyValuePair<string, string>> configList = new List<KeyValuePair<string, string>>();
            foreach (JObject item in jsonArray)
            {
                string CloudKey = item["key"]?.ToString() ?? string.Empty;
                string CloudValue = item["value"]?.ToString() ?? string.Empty;
                configList.Add(new KeyValuePair<string, string>(CloudKey, CloudValue));
            }
            var _Var = configList.FirstOrDefault(p => p.Key == VarName);
            return _Var.Value;
        }

        /// <summary>
        /// 获取所有云变量
        /// </summary>
        /// <returns>返回JSON格式的云变量数据</returns>
        public async Task<string> GetCloudVarArray()
        {
            var (JsonData, Success) = await GetCloudVariablesData();
            if (!Success) return _error;
            JArray jsonArray = JArray.Parse(JsonData);
            JObject result = new JObject();
            foreach (JObject item in jsonArray)
            {
                string key = item["key"]?.ToString() ?? string.Empty;
                string value = item["value"]?.ToString() ?? string.Empty;
                result[key] = value;
            }
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                StringEscapeHandling = StringEscapeHandling.Default,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(result, settings);
        }

        private async Task<(string JsonData, bool Success)> GetCloudVariablesData()
        {
            bool success = await GetSoftCheck();
            if (!success) return (string.Empty, false);
            string jsonData = string.Empty;
            bool result = false;
            await ExecuteApiRequest(async (apiUrl) =>
            {
                string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                string requestUrl = $"{apiUrl}/v3/getCloudVariables?softwareId={softwareId}&isAPI=y";
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                string jsonString = await response.Content.ReadAsStringAsync();
                var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);
                string key = Tools.ExecuteWithDeveloperKey(k => k);
                jsonData = _JsonData?.data != null ? AesDecryptData(_JsonData.data, key) : string.Empty;
                result = true;
                return string.Empty;
            });
            return (jsonData, result);
        }

        /// <summary>
        /// 更新云变量值
        /// </summary>
        /// <param name="VarKey">云变量名称</param>
        /// <param name="Value">新的云变量值</param>
        /// <returns>返回操作结果，包含成功状态和消息</returns>
        public async Task<(bool success, string message)> updateCloudVariables(string VarKey, string Value)
        {
            try
            {
                bool _response = await ExecuteApiRequest(async (apiUrl) =>
                {
                    var data = new JObject { ["key"] = VarKey, ["value"] = Value };
                    string key = Tools.ExecuteWithDeveloperKey(k => k);
                    string encryptedData = AesEncrypt(data, key);
                    string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                    string requestUrl = $"{apiUrl}/v3/updateCloudVariables?info={Uri.EscapeDataString(encryptedData)}&softwareId={softwareId}&isAPI=y";
                    var response = await _httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(responseContent);
                    return (_JsonData?.success ?? false) ? "true" : "false";
                }) == "true";
                return (_response, _response ? $"{{\"key\":\"{VarKey}\",\"value\":\"{Value}\"}}" : "失败");
            }
            catch
            {
                Log.Error("修改云变量失败,网络异常或程序错误");
                return (false, "失败,网络异常或程序错误");
            }
        }

        /// <summary>
        /// 激活软件
        /// </summary>
        /// <param name="authId">授权码ID</param>
        /// <returns>返回操作结果，包含成功状态和消息</returns>
        public async Task<(bool success, string message)> ActivationKey(string authId)
        {
            try
            {
                bool _response = await ExecuteApiRequest(async (apiUrl) =>
                {
                    string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                    string url = $"{apiUrl}/v3/activation?authId={authId}&softwareId={softwareId}&machineCode={Constants.LOCAL_MACHINE_CODE}&isAPI=y";
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var _JsonData = JsonConvert.DeserializeObject<Json2018K>(responseContent);
                    return (_JsonData?.success ?? false) ? "true" : "false";
                }) == "true";
                return (_response, _response ? "授权成功" : "授权失败,请检查授权码是否正确或已被使用");
            }
            catch
            {
                Log.Error("授权失败,网络异常或程序错误");
                return (false, "失败,网络异常或程序错误");
            }
        }

        /// <summary>
        /// 发送消息到服务器
        /// </summary>
        /// <param name="message">要发送的消息内容</param>
        /// <returns>返回服务器响应内容</returns>
        public async Task<string> MessageSend(string message)
        {
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                message = Uri.EscapeDataString(message);
                string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                string url = $"{apiUrl}/v3/messageSend?softwareId={softwareId}&message={message}&isAPI=y";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var jsonObject = JsonConvert.DeserializeObject(responseContent);
                    return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                }
                catch { return responseContent; }
            });
        }

        /// <summary>
        /// 创建网络验证
        /// </summary>
        /// <param name="day">有效天数</param>
        /// <param name="remark">备注信息</param>
        /// <param name="ID">软件ID</param>
        /// <returns>返回服务器响应内容</returns>
        public async Task<string> CreateNetworkAuthentication(int day, string remark, string ID)
        {
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                var data = new { day, remark, times = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds };
                string key = Tools.ExecuteWithDeveloperKey(k => k);
                string encodedCiphertext = AesEncrypt(data, key);
                string url = $"{apiUrl}/v3/createNetworkAuthentication?info={Uri.EscapeDataString(encodedCiphertext)}&softwareId={ID}&isAPI=y";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var jsonObject = JsonConvert.DeserializeObject(responseContent);
                    return JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                }
                catch { return responseContent; }
            });
        }

        /// <summary>
        /// 替换或解除绑定
        /// </summary>
        /// <param name="AuthId">授权码ID</param>
        /// <param name="Code">机器码，可选参数</param>
        /// <returns>返回操作结果，包含成功状态和消息</returns>
        public async Task<(bool success, string message)> ReplaceBind(string AuthId, string? Code = null)
        {
            try
            {
                bool _response = await ExecuteApiRequest(async (apiUrl) =>
                {
                    var data = new { authId = AuthId, machineCode = Code };
                    string key = Tools.ExecuteWithDeveloperKey(k => k);
                    string encodedCiphertext = AesEncrypt(data, key);
                    string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                    string url = $"{apiUrl}/v3/replaceBind?softwareId={softwareId}&info={Uri.EscapeDataString(encodedCiphertext)}&isAPI=y";
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
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
        /// <returns>返回剩余使用时间（毫秒），-1表示永久有效，0表示已过期，1表示未激活</returns>
        public async Task<long> GetRemainingUsageTime()
        {
            var (_, softwareInfo) = await InitializationAsync();
            if (softwareInfo == null) return 1;
            try
            {
                if (softwareInfo.isItEffective && softwareInfo.expirationDate == 7258089599000)
                    return -1;
                else if (softwareInfo.isItEffective && softwareInfo.expirationDate > 0)
                {
                    long currentTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    long timestamp = softwareInfo.expirationDate - currentTimestamp;
                    return timestamp > 0 ? timestamp : 0;
                }
                else return 1;
            }
            catch { return 0; }
        }

        /// <summary>
        /// 获取网络验证码
        /// </summary>
        /// <returns>返回网络验证码字符串</returns>
        public async Task<string> GetNetworkCode()
        {
            bool Success = await GetSoftCheck();
            if (!Success) return _error;
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                var requestData = new { softwareId = softwareId };
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl + "/v3/captcha", content);
                string jsonString = await response.Content.ReadAsStringAsync();
                var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);
                string key = Tools.ExecuteWithDeveloperKey(k => k);
                return _JsonData?.data != null ? AesDecryptData(_JsonData.data, key) : string.Empty;
            });
        }

        #endregion

        #region 用户方法

        /// <summary>
        /// 注册新用户
        /// </summary>
        /// <param name="email">用户邮箱</param>
        /// <param name="password">用户密码</param>
        /// <param name="nickName">用户昵称，可选参数</param>
        /// <param name="avatarUrl">用户头像URL，可选参数</param>
        /// <param name="captcha">验证码，可选参数</param>
        /// <returns>返回是否注册成功</returns>
        public async Task<bool> CustomerRegister(string email, string password, string? nickName = null, string? avatarUrl = null, string? captcha = null)
        {
            string _data = await ExecuteApiRequest(async (apiUrl) =>
            {
                string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                var requestData = new
                {
                    softwareId = softwareId,
                    email,
                    password,
                    avatarUrl,
                    nickName,
                    captcha
                };
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl + "/v3/customerRegister", content);
                string jsonString = await response.Content.ReadAsStringAsync();
                var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonString);
                return (_JsonData != null ? _JsonData.success.ToString() : "false");
            });
            return bool.TryParse(_data, out var result) && result;
        }

        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="ID">软件ID，可选参数</param>
        /// <param name="key">开发者密钥，可选参数</param>
        /// <param name="email">用户邮箱</param>
        /// <param name="password">用户密码</param>
        /// <returns>返回用户信息对象</returns>
        public async Task<UserInfo> InitializationUserAsync(string email, string password,string ID = null, string key = null)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                var secureId = Tools.CreateSecureString(ID);
                Tools.SetSoftwareId(secureId);
            }             
            if (!string.IsNullOrEmpty(key))
            {
                var secureKey = Tools.CreateSecureString(key);
                Tools.SetDeveloperKey(secureKey);
            }
            Constants.EMAIL = email;
            Constants.PASSWORD = password;
           
            Constants.LOCAL_MACHINE_CODE = Tools.GetMachineCodeEx();

            if (IsUserCacheValid()) return GetCachedUserInfo();
            var userInfo = await GetUserInfoAsync();
            if (userInfo.Email != string.Empty) SetCachedUserInfo(userInfo);
            return userInfo ?? new UserInfo();
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns>返回用户信息对象</returns>
        public async Task<UserInfo> GetUserInfo()
        {
            var userInfo = await InitializationUserAsync(Constants.EMAIL, Constants.PASSWORD);
            return userInfo.Email == string.Empty ? new UserInfo() : userInfo;
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <returns>返回用户ID字符串</returns>
        public async Task<string> GetUserId()
        {
            var userInfo = await GetUserInfo();
            return userInfo.CustomerId ?? _worring;
        }

        /// <summary>
        /// 获取用户头像URL
        /// </summary>
        /// <returns>返回头像URL字符串</returns>
        public async Task<string> GetUserAvatar()
        {
            var userInfo = await GetUserInfo();
            return userInfo.AvatarUrl ?? _worring;
        }

        /// <summary>
        /// 获取用户昵称
        /// </summary>
        /// <returns>返回用户昵称字符串</returns>
        public async Task<string> GetUserNickname()
        {
            var userInfo = await GetUserInfo();
            return userInfo.Nickname ?? _worring;
        }

        /// <summary>
        /// 获取用户邮箱
        /// </summary>
        /// <returns>返回用户邮箱字符串</returns>
        public async Task<string> GetUserEmail()
        {
            var userInfo = await GetUserInfo();
            return userInfo.Email ?? _worring;
        }

        /// <summary>
        /// 获取用户余额
        /// </summary>
        /// <returns>返回用户余额数值</returns>
        public async Task<int> GetUserBalance()
        {
            var userInfo = await GetUserInfo();
            return userInfo?.Balance ?? 0;
        }

        /// <summary>
        /// 获取用户许可证状态
        /// </summary>
        /// <returns>如果用户有许可证返回true，否则返回false</returns>
        public async Task<bool> GetUserLicense()
        {
            var userInfo = await GetUserInfo();
            return userInfo.License == "y";
        }

        /// <summary>
        /// 获取用户登录时间戳
        /// </summary>
        /// <returns>返回时间戳</returns>
        public async Task<string> GetUserTimeCrypt()
        {
            var userInfo = await GetUserInfo();
            return userInfo.TimeCrypt ?? _worring;
        }

        /// <summary>
        /// 用户充值
        /// </summary>
        /// <param name="AuthId">授权码ID</param>
        /// <returns>返回服务器响应内容</returns>
        public async Task<string> Recharge(string AuthId)
        {
            var _customerId = await GetUserId();
            return await ExecuteApiRequest(async (apiUrl) =>
            {
                var requestData = new { customerId = _customerId.ToString(), authId = AuthId };
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl + "/v3/customerRecharge", content);
                return await response.Content.ReadAsStringAsync();
            });
        }

        #endregion

        #region 加密解密

        /// <summary>
        /// 使用AES算法加密数据
        /// </summary>
        /// <param name="data">要加密的数据对象</param>
        /// <param name="key">加密密钥（32字节的十六进制字符串）</param>
        /// <returns>返回Base64编码的加密字符串</returns>
        public string AesEncrypt(object data, string key)
        {
            string plaintext = JsonConvert.SerializeObject(data);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = HexStringToByteArray(key);
                aesAlg.IV = new byte[16];
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plaintext);
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// 使用AES算法解密数据
        /// </summary>
        /// <param name="encryptedData">Base64编码的加密字符串</param>
        /// <param name="key">解密密钥（32字节的十六进制字符串）</param>
        /// <returns>返回解密后的明文字符串</returns>
        public string AesDecrypt(string encryptedData, string key)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(encryptedData);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = HexStringToByteArray(key);
                    aesAlg.IV = new byte[16];
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        return srDecrypt.ReadToEnd();
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
        /// 异步获取软件信息
        /// </summary>
        /// <returns>返回软件信息对象</returns>
        private async Task<Mysoft> GetSoftwareInfoAsync()
        {
            return await Tools.ExecuteWithBoth(async (softwareId, developerKey) =>
            {
                string jsonResult = await ExecuteApiRequest(async (apiUrl) =>
                {
                    string requestUrl = $"{apiUrl}/v3/obtainSoftware?softwareId={softwareId}&machineCode={Constants.LOCAL_MACHINE_CODE}&isAPI=y";
                    var response = await _httpClient.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                });
                if (string.IsNullOrEmpty(jsonResult)) return new Mysoft();
                var _JsonData = JsonConvert.DeserializeObject<Json2018K>(jsonResult);
                if (_JsonData?.user == null) return new Mysoft();
                string JsonData = _JsonData.data != null ? AesDecrypt(_JsonData.data, developerKey) : string.Empty;
                if (string.IsNullOrEmpty(JsonData)) return new Mysoft();
                var json2018K = JsonConvert.DeserializeObject<Json2018K>(JsonData);
                return json2018K == null ? new Mysoft() : ConvertToMysoftConfig(json2018K);
            });
        }

        /// <summary>
        /// 将Json2018K对象转换为Mysoft配置对象
        /// </summary>
        /// <param name="source">源Json2018K对象</param>
        /// <returns>转换后的Mysoft配置对象</returns>
        private Mysoft ConvertToMysoftConfig(Json2018K source)
        {
            if (source == null) return new Mysoft();
            long _expriationDate;
            int _numberOfDays;
            if (source.isItEffective == "y" && string.IsNullOrEmpty(source.expirationDate))
                _expriationDate = 7258089599000;
            else
                _expriationDate = long.TryParse(source.expirationDate, out long expiration) ? expiration : 0;
            if (source.isItEffective == "y" && string.IsNullOrEmpty(source.numberOfDays))
                _numberOfDays = 99999;
            else
                _numberOfDays = int.TryParse(source.numberOfDays, out int days) ? days : 0;

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
        /// 异步获取用户信息
        /// </summary>
        /// <returns>返回用户信息对象</returns>
        private async Task<UserInfo> GetUserInfoAsync()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var userinfo = await ExecuteApiRequest(async (apiUrl) =>
            {
                string softwareId = Tools.ExecuteWithSoftwareId(id => id);
                var requestData = new
                {
                    softwareId = softwareId,
                    email = Constants.EMAIL,
                    password = Constants.PASSWORD,
                    timeStamp = timestamp
                };
                string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl + "/v3/customerLogin", content);
                string jsonString = await response.Content.ReadAsStringAsync();
                var _JsonData = JsonConvert.DeserializeObject<JsonUser>(jsonString);
                if (_JsonData?.Data != null)
                    return JsonConvert.SerializeObject(_JsonData.Data, Formatting.Indented);
                else
                    return JsonConvert.SerializeObject(new UserInfo(), Formatting.Indented);
            });
            var UserJsonData = JsonConvert.DeserializeObject<UserInfo>(userinfo);
            return UserJsonData == null ? new UserInfo() : ConvertToUserConfig(UserJsonData);
        }

        /// <summary>
        /// 将UserInfo对象转换为标准用户信息对象
        /// </summary>
        /// <param name="source">源UserInfo对象</param>
        /// <returns>转换后的标准用户信息对象</returns>
        private UserInfo ConvertToUserConfig(UserInfo source)
        {
            if (source == null) return new UserInfo();
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
        /// <returns>如果网络可用返回true，否则返回false</returns>
        private static bool IsNetworkAvailable()
        {
            try { return NetworkInterface.GetIsNetworkAvailable(); }
            catch { Log.Error("检查网络连接状态时出现异常"); return false; }
        }

        /// <summary>
        /// API健康状态类
        /// </summary>
        private class ApiHealthStatus
        {
            public bool IsHealthy { get; set; } = true;
            public DateTime LastChecked { get; set; } = DateTime.MinValue;
            public Exception? LastError { get; set; }
            public bool IsChecking { get; set; }
        }

        /// <summary>
        /// 启动后台健康检查任务
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
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                    catch { await Task.Delay(TimeSpan.FromSeconds(60)); }
                }
            });
        }

        /// <summary>
        /// 检查所有API的健康状态
        /// </summary>
        /// <returns>异步任务</returns>
        private static async Task CheckAllApisHealthAsync()
        {
            var tasks = Constants.ApiAddressList.Select(CheckApiHealthAsync).ToArray();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 检查指定API的健康状态
        /// </summary>
        /// <param name="apiUrl">要检查的API地址</param>
        /// <returns>异步任务</returns>
        private static async Task CheckApiHealthAsync(string apiUrl)
        {
            var status = apiHealthStatus[apiUrl];
            if (status.IsChecking) return;
            lock (lockObject)
            {
                if (status.IsChecking) return;
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
        /// 执行API健康检查
        /// </summary>
        /// <param name="apiUrl">要检查的API地址</param>
        /// <returns>如果API健康返回true，否则返回false</returns>
        private static async Task<bool> PerformHealthCheckAsync(string apiUrl)
        {
            try
            {
                var healthCheckUrls = new[] { $"{apiUrl}/health", $"{apiUrl}/api/health", $"{apiUrl}/" };
                foreach (var checkUrl in healthCheckUrls)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(healthCheckTimeout);
                        var response = await healthCheckClient.GetAsync(checkUrl, cts.Token);
                        if (response.IsSuccessStatusCode) return true;
                    }
                    catch { continue; }
                }
                return false;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                Log.Error("API健康检测超时");
                return false;
            }
            catch (HttpRequestException)
            {
                Log.Error("API健康检测网络请求异常");
                return false;
            }
            catch
            {
                Log.Error("API健康检测出现未知异常");
                return false;
            }
        }

        /// <summary>
        /// 获取最佳可用的API地址
        /// </summary>
        /// <returns>返回可用的API地址</returns>
        private static string GetBestAvailableApiUrl()
        {
            lock (lockObject)
            {
                if (!IsNetworkAvailable()) return DefaultApiUrl;
                if (IsApiHealthy(OpenApiUrl)) return OpenApiUrl;
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
                OpenApiUrl = LocalApiUrl;
                return OpenApiUrl;
            }
        }

        /// <summary>
        /// 检查API是否健康
        /// </summary>
        /// <param name="apiUrl">要检查的API地址</param>
        /// <returns>如果API健康返回true，否则返回false</returns>
        private static bool IsApiHealthy(string apiUrl)
        {
            if (apiUrl == DefaultApiUrl) return true;
            var status = apiHealthStatus[apiUrl];
            if (DateTime.Now - status.LastChecked < healthCacheDuration)
                return status.IsHealthy;
            _ = Task.Run(() => CheckApiHealthAsync(apiUrl));
            return status.IsHealthy;
        }

        /// <summary>
        /// 标记API为不健康状态
        /// </summary>
        /// <param name="apiUrl">API地址</param>
        /// <param name="error">错误信息</param>
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
            _ = Task.Run(() => CheckApiHealthAsync(apiUrl));
        }

        /// <summary>
        /// 执行API请求
        /// </summary>
        /// <param name="requestFunc">请求函数</param>
        /// <returns>返回API响应内容</returns>
        private static async Task<string> ExecuteApiRequest(Func<string, Task<string>> requestFunc)
        {
            string bestApiUrl = GetBestAvailableApiUrl();
            if (bestApiUrl == LocalApiUrl) return string.Empty;
            try
            {
                var result = await requestFunc(bestApiUrl);
                return result ?? string.Empty;
            }
            catch (HttpRequestException ex) when (IsNetworkAvailable())
            {
                MarkApiAsUnhealthy(bestApiUrl, ex);
                bestApiUrl = GetBestAvailableApiUrl();
                if (bestApiUrl != OpenApiUrl)
                {
                    try
                    {
                        var retryResult = await requestFunc(bestApiUrl);
                        return retryResult ?? string.Empty;
                    }
                    catch (Exception retryEx)
                    {
                        MarkApiAsUnhealthy(bestApiUrl, retryEx);
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="encryptedText">加密文本</param>
        /// <param name="secret">解密密钥</param>
        /// <returns>返回解密后的文本</returns>
        private string AesDecryptData(string encryptedText, string secret)
        {
            byte[] cipherData = Convert.FromBase64String(encryptedText);
            if (cipherData.Length < 16)
            {
                Log.Error("无效的加密文本");
                throw new ArgumentException("Invalid encrypted text");
            }
            byte[] saltData = new byte[8];
            Array.Copy(cipherData, 8, saltData, 0, 8);
            GenerateKeyAndIV(saltData, Encoding.Default.GetBytes(secret), out byte[] key, out byte[] iv);
            byte[] data = new byte[cipherData.Length - 16];
            Array.Copy(cipherData, 16, data, 0, data.Length);
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
                    var utf8Result = sr.ReadToEnd();
                    byte[] ansiBytes = Encoding.Default.GetBytes(utf8Result);
                    var json = Encoding.Default.GetString(ansiBytes);
                    var parsedJson = JsonConvert.DeserializeObject(json);
                    return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                }
            }
        }

        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>返回转换后的字节数组</returns>
        private static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException("十六进制字符串长度必须是偶数");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// 生成加密密钥和初始化向量
        /// </summary>
        /// <param name="saltData">盐值数据</param>
        /// <param name="password">密码数据</param>
        /// <param name="key">输出的密钥</param>
        /// <param name="iv">输出的初始化向量</param>
        private void GenerateKeyAndIV(byte[] saltData, byte[] password, out byte[] key, out byte[] iv)
        {
            StringBuilder str = new StringBuilder();
            string md5str = "";
            for (int i = 0; i < 3; i++)
            {
                byte[] previousMd5 = HexStringToBytes(md5str);
                byte[] combined = CombineBytes(previousMd5, password, saltData);
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(combined);
                    md5str = BytesToHexString(hash);
                    str.Append(md5str);
                }
            }
            byte[] resultBytes = HexStringToBytes(str.ToString());
            key = new byte[32];
            iv = new byte[16];
            Array.Copy(resultBytes, 0, key, 0, 32);
            Array.Copy(resultBytes, 32, iv, 0, 16);
        }

        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>返回转换后的字节数组</returns>
        private byte[] HexStringToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return new byte[0];
            if (hex.Length % 2 != 0) throw new ArgumentException("Hexadecimal string must have even length");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// 将字节数组转换为十六进制字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>返回转换后的十六进制字符串</returns>
        private string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 合并多个字节数组
        /// </summary>
        /// <param name="arrays">要合并的字节数组</param>
        /// <returns>返回合并后的字节数组</returns>
        private byte[] CombineBytes(params byte[][] arrays)
        {
            int length = arrays.Sum(arr => arr.Length);
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

        #region IDisposable 实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // 静态 HttpClient 不应在此释放，因为会被被其他实例使用。
                // 实际应在应用程序关闭时统一释放。这里仅实现接口，不释放静态资源。
                _disposed = true;
            }
        }

        #endregion
    }
}