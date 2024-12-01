
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static ASM_GS.Controllers.HomeController;
using ASM_GS.Controllers;
using ASM_GS.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ASM_GS.Controllers.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.Configure<GeminiSettings>(
    builder.Configuration.GetSection("Authentication"));
// Google authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ASM_GS.Controllers.ApplicationDbContext>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

});

builder.Services.AddSingleton<IVnPayServices, VnPayServices>();
var app = builder.Build();
// Configure the HTTP request pipeline.
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
app.UseSession();
app.UseAuthentication(); // Add authentication middleware before authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
//        options.SlidingExpiration = true;
//        options.AccessDeniedPath = "/Forbidden/";
//        options.LoginPath = "/Home/LoginAndSignUp/";
//        options.LogoutPath = "/Home/Logout/";
//        options.Events = new CookieAuthenticationEvents
//        {
//            OnRedirectToAccessDenied = context =>
//            {
//                context.Response.StatusCode = 403;
//                context.Response.Redirect("/Error/Forbidden/");
//                return Task.CompletedTask;
//            },
//            OnRedirectToLogin = context =>
//            {
//                context.Response.StatusCode = 401;
//                context.Response.Redirect("/Home/Login/");
//                return Task.CompletedTask;
//            },
//            OnRedirectToLogout = context =>
//            {
//                context.Response.Redirect("/Account/Logout/");
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