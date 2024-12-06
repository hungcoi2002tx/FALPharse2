using FAL.FrontEnd.Middleware;
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
    client.BaseAddress = new Uri("https://dev.demorecognition.click/"); // TODO: sửa theo domain đúng
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpContextAccessor(); // Đăng ký IHttpContextAccessor

// Required for session handling
//// Thêm Authentication và Cookie Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Login"; // Trang đăng nhập
//        options.AccessDeniedPath = "/AccessDenied"; // Trang không đủ quyền truy cập
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Thời gian hết hạn cookie
//        options.SlidingExpiration = true; // Gia hạn thời gian nếu người dùng hoạt động
//    });
//builder.Services.AddHttpContextAccessor(); // Ensure HttpContext is accessible
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "1"));
//});
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("System", policy => policy.RequireClaim(ClaimTypes.Role, "2"));
//});
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.UseMiddleware<SessionAuthMiddleware>();
app.UseMiddleware<RoleAuthorizationMiddleware>();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Dashboard/Main");
    return Task.CompletedTask;
});
app.Run();
