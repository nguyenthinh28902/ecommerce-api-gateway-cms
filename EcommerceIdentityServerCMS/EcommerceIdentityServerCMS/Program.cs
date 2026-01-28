using EcommerceIdentityServerCMS.Common.Helpers;
using EcommerceIdentityServerCMS.Common.Helpers.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// trang signin
builder.Services.AddControllersWithViews();

//settings config
builder.Services.AddConfigAppSetting(builder.Configuration);
//identity server
builder.Services.AddAuthenticationIdentityServer(builder.Configuration);
builder.Services.AddAuthenticationExtensions(builder.Configuration);
builder.Services.AddServiceDI(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

app.UseHttpsRedirection();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
