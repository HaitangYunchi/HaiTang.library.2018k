using Newtonsoft.Json;
using System.Text;

namespace HaiTang.Library.Api2018k.SDK
{
 
    /// <summary>
    /// 2018K SDK 主类
    /// </summary>
    public class AdminSDK
    {
        private readonly HttpClient _httpClient;

        // 存储token的私有字段
        private string _token;

        // 公开的认证状态属性
        private bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        /// <summary>
        /// 获取或设置访问令牌
        /// </summary>
        public string Token
        {
            get => _token;
            private set => _token = value;
        }

        /// <summary>
        /// 构造函数（初始化HttpClient）
        /// </summary>
        public AdminSDK()
        {
            _httpClient = new()
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _token = string.Empty;  // 初始化 _token
        }

        /// <summary>
        /// 登录接口（返回布尔值和信息）
        /// </summary>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <returns>布尔值</returns>
        public async Task<(bool Success, string Message)> LoginAsync(string account, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    user = account,
                    password = password
                };

                string jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{Constants.BaseUrl}login", content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(responseContent);

                Token = result?.data?.token?.ToString() ?? string.Empty;
                return (IsAuthenticated, Token);
            }
            catch (Exception ex)
            {
                Token = string.Empty;
                return (false, $"登录失败: {ex.Message}");
            }
        }


        /// <summary>
        /// 获取软件列表ID（返回软件ID数组）
        /// </summary>
        /// <param name="maxCount">最大数量</param>
        /// <returns>软件ID数组</returns>
        /// <exception cref="Exception">请求异常</exception>
        public async Task<List<string>> GetSoftwareIdsAsync(long maxCount = 10)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("请先登录");
            }
            try
            {
                var request = new GetSoftwareListRequest
                {
                    page = new PageInfo
                    {
                        limit = (int)maxCount
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 添加Token头
                _httpClient.DefaultRequestHeaders.Remove("token");
                _httpClient.DefaultRequestHeaders.Add("token", Token);

                HttpResponseMessage response = await _httpClient.PostAsync($"{Constants.BaseUrl}softwareList", content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(responseContent);

                List<string> softwareIds = new List<string>();
                if (result?.data?.list != null)
                {
                    foreach (var item in result.data.list)
                    {
                        softwareIds.Add(item.softwareId?.ToString() ?? string.Empty);
                    }
                }

                return softwareIds;
            }
            catch (Exception ex)
            {
                throw new Exception("获取软件列表失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 根据软件ID获取软件信息
        /// </summary>
        /// <param name="softwareId">软件ID</param>
        /// <param name="maxCount">最大查询数量，默认10</param>
        /// <returns>软件信息实体</returns>
        /// <exception cref="Exception">请求异常</exception>
        public async Task<SoftwareInfo> GetSoftwareInfoByIdAsync(string softwareId, long maxCount = 10)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("请先登录");
            }
            try
            {
                var request = new GetSoftwareListRequest
                {
                    page = new PageInfo
                    {
                        limit = (int)maxCount
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 添加Token头
                _httpClient.DefaultRequestHeaders.Remove("token");
                _httpClient.DefaultRequestHeaders.Add("token", Token);

                HttpResponseMessage response = await _httpClient.PostAsync($"{Constants.BaseUrl}softwareList", content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(responseContent);

                SoftwareInfo softwareInfo = new SoftwareInfo();
                if (result?.data?.list != null)
                {
                    foreach (var item in result.data.list)
                    {
                        if (item.softwareId?.ToString() == softwareId)
                        {
                            softwareInfo.SoftwareName = item.name?.ToString() ?? string.Empty;
                            softwareInfo.VisitCount = item.visit?.ToString() ?? string.Empty;
                            softwareInfo.UpdateContent = item.remark?.ToString() ?? string.Empty;
                            softwareInfo.SoftwareVersion = item.version?.ToString() ?? string.Empty;
                            softwareInfo.Md5 = item.md5?.ToString() ?? string.Empty;
                            softwareInfo.CreateTime = item.createTime?.ToString() ?? string.Empty;
                            softwareInfo.LowVersion = item.lowVersion?.ToString() ?? string.Empty;
                            softwareInfo.ForceUpdate = item.force ?? false;
                            softwareInfo.DownloadUrl = item.url?.ToString() ?? string.Empty;
                            softwareInfo.Id = item._id?.ToString() ?? string.Empty;
                            break;
                        }
                    }
                }

                return softwareInfo;
            }
            catch (Exception ex)
            {
                throw new Exception("获取软件信息失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 获取卡密列表
        /// </summary>
        /// <param name="maxCount">最大数量</param>
        /// <param name="softwareId">实例ID，默认为空，表示获取所有软件的卡密</param>
        /// <returns>卡密信息列表</returns>
        /// <exception cref="Exception">请求异常</exception>
        public async Task<List<CardInfo>> GetCardListAsync( string? softwareId = null, long maxCount = 50)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("请先登录");
            }
            try
            {
                var request = new GetCardListRequest
                {
                    page = new PageInfo
                    {
                        limit = (int)maxCount
                    },
                    softwareId = softwareId ?? string.Empty // 保证不会为 null
                };

                string jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 添加Token头
                _httpClient.DefaultRequestHeaders.Remove("token");
                _httpClient.DefaultRequestHeaders.Add("token", Token);

                HttpResponseMessage response = await _httpClient.PostAsync($"{Constants.BaseUrl}authList", content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(responseContent);

                List<CardInfo> cardList = [];
                if (result?.data?.list != null)
                {
                    foreach (var item in result.data.list)
                    {
                        if (softwareId == null || softwareId == string.Empty)
                        {
                            CardInfo card = new()
                            {
                                CardNumber = item.authId?.ToString() ?? string.Empty,
                                Remarks = item.remark?.ToString() ?? string.Empty,
                                Duration = item.day?.ToString() ?? string.Empty,
                                IsActivated = item.status ?? false,
                                MachineCode = item.macid?.ToString() ?? string.Empty,
                                SoftwareId = item.softwareId?.ToString() ?? string.Empty

                            };
                            cardList.Add(card);
                        }
                        else if (item.softwareId?.ToString() == softwareId)
                        {
                            CardInfo card = new()
                            {
                                CardNumber = item.authId?.ToString() ?? string.Empty,
                                Remarks = item.remark?.ToString() ?? string.Empty,
                                Duration = item.day?.ToString() ?? string.Empty,
                                IsActivated = item.status ?? false,
                                MachineCode = item.macid?.ToString() ?? string.Empty,
                                SoftwareId = item.softwareId?.ToString() ?? string.Empty
                            };
                            cardList.Add(card);
                        }

                    }
                }

                return cardList;
            }
            catch (Exception ex)
            {
                throw new Exception("获取卡密列表失败：" + ex.Message, ex);
            }
        }


        /// <summary>
        /// 创建卡密
        /// </summary>
        /// <param name="softwareId">软件ID</param>
        /// <param name="remarks">备注</param>
        /// <param name="duration">时长（0为永久，单位：天）</param>
        /// <param name="count">创建数量</param>
        /// <returns>返回一个元组，包含操作是否成功和创建的卡密列表</returns>
        /// <exception cref="Exception">请求异常</exception>
        public async Task<(bool Success, List<string> CreatedCards)> CreateCardAsync(string softwareId, string remarks, int duration, int count)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("请先登录");
            }
            List<string> createdCards = new List<string>();
            try
            {
                var request = new CreateCardRequest
                {
                    softwareId = softwareId,
                    day = duration,
                    createNumber = count,
                    remark = remarks
                };

                string jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 添加Token头
                _httpClient.DefaultRequestHeaders.Remove("token");
                _httpClient.DefaultRequestHeaders.Add("token", Token);

                HttpResponseMessage response = await _httpClient.PostAsync($"{Constants.BaseUrl}createAuth", content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(responseContent);

                // 提取创建的卡密
                if (result?.data != null)
                {
                    foreach (var card in result.data)
                    {
                        createdCards.Add(card?.ToString() ?? string.Empty);
                    }
                }

                return (result?.message == "请求成功", createdCards);
            }
            catch (Exception ex)
            {
                throw new Exception("创建卡密失败：" + ex.Message, ex);
            }
        }


        /// <summary>
        /// 创建软件
        /// </summary>
        /// <param name="name">软件名称</param>
        /// <param name="version">软件版本</param>
        /// <returns>是否创建成功</returns>
        /// <exception cref="Exception">请求异常</exception>
        public async Task<bool> CreateSoftwareAsync(string name, string version)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("请先登录");
            }
            try
            {
                var request = new CreateSoftwareRequest
                {
                    Name = name,
                    Version = version
                };

                string jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 添加Token头
                _httpClient.DefaultRequestHeaders.Remove("token");
                _httpClient.DefaultRequestHeaders.Add("token", Token);

                HttpResponseMessage response = await _httpClient.PostAsync($"{Constants.BaseUrl}createSoftware", content);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(responseContent);

                return result?.message == "请求成功";
            }
            catch (Exception ex)
            {
                throw new Exception("创建软件失败：" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 编辑软件信息
        /// </summary>
        /// <param name="id">软件ID</param>
        /// <param name="updatedInfo">修改后的软件信息（Id字段无需填写）</param>
        /// <returns>是否编辑成功</returns>
        public async Task<bool> EditSoftwareInfoAsync(string id, SoftwareInfo updatedInfo)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("请先登录");
            }
            // 需根据实际接口补充实现
            // 此处仅为预留接口，需根据后端实际的编辑接口参数调整
            throw new NotImplementedException("编辑软件信息接口尚未实现，请根据实际API补充");
        }
    }

    /// <summary>
    /// 软件信息实体类
    /// </summary>
    public class SoftwareInfo
    {
        /// <summary>
        /// 软件名称
        /// </summary>
        public string SoftwareName { get; set; } = string.Empty;

        /// <summary>
        /// 软件版本号
        /// </summary>
        public string SoftwareVersion { get; set; } = string.Empty;

        /// <summary>
        /// 访问数量
        /// </summary>
        public string VisitCount { get; set; } = string.Empty;

        /// <summary>
        /// 更新内容
        /// </summary>
        public string UpdateContent { get; set; } = string.Empty;

        /// <summary>
        /// 强制更新版本
        /// </summary>
        public string LowVersion { get; set; } = string.Empty;

        /// <summary>
        /// MD5值
        /// </summary>
        public string Md5 { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; } = string.Empty;

        /// <summary>
        /// 下载链接
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// 是否强制更新
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// 软件ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// 卡密信息实体类
    /// </summary>
    public class CardInfo
    {
        /// <summary>
        /// 卡密编号
        /// </summary>
        public string CardNumber { get; set; } = string.Empty;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// 机器码
        /// </summary>
        public string MachineCode { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; } = string.Empty;

        /// <summary>
        /// 时长（0为永久，单位：天）
        /// </summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置软件ID，用于标识和识别特定的软件实例。
        /// </summary>
        public string SoftwareId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录请求参数
    /// </summary>
    internal class LoginRequest
    {
           public object fingerId { get; set; } = string.Empty;
        public string user { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool agree { get; set; } = true;
    }

    /// <summary>
    /// 分页请求基础类
    /// </summary>
    internal class PageRequest
    {
        public PageInfo page { get; set; } = new PageInfo();
    }

    /// <summary>
    /// 分页信息
    /// </summary>
    internal class PageInfo
    {
        public int limit { get; set; }
        public int count { get; set; } = 0;
        public int pageNum { get; set; } = 1;
    }

    /// <summary>
    /// 获取软件列表请求参数
    /// </summary>
    internal class GetSoftwareListRequest : PageRequest
    {
        public string softwareName { get; set; } = string.Empty;
        public string softwareId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 获取卡密列表请求参数
    /// </summary>
    internal class GetCardListRequest : PageRequest
    {
        public string privateKey { get; set; } = string.Empty;
        public object status { get; set; } = string.Empty;
        public string day { get; set; } = string.Empty;
        public string authId { get; set; } = string.Empty;
        public string macid { get; set; } = string.Empty;
        public string remark { get; set; } = string.Empty;
        public string softwareId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建卡密请求参数
    /// </summary>
    internal class CreateCardRequest
    {
        public string softwareId { get; set; } = string.Empty;
        public int day { get; set; }
        public int createNumber { get; set; }
        public string remark { get; set; } = string.Empty;
        public object bindCount { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建软件请求参数
    /// </summary>
    internal class CreateSoftwareRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    /// <summary>
    /// 通用返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ApiResult<T>
    {
        public int code { get; set; }
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public T data { get; set; } = default!;
    }
}
