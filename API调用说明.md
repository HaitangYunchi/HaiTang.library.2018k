# HaiTang.Library.2018k API 调用说明

## 概述

本文档详细介绍 HaiTang.Library.2018k 类库的 API 使用方法，包含代码示例和调用示例。

---

## 目录

1. [命名空间](#1-命名空间)
2. [核心类说明](#2-核心类说明)
3. [软件管理 API](#3-软件管理-api)
4. [用户管理 API](#4-用户管理-api)
5. [卡密验证 API](#5-卡密验证-api)
6. [云变量 API](#6-云变量-api)
7. [黑白名单 API](#7-黑白名单-api)
8. [工具类 API](#8-工具类-api)
9. [配置说明](#9-配置说明)

---

## 1. 命名空间

```csharp
using HaiTang.Library.Api2018k;
using HaiTang.Library.Api2018k.Models;
```

---

## 2. 核心类说明

| 类名 | 说明 |
|------|------|
| `Update` | 主 API 类，提供软件更新、用户管理、卡密验证等功能 |
| `Tools` | 工具类，提供加密、解密、机器码生成等工具方法 |
| `Constants` | 常量类，存储 API 地址、敏感信息等配置 |
| `Mysoft` | 软件信息模型 |
| `UserInfo` | 用户信息模型 |

---

## 3. 软件管理 API

### 3.1 初始化软件实例

**方法签名**:
```csharp
public async Task<(bool Success, Mysoft? config)> InitializationAsync(
    string? ID = null, 
    string? key = null, 
    string? Code = null
)
```

**参数说明**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|:---:|------|
| `ID` | `string` | 否 | 软件ID |
| `key` | `string` | 否 | 开发者密钥 |
| `Code` | `string` | 否 | 机器码（自动获取） |

**返回值**:
- `Success`: 是否初始化成功
- `config`: 软件配置信息

**示例**:
```csharp
using (var update = new Update())
{
    var (success, config) = await update.InitializationAsync(
        ID: "your_software_id",
        key: "your_developer_key"
    );
    
    if (success)
    {
        Console.WriteLine($"软件名称: {config.softwareName}");
        Console.WriteLine($"版本号: {config.versionNumber}");
    }
    else
    {
        Console.WriteLine("初始化失败");
    }
}
```

### 3.2 获取软件信息

#### 3.2.1 获取全部配置
```csharp
var softwareInfo = await update.GetSoftAll();
```

#### 3.2.2 获取软件ID
```csharp
string softwareId = await update.GetSoftwareID();
```

#### 3.2.3 获取版本号
```csharp
string version = await update.GetVersionNumber();
```

#### 3.2.4 获取下载链接
```csharp
string downloadUrl = await update.GetDownloadLink();
```

#### 3.2.5 获取公告
```csharp
string notice = await update.GetNotice();
```

#### 3.2.6 检查是否需要强制更新
```csharp
bool needUpdate = await update.GetMandatoryUpdate();
```

#### 3.2.7 获取剩余使用时间
```csharp
long remainingTime = await update.GetRemainingUsageTime();
// 返回值说明:
// -1: 永久有效
//  0: 已过期
//  1: 未激活
// >0: 剩余毫秒数
```

---

## 4. 用户管理 API

### 4.1 用户注册

**方法签名**:
```csharp
public async Task<bool> CustomerRegister(
    string email, 
    string password, 
    string? nickName = null, 
    string? avatarUrl = null, 
    string? captcha = null
)
```

**示例**:
```csharp
bool success = await update.CustomerRegister(
    email: "user@example.com",
    password: "password123",
    nickName: "用户名"
);
```

### 4.2 用户登录

**方法签名**:
```csharp
public async Task<UserInfo> InitializationUserAsync(
    string email, 
    string password,
    string? ID = null, 
    string? key = null
)
```

**示例**:
```csharp
UserInfo userInfo = await update.InitializationUserAsync(
    email: "user@example.com",
    password: "password123",
    ID: "your_software_id",
    key: "your_developer_key"
);

if (!string.IsNullOrEmpty(userInfo.Email))
{
    Console.WriteLine($"用户ID: {userInfo.CustomerId}");
    Console.WriteLine($"余额: {userInfo.Balance}");
}
```

### 4.3 获取用户信息

#### 4.3.1 获取用户ID
```csharp
string userId = await update.GetUserId();
```

#### 4.3.2 获取用户昵称
```csharp
string nickname = await update.GetUserNickname();
```

#### 4.3.3 获取用户余额
```csharp
int balance = await update.GetUserBalance();
```

#### 4.3.4 获取许可证状态
```csharp
bool hasLicense = await update.GetUserLicense();
```

### 4.4 用户充值

```csharp
string result = await update.Recharge("AUTH_CODE_123");
```

---

## 5. 卡密验证 API

### 5.1 激活软件

**方法签名**:
```csharp
public async Task<(bool success, string message)> ActivationKey(string authId)
```

**示例**:
```csharp
var (success, message) = await update.ActivationKey("YOUR_AUTH_ID");
if (success)
{
    Console.WriteLine("激活成功");
}
else
{
    Console.WriteLine($"激活失败: {message}");
}
```

### 5.2 创建网络认证

**方法签名**:
```csharp
public async Task<string> CreateNetworkAuthentication(
    int? day = null, 
    int? hour = null, 
    int? minute = null, 
    string? remark = null,
    string? bindCount = null
)
```

**示例**:
```csharp
string result = await update.CreateNetworkAuthentication(
    day: 30,
    remark: "测试授权",
    bindCount: "3"
);
```

### 5.3 替换/解绑绑定

**方法签名**:
```csharp
public async Task<(bool success, string message)> ReplaceBind(string AuthId, string? Code = null)
```

**示例**:
```csharp
// 解绑
var (success, message) = await update.ReplaceBind("AUTH_ID");

// 换绑到新机器
var (success, message) = await update.ReplaceBind("AUTH_ID", "NEW_MACHINE_CODE");
```

---

## 6. 云变量 API

### 6.1 获取单个云变量

**方法签名**:
```csharp
public async Task<string> GetCloudVariables(string VarName)
```

**示例**:
```csharp
string value = await update.GetCloudVariables("MyVariable");
```

### 6.2 获取所有云变量

**方法签名**:
```csharp
public async Task<string> GetCloudVarArray()
```

**示例**:
```csharp
string jsonResult = await update.GetCloudVarArray();
// 返回格式: {"key1":"value1","key2":"value2"}
```

### 6.3 更新云变量

**方法签名**:
```csharp
public async Task<(bool success, string message)> updateCloudVariables(string VarKey, string Value)
```

**示例**:
```csharp
var (success, message) = await update.updateCloudVariables(
    VarKey: "MyVariable",
    Value: "new_value"
);
```

---

## 7. 黑白名单 API

### 7.1 检查白名单

**方法签名**:
```csharp
public async Task<(bool Success, string Message)> GetWhiteList(string input)
```

**示例**:
```csharp
var (success, message) = await update.GetWhiteList("192.168.1.100");
if (success)
{
    Console.WriteLine(message); // "已在白名单中找到 192.168.1.100"
}
```

### 7.2 检查黑名单

**方法签名**:
```csharp
public async Task<(bool Success, string Message)> GetBlackList(string input)
```

**示例**:
```csharp
var (success, message) = await update.GetBlackList("bad_user@example.com");
```

---

## 8. 工具类 API

### 8.1 获取机器码

```csharp
// 新版（推荐）
string machineCode = Tools.GetMachineCodeEx();

// 旧版（已过时）
[Obsolete]
string machineCode = Tools.GetMachineCode();
```

### 8.2 加密解密

#### 8.2.1 AES 加密（简单模式）
```csharp
string encrypted = Tools.Encrypt("plain text", "hex_key_32_bytes");
string decrypted = Tools.Decrypt(encrypted, "hex_key_32_bytes");
```

#### 8.2.2 AES 加密（密码+盐模式）
```csharp
string encrypted = Tools.Encrypt("plain text", "password", "salt");
string decrypted = Tools.Decrypt(encrypted, "password", "salt");
```

#### 8.2.3 服务端加密解密
```csharp
var data = new { key = "value" };
string encrypted = Tools.ServerEncrypt(data, "hex_key");
string decrypted = Tools.ServerDecrypt(encrypted, "hex_key");
```

### 8.3 哈希计算

```csharp
string sha256 = Tools.Sha256("input string");
string sha512 = Tools.Sha512("input string");
```

### 8.4 生成随机字符串

```csharp
// 字母+数字（默认）
string random = Tools.GenerateRandomString(16);

// 仅字母
string letters = Tools.GenerateRandomString(16, type: 1);

// 仅数字
string numbers = Tools.GenerateRandomString(16, type: 2);
```

### 8.5 RSA2 密钥生成

```csharp
// 生成密钥对
var keyPair = Tools.GenerateRsa2KeyPair(2048);

// 保存到文件
Tools.SaveRsa2PemToFile("private.pem", "public.pem");
```

---

## 9. 配置说明

### 9.1 开发模式

```csharp
// 启用开发模式
Constants.DEVELOPMENT_MODE = true;

// 设置开发环境API地址
Constants.DEVELOPMENT_API_URL = "http://127.0.0.1";
```

### 9.2 静态方法操作缓存

```csharp
// 获取缓存的软件信息
Mysoft cached = Update.GetCachedSoftwareInfo();

// 检查缓存是否有效
bool isValid = Update.IsCacheValid();

// 清除缓存
Update.ClearStaticCache();

// 获取缓存的用户信息
UserInfo userCached = Update.GetCachedUserInfo();

// 清除用户缓存
Update.ClearUserCache();
```

---

## 10. 完整示例

### 示例 1: 软件更新检查

```csharp
using (var update = new Update())
{
    // 初始化
    var (success, config) = await update.InitializationAsync(
        ID: "YOUR_SOFTWARE_ID",
        key: "YOUR_DEVELOPER_KEY"
    );
    
    if (!success)
    {
        Console.WriteLine("初始化失败");
        return;
    }
    
    // 检查版本
    string currentVersion = "1.0.0";
    string latestVersion = await update.GetVersionNumber();
    
    if (new Version(latestVersion) > new Version(currentVersion))
    {
        Console.WriteLine($"发现新版本: {latestVersion}");
        
        // 检查是否强制更新
        bool mandatory = await update.GetMandatoryUpdate();
        if (mandatory)
        {
            Console.WriteLine("强制更新，正在下载...");
            string downloadUrl = await update.GetDownloadLink();
            // 执行下载逻辑
        }
    }
    
    // 获取公告
    string notice = await update.GetNotice();
    if (!string.IsNullOrEmpty(notice))
    {
        Console.WriteLine($"公告: {notice}");
    }
}
```

### 示例 2: 用户登录与充值

```csharp
using (var update = new Update())
{
    // 用户登录
    UserInfo user = await update.InitializationUserAsync(
        email: "user@example.com",
        password: "password123",
        ID: "YOUR_SOFTWARE_ID",
        key: "YOUR_DEVELOPER_KEY"
    );
    
    if (string.IsNullOrEmpty(user.Email))
    {
        Console.WriteLine("登录失败");
        return;
    }
    
    Console.WriteLine($"欢迎, {user.Nickname}");
    Console.WriteLine($"余额: {user.Balance}");
    
    // 使用卡密充值
    string result = await update.Recharge("AUTH_CODE_123");
    Console.WriteLine(result);
}
```

### 示例 3: 云变量操作

```csharp
using (var update = new Update())
{
    await update.InitializationAsync("YOUR_SOFTWARE_ID", "YOUR_DEVELOPER_KEY");
    
    // 获取所有云变量
    string allVars = await update.GetCloudVarArray();
    Console.WriteLine("所有云变量:");
    Console.WriteLine(allVars);
    
    // 获取单个变量
    string apiUrl = await update.GetCloudVariables("ApiUrl");
    Console.WriteLine($"ApiUrl: {apiUrl}");
    
    // 更新变量
    var (success, message) = await update.updateCloudVariables("ApiUrl", "https://new.api.com");
    if (success)
    {
        Console.WriteLine("更新成功");
    }
}
```

---

## 11. 注意事项

1. **资源释放**: `Update` 类实现了 `IDisposable` 接口，建议使用 `using` 语句确保资源正确释放。

2. **敏感信息**: 用户邮箱和密码使用 `SecureString` 存储，内存中自动加密。

3. **API 故障转移**: 类库自动实现多 API 地址故障转移，无需手动处理。

4. **缓存机制**: 软件信息和用户信息默认缓存 5 分钟，可通过静态方法手动清除。

5. **开发模式**: 设置 `Constants.DEVELOPMENT_MODE = true` 可使用本地开发服务器。

---

**生成时间**: 2026年06月11日