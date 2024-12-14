using FAL.FrontEnd.Helper;
using FAL.FrontEnd.Middleware;
using FAL.FrontEnd.Models;
using FAL.FrontEnd.Service;
using FAL.FrontEnd.Service.IService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("FaceDetectionAPI", client =>
{
    client.BaseAddress = new Uri(FEGlobalVarians.BE_URL); // TODO: sửa theo domain đúng
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<TokenExtentions>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IBaseApiService, BaseApiService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true; // Chỉ cho phép truy cập cookie qua HTTP
    options.Cookie.IsEssential = true; // Bắt buộc cookie dù bật chế độ GDPR
});
builder.Services.AddHttpContextAccessor(); // Đảm bảo HttpContext có thể truy cập được

var app = builder.Build();
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
FAL.FrontEnd.Helper.SessionExtensions.Configure(httpContextAccessor);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.UseStatusCodePages(context =>
{
    if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/Shared/NotFound");
    }

    return Task.CompletedTask;
});

app.UseMiddleware<SessionAuthMiddleware>();
app.UseMiddleware<RoleAuthorizationMiddleware>();

//app.MapGet("/", context =>
//{
//    context.Response.Redirect("/Dashboard/Main");
//    return Task.CompletedTask;
//});
app.Run();
