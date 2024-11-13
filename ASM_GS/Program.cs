using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using static ASM_GS.Controllers.HomeController;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. Thi?t l?p k?t n?i c? s? d? li?u
var connectionString = configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. C?u hình các tùy ch?n liên quan ??n xác th?c Google
builder.Services.Configure<GeminiSettings>(configuration.GetSection("Authentication"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Login"; // ???ng d?n ?? ng??i dùng ??ng nh?p
    options.AccessDeniedPath = "/Home/AccessDenied"; // ???ng d?n khi ng??i dùng b? t? ch?i quy?n truy c?p
})
.AddGoogle(options =>
{
    options.ClientId = configuration["Authentication:Google:ClientId"];
    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
});

// 3. C?u hình m?c ??nh cho Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Yêu c?u xác nh?n tài kho?n tr??c khi ??ng nh?p
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// 4. C?u hình gi?i h?n kích th??c t?p t?i lên
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Gi?i h?n kích th??c t?p t?i lên là 100 MB
});

// 5. C?u hình CORS ?? cho phép các ngu?n khác nhau truy c?p (c?n thi?t n?u s? d?ng API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 6. C?u hình các d?ch v? MVC và Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 7. C?u hình phiên làm vi?c
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian h?t h?n c?a phiên làm vi?c
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 8. Tùy ch?n cho l?i xác th?c và quy?n truy c?p
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
//        options.SlidingExpiration = true;
//        options.AccessDeniedPath = "/Home/AccessDenied";
//        options.LoginPath = "/Home/Login";
//        options.LogoutPath = "/Home/Logout";
//        options.Events = new CookieAuthenticationEvents
//        {
//            OnRedirectToAccessDenied = context =>
//            {
//                context.Response.StatusCode = 403;
//                context.Response.Redirect("/Error/Forbidden");
//                return Task.CompletedTask;
//            },
//            OnRedirectToLogin = context =>
//            {
//                context.Response.StatusCode = 401;
//                context.Response.Redirect("/Home/Login");
//                return Task.CompletedTask;
//            },
//            OnRedirectToLogout = context =>
//            {
//                context.Response.Redirect("/Home/Logout");
//                return Task.CompletedTask;
//            },
//            OnRedirectToReturnUrl = context =>
//            {
//                if (context.Response.StatusCode == 404)
//                {
//                    context.Response.Redirect("/Error/404");
//                }
//                return Task.CompletedTask;
//            }
//        };
//    });

var app = builder.Build();

// 9. C?u hình pipeline x? lý các yêu c?u HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAllOrigins"); // S? d?ng chính sách CORS cho API
app.UseSession(); // S? d?ng Session

app.UseAuthentication(); // Thêm middleware xác th?c
app.UseAuthorization(); // Thêm middleware ?y quy?n

// C?u hình các tuy?n ???ng ?i?u khi?n (Controllers)
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// C?u hình cho Razor Pages (n?u có)
app.MapRazorPages();

app.Run();
