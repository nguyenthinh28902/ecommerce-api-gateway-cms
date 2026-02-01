using Ecommerce.ApiGateway.Cms.Common.Auth;
using Ecommerce.ApiGateway.Cms.Common.Helpers;
using Ecommerce.ApiGateway.Cms.Models.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.Configure<IdentityServerOptions>(
            builder.Configuration.GetSection("IdentityServerOptions"));
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Nạp file cấu hình (appsettings.reverseproxy.identity.json)
builder.Services.AddCustomAppSettings(builder.Configuration);

//redis 
builder.Services.AddStackExchangeRedis(builder.Configuration);
// 1. Cài đặt Authentication (Dùng hàm bạn đã viết)
builder.Services.AddGatewayAuthentication(builder.Configuration);

// 2. Cài đặt Proxy (Dùng hàm mới thêm AddTransforms ở trên)
builder.Services.AddGatewayProxy(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapControllers();

app.Run();
