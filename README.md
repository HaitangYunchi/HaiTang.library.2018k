## HaiTang.library Update API 调用手册


 ### 概述

HaiTang.library.Update 类提供了与 [2018k](http://2018k.cn) API 接口的完整封装，包括软件更新、用户管理、卡密验证、云变量操作等功能。本库支持多API地址故障转移、健康检测和加密通信。

 ### 快速开始

 **1. 初始化** 

 **软件实例初始化** 

```csharp
var update = new Update();
var softwareInfo = await update.InitializationAsync("软件ID", "开发者密钥", "可选机器码");
```

 **用户初始化** 

```csharp
var userInfo = await update.InitializationUserAsync("软件ID", "开发者密钥", "邮箱", "密码");
```

### 软件实例方法 

  **1. 检测软件实例状态** 

```csharp
bool isValid = await update.GetSoftCheck("软件ID", "开发者密钥", "可选机器码");
```

 **· 参数:
  · ID: 程序实例ID
  · key: OpenID/开发者密钥
  · Code: 机器码（可选，为空时自动获取）
· 返回值: bool - 实例是否有效** 

 **2. 获取软件信息** 

 **获取全部信息** 

```csharp
string allInfo = await update.GetSoftAll();
// 返回格式化的JSON字符串
```

 **获取特定信息** 

```csharp
string softwareId = await update.GetSoftwareID();        // 实例ID
string version = await update.GetVersionNumberl();       // 版本号
string name = await update.GetSoftwareName();            // 软件名称
string updateInfo = await update.GetVersionInformation(); // 更新内容
string notice = await update.GetNotice();                // 公告
string downloadLink = await update.GetDownloadLink();    // 下载链接
string visits = await update.GetNumberOfVisits();        // 访问量
string minVersion = await update.GetMiniVersion();       // 最低版本
```

 **3. 卡密相关操作** 

 **检查卡密状态** 

```csharp
bool isValid = await update.GetIsItEffective();
```

 **· 返回值: bool - 卡密是否有效** 

 **获取卡密信息** 

```csharp
string expireDate = await update.GetExpirationDate();    // 过期时间戳
string remarks = await update.GetRemarks();              // 备注
string days = await update.GetNumberOfDays();            // 有效期天数
string authId = await update.GetNetworkVerificationId(); // 卡密ID
```

 **激活卡密** 

```csharp
var (success, message) = await update.ActivationKey("卡密ID");
```

 **· 参数: authId - 卡密ID
· 返回值: (bool, string) - (成功标志, 消息)** 
 **
创建卡密** 

```csharp
string result = await update.CreateNetworkAuthentication(30, "测试卡密");
// 返回JSON格式的卡密信息
```

 **· 参数:
  · day: 有效期天数
  · remark: 卡密备注** 

 **解绑/换绑** 

```csharp
var (success, message) = await update.ReplaceBind("卡密ID", "新机器码");
```

 **· 参数:
  · AuthId: 卡密ID
  · Code: 新机器码（可选** ）

 **4. 云变量操作** 

 **获取云变量** 

```csharp
string value = await update.GetCloudVariables("变量名");
```

 **· 参数: VarName - 云端变量名称
· 返回值: string - 变量值** 

 **设置/更新云变量** 

```csharp
var (success, message) = await update.updateCloudVariables("变量名", "新值");
```

 **· 参数:
  · VarKey: 变量名
  · Value: 变量值
· 返回值: (bool, string) - (成功标志, 消息)** 

 **5. 其他操作** 

 **发送消息** 

```csharp
string response = await update.MessageSend("需要发送的消息");
// 返回服务器响应JSON
```

 **获取服务器时间** 

```csharp
string timestamp = await update.GetTimeStamp();
```

 **检查强制更新** 

```csharp
bool forceUpdate = await update.GetMandatoryUpdate();
```

 **获取软件MD5** 

```csharp
string md5 = await update.GetSoftwareMd5();
```

 **获取剩余使用时间** 

```csharp
long remainingTime = await update.GetRemainingUsageTime();
```

 **· 返回值:
  · -1: 永久
  · 0: 已过期
  · 1: 未注册
  · 其他: 剩余时间戳（毫 ） **

 **获取网络验证码** 

```csharp
string captcha = await update.GetNetworkCode();
```

### 用户管理方法 

 **1. 用户注册** 

```csharp
bool success = await update.CustomerRegister("email@example.com", "password", "昵称", "头像URL", "验证码");
```

 **· 参数:
  · email: 邮箱
  · password: 密码
  · nickName: 昵称（可选）
  · avatarUrl: 头像URL（可选）
  · captcha: 验证码（可选）
· 返回值: bool - 注册是否成功** 

 **2. 获取用户信息** 

 **获取全部用户信息** 

```csharp
string userInfoJson = await update.GetUserInfo();
// 返回格式化的JSON字符串
```

 **获取特定用户信息** 

```csharp
string userId = await update.GetUserId();          // 用户ID
string avatar = await update.GetUserAvatar();      // 用户头像
string nickname = await update.GetUserNickname();  // 用户昵称
string email = await update.GetUserEmail();        // 用户邮箱
string balance = await update.GetUserBalance();    // 账户余额/时长
bool license = await update.GetUserLicense();      // 是否授权
string loginTime = await update.GetUserTimeCrypt();// 登录时间戳
```

 **3. 卡密充值** 

```csharp
string result = await update.Recharge("卡密ID");
// 返回服务器响应JSON
```

 **加密解密方法** 

 **1. AES加密** 

```csharp
string encrypted = update.AesEncrypt(dataObject, "十六进制密钥");
```

 **· 参数:
  · data: 要加密的数据对象
  · key: 十六进制格式的密钥
· 返回值: string - Base64编码的加密字符串** 

 **2. AES解密** 

```csharp
string decrypted = update.AesDecrypt("加密字符串", "十六进制密钥");
```

 **· 参数:
  · encryptedData: Base64编码的加密字符串
  · key: 十六进制格式的密钥
· 返回值: string - 解密后的字符串** 


### 缓存机制

 **软件信息缓存** 

·  **缓存时间:**  5分钟
·  **相关方法:** 
  · GetCachedSoftwareInfo(): 获取缓存
  · SetCachedSoftwareInfo(): 设置缓存
  · IsCacheValid(): 检查缓存有效性
  · ClearStaticCache(): 清除缓存

 **用户信息缓存** 

·  **缓存时间:**  5分钟
·  **相关方法:** 
  · GetCachedUserInfo(): 获取缓存
  · SetCachedUserInfo(): 设置缓存
  · IsUserCacheValid(): 检查缓存有效性
  · ClearUserCache(): 清除缓存

### 故障转移机制

 **健康检测** 

· 自动检测API地址健康状态
· 5分钟缓存检测结果
· 支持多个备用地址自动切换

 **网络检查** 

· 自动检测网络连接状态
· 网络不可用时使用本地地址

### 工具方法

 **机器码获取** 

```csharp
Tools.GetMachineCodeEx()  // 获取机器码
```

 **程序更新** 

```csharp
Update.upgrade("下载地址")  // 启动更新程序
```

### 注意事项

1. 初始化顺序: 调用具体方法前需要先调用对应的初始化方法
2. 异步操作: 所有API调用都是异步的，需要使用 await
3. 错误处理: 建议对每个API调用进行异常捕获
4. 网络状态: 在网络不稳定时可能有重试机制
5. 缓存: 注意缓存可能导致数据不是实时最新的
6. 线程安全: 大多数方法是线程安全的，但建议避免并发初始化

### 示例代码

 **完整软件验证流程** 

```csharp
var update = new Update();

// 1. 初始化并检查软件状态
var softwareInfo = await update.InitializationAsync("your_software_id", "your_developer_key");
if (softwareInfo == null)
{
    Console.WriteLine("软件初始化失败");
    return;
}

// 2. 检查卡密状态
if (await update.GetIsItEffective())
{
    Console.WriteLine("卡密有效");
    
    // 3. 获取软件信息
    string version = await update.GetVersionNumberl();
    string notice = await update.GetNotice();
    
    // 4. 检查是否需要更新
    if (await update.GetMandatoryUpdate())
    {
        string downloadLink = await update.GetDownloadLink();
        Update.upgrade(downloadLink);
    }
}
else
{
    Console.WriteLine("卡密无效或已过期");
}
```

 **用户登录和充值** 

```csharp
// 1. 用户初始化
var userInfo = await update.InitializationUserAsync("software_id", "developer_key", "email", "password");

// 2. 获取用户信息
string nickname = await update.GetUserNickname();
string balance = await update.GetUserBalance();

// 3. 卡密充值
string rechargeResult = await update.Recharge("card_id");
```

这个调用手册涵盖了 HaiTang.library.Update 类的主要公开API方法，包括参数说明、返回值说明和使用示例。使用时请根据实际需求选择合适的API方法。