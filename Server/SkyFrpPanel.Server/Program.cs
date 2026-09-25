using AspNetCoreRateLimit;
using SkyFrpPanel.Common.Cache;
using SkyFrpPanel.Common.DynamicApiSimple.Extens;
using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.WebExtensions;
using SkyFrpPanel.Mall;
using SkyFrpPanel.ServiceCore.Signalr;
using SkyFrpPanel.ServiceCore.SqlSugar;
using SkyFrpPanel.Extensions;
using SkyFrpPanel.Infrastructure.Converter;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using NLog.Web;
using Scalar.AspNetCore;
using SqlSugar;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Collections.Generic;



var builder = WebApplication.CreateBuilder(args);



// 隐藏默认Server
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// NLog: Setup NLog for Dependency injection
builder.Host.UseNLog();

builder.Services.AddDynamicApi();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//注入HttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// 跨域配置
builder.Services.AddCors(builder.Configuration);
//消除Error unprotecting the session cookie警告
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "DataProtection"));
//普通验证码
builder.Services.AddCaptcha(builder.Configuration);
//IPRatelimit
builder.Services.AddIPRate(builder.Configuration);
//绑定整个对象到Model上
builder.Services.Configure<OptionsSetting>(builder.Configuration);
builder.Configuration.AddJsonFile("codeGen.json");
builder.Configuration.AddJsonFile("iprate.json");
//jwt 认证
builder.Services.AddJwt();
//配置文件
builder.Services.AddSingleton(new AppSettings(builder.Configuration));
//app服务注册
builder.Services.AddAppService();
//开启计划任务
builder.Services.AddTaskSchedulers();
//请求大小限制
builder.Services.AddRequestLimit(builder.Configuration);
//注册REDIS 服务
var openRedis = builder.Configuration["RedisServer:open"];
if (openRedis == "1")
{
    RedisServer.Initalize();
}

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(GlobalActionMonitor));//全局注册
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new JsonConverterUtil.DateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonConverterUtil.DateTimeNullConverter());
    options.JsonSerializerOptions.Converters.Add(new StringConverter());
    //PropertyNamingPolicy属性用于前端传过来的属性的格式策略，目前内置的仅有一种策略CamelCase
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
//注入SignalR实时通讯，默认用json传输
builder.Services.AddSignalR()
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
// 显示logo
builder.Services.AddLogo();
// 添加本地化服务
builder.Services.AddLocalization(options => options.ResourcesPath = "");

// 注册TcpServer
//builder.Services.AddSingleton<ITcpServer, TcpServer>();
// 1. 启动阶段直接创建实例
//var zepServer = new FrpZepServer();
//// 2. 注册到DI，全程使用这个已创建的对象
//builder.Services.AddSingleton(zepServer);

// 初始化 XmlCommentHelper 用于读取 XML 注释
var xmlCommentHelper = new SkyFrpPanel.Infrastructure.Helper.XmlCommentHelper();
var xmlFiles = new[]
{
    Path.Combine(AppContext.BaseDirectory, "SkyFrpPanel.Server.xml"),
    Path.Combine(AppContext.BaseDirectory, "SkyFrpPanel.Model.xml"),
    Path.Combine(AppContext.BaseDirectory, "SkyFrpPanel.ServiceCore.xml"),
    Path.Combine(AppContext.BaseDirectory, "SkyFrpPanel.Infrastructure.xml"),
    Path.Combine(AppContext.BaseDirectory, "SkyFrpPanel.Mall.xml"),
    Path.Combine(AppContext.BaseDirectory, "SkyFrpPanel.Service.xml"),
    Path.Combine(AppContext.BaseDirectory, "CommonRelyOn.xml")
};
var existingXmlFiles = xmlFiles.Where(f => File.Exists(f)).ToArray();
if (existingXmlFiles.Length > 0)
{
    xmlCommentHelper.Load(existingXmlFiles);
}

builder.Services.AddSingleton(xmlCommentHelper);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer<XmlCommentDocumentTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "SkyFrpPanel API";
        document.Info.Description = "SkyFrpPanel 后台管理系统接口文档";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
    options.AddOperationTransformer<AddVersionToHeaderTransformer>();
    options.AddOperationTransformer<XmlCommentOperationTransformer>();
});

// 在应用程序启动的最开始处调用
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SkyFrpPanel API 文档")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}


//自定义Server
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;

        headers["Server"] = "SkyFrpPanel/1.0";     // 自定义

        return Task.CompletedTask;
    });

    await next();
});


InternalApp.ServiceProvider = app.Services;
InternalApp.Configuration = builder.Configuration;
InternalApp.WebHostEnvironment = app.Environment;
//初始化db
builder.Services.AddDb(app.Environment);
builder.Services.InitDb(app.Environment);
// 启动时立即实例化所有标记了 EagerInit = true 的单例服务（需在DB初始化之后）
app.Services.InitEagerServices();
var workId = builder.Configuration["workId"].ParseToInt();
if (app.Environment.IsDevelopment())
{
    workId += 1;
}
SnowFlakeSingle.WorkId = workId;
//使用全局异常中间件
app.UseMiddleware<GlobalExceptionMiddleware>();

// 配置中间件以支持本地化
var supportedCultures = new List<CultureInfo> {
                    new CultureInfo("zh-Hant"),
                    new CultureInfo("zh-CN"),
                    new CultureInfo("en")
                };
app.UseRequestLocalization(options =>
{
    options.DefaultRequestCulture = new RequestCulture("zh-CN");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.FallBackToParentCultures = true;
});

//请求头转发
//ForwardedHeaders中间件会自动把反向代理服务器转发过来的X-Forwarded-For（客户端真实IP）以及X-Forwarded-Proto（客户端请求的协议）自动填充到HttpContext.Connection.RemoteIPAddress和HttpContext.Request.Scheme中，这样应用代码中读取到的就是真实的IP和真实的协议了，不需要应用做特殊处理。
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.Use((context, next) =>
{
    //设置可以多次获取body内容
    context.Request.EnableBuffering();
    if (context.Request.Query.TryGetValue("access_token", out var token))
    {
        context.Request.Headers.Append("Authorization", $"Bearer {token}");
    }
    return next();
});
//开启访问静态文件/wwwroot目录文件，要放在UseRouting前面
app.UseStaticFiles();
//开启路由访问
app.UseRouting();
app.UseCors("Policy");//要放在app.UseEndpoints前。

app.UseAuthentication();
app.UseMiddleware<JwtAuthMiddleware>();
app.UseAuthorization();

//开启缓存
app.UseResponseCaching();
if (builder.Environment.IsProduction())
{
    //恢复/启动任务
    app.UseAddTaskSchedulers();
}
//初始化字典数据
app.UseInit();

//启用客户端IP限制速率
app.UseIpRateLimiting();
app.UseRateLimiter();
//设置socket连接


app.MapHub<MessageHub>("/msgHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapControllers();
app.Run();