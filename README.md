# HaiTangYunchi.library.Update API 调用手册

## 概述

HaiTang.library.Update 类提供了与 [2018k](http://2018k.cn) API 接口的完整封装，包括软件更新、用户管理、卡密验证、云变量操作等功能。本库支持多 API 地址故障转移、健康检测和加密通信。

## 快速开始

### **初始化**

#### **软件实例初始化**

```c#
using HaiTang.library;
Update update = new();  // 实例化更新对象
var softwareInfo = await update.InitializationAsync("软件ID", "开发者密钥", "可选机器码");
```

#### **用户初始化**

```c#
var userInfo = await update.InitializationUserAsync("软件ID", "开发者密钥", "邮箱", "密码");
```

## 软件实例方法

### 1. 检测软件实例状态

```c#
bool isValid = await update.GetSoftCheck("软件ID", "开发者密钥", "可选机器码");
```

- **参数**:
  - `ID`: 程序实例ID
  - `key`: 开发者密钥
  - `Code`: 机器码（可选，为空时自动获取）
- **返回值**: `bool` - 实例是否有效

### 2. 获取软件信息

#### 初始化后直接调用

```c#
using HaiTang.library;
Update update = new();  // 实例化更新对象
var softwareInfo = await update.InitializationAsync("软件ID", "开发者密钥", "可选机器码");

string softwareId = softwareInfo.softwareId;        	// 实例ID
string version = softwareInfo.versionNumber;       		// 版本号
string name = softwareInfo.softwareName;            	// 软件名称
string updateInfo = softwareInfo.versionInformation; 	// 更新内容
string notice = softwareInfo.notice;                	// 公告
string downloadLink = softwareInfo.downloadLink;    	// 下载链接
int visits = softwareInfo.numberOfVisits;        		// 访问量
bool isItEffective = softwareInfo.isItEffective ;       // 是否激活
long expirationDate = softwareInfo.expirationDate;		// 过期时间戳(毫秒)
```

#### 方法获取特定信息

```c#
string allInfo = await update.GetSoftAll();					// 返回格式化的JSON字符串
string softwareId = await update.GetSoftwareID();        	// 实例ID
string version = await update.GetVersionNumber();       	// 版本号
string name = await update.GetSoftwareName();            	// 软件名称
string updateInfo = await update.GetVersionInformation();	// 更新内容
string notice = await update.GetNotice();					// 公告
string downloadLink = await update.GetDownloadLink();    	// 下载链接
string visits = await update.GetNumberOfVisits();        	// 访问量
string minVersion = await update.GetMiniVersion();       	// 最低版本
```

### 3. 卡密相关操作

#### 检查卡密状态

```c#
bool isValid = await update.GetIsItEffective();
```

- **返回值**: `bool` - 卡密是否有效

#### 获取卡密信息

```c#
string expireDate = await update.GetExpirationDate();    // 过期时间戳
string remarks = await update.GetRemarks();              // 备注
string days = await update.GetNumberOfDays();            // 有效期天数
string authId = await update.GetNetworkVerificationId(); // 卡密ID
```

#### 激活卡密

```c#
var (success, message) = await update.ActivationKey("卡密ID");
```

- **参数**: `authId` - 卡密ID
- **返回值**: `(bool, string)` - (成功标志, 消息)

#### 创建卡密

```c#
string result = await update.CreateNetworkAuthentication(30, "测试卡密");
// 返回JSON格式的卡密信息
```

- **参数**:
  - `day`: 有效期天数
  - `remark`: 卡密备注

#### 解绑/换绑

```c#
var (success, message) = await update.ReplaceBind("卡密ID", "新机器码");
```

- **参数**:
  - `AuthId`: 卡密ID
  - `Code`: 新机器码（为空则解绑）

### 4. 云变量操作

#### 获取云变量

```c#
string value = await update.GetCloudVariables("变量名");
```

- **参数**: `VarName` - 云端变量名称
- **返回值**: `string` - 变量值

#### 设置/更新云变量

```c#
var (success, message) = await update.updateCloudVariables("变量名", "新值");
```

- **参数**:
  - `VarKey`: 变量名
  - `Value`: 变量值
- **返回值**: `(bool, string)` - (成功标志, 消息)

### 5. 其他操作

#### 发送消息

```c#
string response = await update.MessageSend("需要发送的消息");
// 返回服务器响应JSON 无实际意义
```

#### 检查强制更新

```c#
bool forceUpdate = await update.GetMandatoryUpdate();
```

#### 获取服务器时间戳

```c#
string timestamp = await update.GetTimeStamp();
```

#### 获取剩余使用时间

```c#
long remainingTime = await update.GetRemainingUsageTime();
```

- **返回值**:
  - `-1`: 永久
  - `0`: 已过期
  - `1`: 未注册
  - 其他: 剩余时间戳（毫秒）

## 用户管理方法

### 1. 用户注册

```c#
bool success = await update.CustomerRegister("email", "password", "nickName", "avatarUrl", "captcha");
```

- **参数**:
  - `email`: 邮箱
  - `password`: 密码
  - `nickName`: 昵称（可选）
  - `avatarUrl`: 头像URL（可选）
  - `captcha`: 验证码（可选）
- **返回值**: `bool` - 注册是否成功

### 2. 获取用户信息

#### 获取全部用户信息

```c#
string userInfoJson = await update.GetUserInfo();
// 返回格式化的JSON字符串
```

#### 获取特定用户信息

```c#
string userId = await update.GetUserId();          // 用户ID
string avatar = await update.GetUserAvatar();      // 用户头像
string nickname = await update.GetUserNickname();  // 用户昵称
string email = await update.GetUserEmail();        // 用户邮箱
string balance = await update.GetUserBalance();    // 账户余额/时长
bool license = await update.GetUserLicense();      // 是否授权
string loginTime = await update.GetUserTimeCrypt();// 登录时间戳
```

### 3. 卡密充值

```c#
string result = await update.Recharge("卡密ID");
// 返回服务器响应JSON
```

## 工具方法

### 1.常用方法

```c#
Tools.GetMachineCodeEx();  							// 获取机器码
Tools.GenerateRandomString(int length,int type);	// 生成随机字符
Tools.GenerateSalt(int length = 64);  				// 生成随机盐值，默认为64字节
Tools.Sha256(string input);							// 生成SHA256哈希值
Tools.Sha512(string input);							// 生成SHA512哈希值
Tools.upgrade(string downloadLink);		        	// 启动更新程序
```

### 2.程序更新

```c#
Tools.upgrade(string downloadLink);	// 启动更新程序
```

### 3.AES加密 自动IV

```c#
Tools.Encrypt(string plainText,string key);		// AES加密
Tools.Decrypt(string cipherText, string key);	// AES解密
```

### 4.AES加密 自动IV带盐值和密码

```c#
Tools.Encrypt(string plainText, string password, string salt);	// AES加密
Tools.Decrypt(string cipherText, string password, string salt);	// AES解密
```

### Log日志类方法

Log 类是一个静态日志工具类，提供按天分割的日志文件记录功能。日志文件默认存储在应用程序根目录下的 `Logs` 文件夹中

#### 日志格式

```tex
{时间戳} [{日志级别}] {类名}.{方法名} - {消息内容} {异常信息}
示例：2025-12-01 15:02:46.1234 [INFO] OrderService.ProcessOrder - 开始处理订单 #1001
```

#### 类方法

##### `Debug(string message)`

**描述**
记录 Debug 级别的日志消息，通常用于开发阶段的调试信息记录。

**参数**

- `message` (string): 要记录的调试信息

**调用示例**

```c#
// 记录调试信息
Log.Debug("开始处理用户请求，参数: {param}");
Log.Debug("缓存命中率: {rate}%");
Log.Debug("内存使用情况: {used}/{total} MB");
```

**输出示例**

```tex
2025-12-01 15:02:46.1234 [DEBUG] UserController.GetUser - 开始处理用户请求，参数: 2345
2025-12-01 15:02:47.2345 [DEBUG] CacheManager.GetData - 缓存命中率: 85.5%
2025-12-01 15:02:48.3456 [DEBUG] MemoryMonitor.Check - 内存使用情况: 512/1024 MB
```

##### `Info(string message)`

**描述**
记录 Info 级别的日志消息，用于记录应用程序的正常运行状态信息。

**参数**

- `message` (string): 要记录的一般信息

**调用示例**

```c#
// 记录应用程序状态信息
Log.Info("应用程序启动成功");
Log.Info("用户 'admin' 登录系统");
Log.Info("数据库连接池初始化完成，连接数: {count}");
Log.Info("定时任务执行完成，耗时: {elapsed}ms");
```

**输出示例**

```tex
2025-12-01 15:02:46.1234 [INFO] Program.Main - 应用程序启动成功
2025-12-01 15:02:47.2345 [INFO] AuthService.Login - 用户 'admin' 登录系统
2025-12-01 15:02:48.3456 [INFO] DatabasePool.Initialize - 数据库连接池初始化完成，连接数: 20
2025-12-01 15:02:49.4567 [INFO] Scheduler.Execute - 定时任务执行完成，耗时: 1250ms
```

##### `Warn(string message)`

**描述**
记录 Warn 级别的日志消息，用于记录可能需要关注的潜在问题或异常情况。

**参数**

- `message` (string): 要记录的警告信息

**调用示例**

```c#
// 记录警告信息
Log.Warn("数据库连接池使用率过高: {percentage}%");
Log.Warn("API响应时间超过阈值: {time}ms (阈值: {threshold}ms)");
Log.Warn("配置文件 {file} 不存在，使用默认配置");
Log.Warn("磁盘空间不足，剩余: {freeSpace}GB");
```

**输出示例**

```tex
2025-12-01 15:02:46.1234 [WARN] DatabasePool.Monitor - 数据库连接池使用率过高: 85%
2025-12-01 15:02:47.2345 [WARN] ApiMonitor.CheckResponse - API响应时间超过阈值: 1200ms (阈值: 1000ms)
2025-12-01 15:02:48.3456 [WARN] ConfigManager.Load - 配置文件 appsettings.custom.json 不存在，使用默认配置
2025-12-01 15:02:49.4567 [WARN] DiskMonitor.Check - 磁盘空间不足，剩余: 2.5GB
```

##### `Error(string message)`

**描述**
记录 Error 级别的日志消息，用于记录不影响应用程序继续运行的错误。

**参数**

- `message` (string): 要记录的错误信息

**调用示例**

```c#
// 记录错误信息（无异常）
Log.Error("文件上传失败：文件大小超过限制");
Log.Error("用户权限验证失败，用户ID: {userId}");
Log.Error("API请求失败，HTTP状态码: {statusCode}");
Log.Error("数据验证失败，字段 '{field}' 格式错误");
```

**输出示例**

```tex
2025-12-01 15:02:46.1234 [ERROR] FileService.Upload - 文件上传失败：文件大小超过限制
2025-12-01 15:02:47.2345 [ERROR] AuthService.CheckPermission - 用户权限验证失败，用户ID: 12345
2025-12-01 15:02:48.3456 [ERROR] ApiClient.Request - API请求失败，HTTP状态码: 404
2025-12-01 15:02:49.4567 [ERROR] Validator.Validate - 数据验证失败，字段 'email' 格式错误
```

##### `Error(Exception ex, string message)`

**描述**
记录 Error 级别的日志消息，包含异常详细信息，用于记录包含异常详细信息的错误。

**参数**

- `ex` (Exception): 相关的异常对象
- `message` (string): 要记录的错误描述信息

**调用示例**

```c#
try
{
    // 可能抛出异常的代码
    var result = ProcessData(input);
}
catch (ArgumentNullException ex)
{
    Log.Error(ex, "数据处理失败：输入参数为空");
}
catch (FormatException ex)
{
    Log.Error(ex, "数据格式转换失败");
}
catch (IOException ex)
{
    Log.Error(ex, "文件读写操作失败");
}
catch (Exception ex)
{
    Log.Error(ex, "处理过程中发生未知错误");
}
```

**输出示例**

```tex
2025-12-01 15:02:46.1234 [ERROR] DataProcessor.Process - 数据处理失败：输入参数为空 
System.ArgumentNullException: 值不能为 null。
参数名: input
   在 DataProcessor.ValidateInput(String input) 位置 DataProcessor.cs:行号 42
   在 DataProcessor.Process(String input) 位置 DataProcessor.cs:行号 25

2025-12-01 15:02:47.2345 [ERROR] Converter.ConvertToInt - 数据格式转换失败 
System.FormatException: 输入字符串的格式不正确。
   在 System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer& number, NumberFormatInfo info, Boolean parseDecimal)
   在 Converter.ConvertToInt(String value) 位置 Converter.cs:行号 18

2025-12-01 15:02:48.3456 [ERROR] FileService.ReadFile - 文件读写操作失败 
System.IO.IOException: 文件 'C:\data\config.json' 正由另一进程使用，因此该进程无法访问此文件。
   在 System.IO.FileStream.ValidateFileHandle(SafeFileHandle fileHandle)
   在 FileService.ReadFile(String path) 位置 FileService.cs:行号 33
```

##### `Fatal(string message)`

**描述**
记录 Fatal 级别的日志消息，用于记录导致应用程序无法继续运行的严重错误。

**参数**

- `message` (string): 要记录的严重错误信息

**调用示例**

```c#
// 记录致命错误
Log.Fatal("数据库连接完全失败，应用程序无法启动");
Log.Fatal("关键配置文件缺失，系统无法运行");
Log.Fatal("系统内存耗尽，应用程序即将崩溃");
Log.Fatal("未处理的异常导致应用程序终止");
```

**输出示例**

```tex
2025-12-01 15:02:46.1234 [FATAL] Program.Main - 数据库连接完全失败，应用程序无法启动
2025-12-01 15:02:47.2345 [FATAL] ConfigManager.Initialize - 关键配置文件缺失，系统无法运行
2025-12-01 15:02:48.3456 [FATAL] MemoryManager.Allocate - 系统内存耗尽，应用程序即将崩溃
2025-12-01 15:02:49.4567 [FATAL] GlobalExceptionHandler.Handle - 未处理的异常导致应用程序终止
```

#### 全局捕获异常

对于没捕获的异常记录，我们可以试着在全局异常捕获中记录未处理的异常，方便以后在日志中查看未知异常

##### 1. 控制台应用程序

```c#
using System;
using System.Threading.Tasks;
using HaiTang.library;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // 设置全局异常处理
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    Log.Fatal($"未处理的应用程序域异常，应用程序将{(e.IsTerminating ? "终止" : "继续")}");
                    Log.Error(exception, "异常详情");
                }
                
                Console.WriteLine("发生严重错误，请查看日志文件。");
            };

            // 处理异步任务异常
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved(); // 避免进程崩溃
                Log.Error(e.Exception, "未观察到的异步任务异常");
            };

            Log.Info("控制台应用程序启动");
            
            // 模拟业务逻辑
            RunApplication();
            
            Log.Info("应用程序正常结束");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "主线程异常");
            Console.WriteLine($"应用程序异常：{ex.Message}");
        }
    }

    static void RunApplication()
    {
        // 正常业务代码
        Log.Debug("执行业务逻辑");
        
        // 模拟一个会抛出异常的操作
        Console.WriteLine("请输入一个数字进行测试：");
        var input = Console.ReadLine();
        
        // 触发一个异常（会被全局异常处理器捕获）
        Task.Run(() =>
        {
            throw new InvalidOperationException("后台任务中的异常");
        });

        // 主线程中故意抛出异常
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException(nameof(input), "输入不能为空");
        }
    }
}
```

##### 2. WinForms 应用程序

```c#
using System;
using System.Windows.Forms;
using HaiTang.library;

namespace WinFormsApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // 设置UI线程异常处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
            {
                Log.Error(e.Exception, "UI线程未处理异常");
                MessageBox.Show($"发生错误：{e.Exception.Message}\n\n详细信息已记录到日志。",
                    "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // 设置非UI线程异常处理
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
                    Log.Fatal($"非UI线程未处理异常，应用程序将{(e.IsTerminating ? "终止" : "继续")}");
                    Log.Error(exception, "异常详情");
                }
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Log.Info("WinForms应用程序启动");
            Application.Run(new MainForm());
            Log.Info("WinForms应用程序结束");
        }
    }

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            
            // 添加一个会抛出异常的按钮
            var btnThrow = new Button
            {
                Text = "点击抛出异常",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(150, 30)
            };
            
            btnThrow.Click += (sender, e) =>
            {
                try
                {
                    throw new InvalidOperationException("按钮点击触发的异常");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "按钮点击异常");
                    MessageBox.Show($"操作失败：{ex.Message}");
                }
            };
            
            this.Controls.Add(btnThrow);
        }
    }
}
```

##### 3. WPF 应用程序

```xaml
<!-- App.xaml -->
<Application x:Class="WpfApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             Startup="Application_Startup"
             DispatcherUnhandledException="Application_DispatcherUnhandledException"
             Exit="Application_Exit">
</Application>
```

```c#
using System;
using System.Windows;
using System.Windows.Threading;
using HaiTang.library;

namespace WpfApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 应用程序域异常处理
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                if (exception != null)
                {
                    Log.Fatal($"应用程序域未处理异常，应用程序将{(args.IsTerminating ? "终止" : "继续")}");
                    Log.Error(exception, "异常详情");
                }
            };

            // 异步任务异常处理
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
                Log.Error(args.Exception, "未观察到的异步任务异常");
            };

            Log.Info("WPF应用程序启动");
            
            // 创建主窗口
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // 阻止应用程序崩溃
            
            Log.Error(e.Exception, "UI线程未处理异常");
            
            var result = MessageBox.Show(
                $"发生错误：{e.Exception.Message}\n\n是否继续运行应用程序？",
                "应用程序错误",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error);
            
            if (result == MessageBoxResult.No)
            {
                Shutdown(1);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Log.Info("WPF应用程序退出");
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log.Info("主窗口初始化完成");
            
            // 添加抛出异常的按钮
            var btnThrow = new System.Windows.Controls.Button
            {
                Content = "点击抛出异常",
                Width = 150,
                Height = 30,
                Margin = new Thickness(20)
            };
            
            btnThrow.Click += (sender, e) =>
            {
                try
                {
                    throw new InvalidOperationException("WPF按钮点击异常");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "按钮点击异常");
                    MessageBox.Show($"操作失败：{ex.Message}", "错误", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            
            this.Content = btnThrow;
        }
    }
}
```

## 缓存机制

### 软件信息缓存

- 缓存时间: 5分钟
- 相关方法:
  - `GetCachedSoftwareInfo()`: 获取缓存
  - `SetCachedSoftwareInfo()`: 设置缓存
  - `IsCacheValid()`: 检查缓存有效性
  - `ClearStaticCache()`: 清除缓存

### 用户信息缓存

- 缓存时间: 5分钟
- 相关方法:
  - `GetCachedUserInfo()`: 获取缓存
  - `SetCachedUserInfo()`: 设置缓存
  - `IsUserCacheValid()`: 检查缓存有效性
  - `ClearUserCache()`: 清除缓存

## 故障转移机制

### 健康检测

- 自动检测 API 地址健康状态
- 5分钟缓存检测结果
- 支持多个备用地址自动切换

### 网络检查

- 自动检测网络连接状态
- 网络不可用时返回 NULL;

## 注意事项

1. **初始化顺序**: 调用具体方法前需要先调用对应的初始化方法
2. **异步操作**: 所有API调用都是异步的，需要使用 `await`
3. **错误处理**: 建议对每个API调用进行异常捕获
4. **网络状态**: 在网络不稳定时可能有重试机制
5. **缓存**: 注意缓存可能导致数据不是实时最新的
6. **线程安全**: 大多数方法是线程安全的，但建议避免并发初始化

## 示例代码

### 完整软件验证流程

```c#
using HaiTang.library;
Update update = new();  // 实例化更新对象

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
    string version = await update.GetVersionNumber();
    string notice = await update.GetNotice();
    
    // 4. 检查是否需要更新
    if (await update.GetMandatoryUpdate())
    {
        string downloadLink = await update.GetDownloadLink();
        Tools.upgrade(downloadLink);
        // 退出当前应用程序
		Application.Exit();
    }
}
else
{
    Console.WriteLine("卡密无效或已过期");
}
```

### 用户登录和充值

```c#
// 1. 用户初始化
var userInfo = await update.InitializationUserAsync("software_id", "developer_key", "email", "password");

// 2. 获取用户信息
string nickname = await update.GetUserNickname();
string balance = await update.GetUserBalance();

// 3. 卡密充值
string rechargeResult = await update.Recharge("card_id");
```

这个调用手册涵盖了 HaiTang.library.Update 类的主要公开 API 方法，包括参数说明、返回值说明和使用示例。使用时请根据实际需求选择合适的API方法。