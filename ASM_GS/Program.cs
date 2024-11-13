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

// 2. C?u h�nh c�c t�y ch?n li�n quan ??n x�c th?c Google
builder.Services.Configure<GeminiSettings>(configuration.GetSection("Authentication"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Login"; // ???ng d?n ?? ng??i d�ng ??ng nh?p
    options.AccessDeniedPath = "/Home/AccessDenied"; // ???ng d?n khi ng??i d�ng b? t? ch?i quy?n truy c?p
})
.AddGoogle(options =>
{
    options.ClientId = configuration["Authentication:Google:ClientId"];
    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
});

// 3. C?u h�nh m?c ??nh cho Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Y�u c?u x�c nh?n t�i kho?n tr??c khi ??ng nh?p
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// 4. C?u h�nh gi?i h?n k�ch th??c t?p t?i l�n
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Gi?i h?n k�ch th??c t?p t?i l�n l� 100 MB
});

// 5. C?u h�nh CORS ?? cho ph�p c�c ngu?n kh�c nhau truy c?p (c?n thi?t n?u s? d?ng API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 6. C?u h�nh c�c d?ch v? MVC v� Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 7. C?u h�nh phi�n l�m vi?c
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian h?t h?n c?a phi�n l�m vi?c
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 8. T�y ch?n cho l?i x�c th?c v� quy?n truy c?p
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

// 9. C?u h�nh pipeline x? l� c�c y�u c?u HTTP
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

app.UseCors("AllowAllOrigins"); // S? d?ng ch�nh s�ch CORS cho API
app.UseSession(); // S? d?ng Session

app.UseAuthentication(); // Th�m middleware x�c th?c
app.UseAuthorization(); // Th�m middleware ?y quy?n

// C?u h�nh c�c tuy?n ???ng ?i?u khi?n (Controllers)
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// C?u h�nh cho Razor Pages (n?u c�)
app.MapRazorPages();

app.Run();
