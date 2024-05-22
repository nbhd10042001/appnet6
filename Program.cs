using System.Net;
using App.Areas.Product.Models.Services;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

#nullable disable

namespace App;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //...
        // builder.WebHost.ConfigureKestrel(serverOptions =>
        // {
        //     // serverOptions.ListenAnyIP(8090); // to listen for incoming http connection on port 5001
        //     // serverOptions.ListenAnyIP(7001, listenOptions => listenOptions.UseHttps()); // to listen for incoming https connection on port 7001
        //     // serverOptions.ListenLocalhost(8090);
        //     // serverOptions.ListenLocalhost(7001, opts => opts.UseHttps());
        // });
        //...

        builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:5000");

        // cau hinh connect string trong appsettings.json
        var connectString = builder.Configuration.GetConnectionString("AppMvcConnectionString");
        builder.Services.AddDbContext<AppDbContext>(options => {
            options.UseSqlServer(connectString);
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        //send mail
        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSetting"));
        builder.Services.AddSingleton<IEmailSender, SendMailService>();

        //dang ky Identity
        builder.Services.AddIdentity<AppUser, IdentityRole>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

        // Truy cập IdentityOptions
        builder.Services.Configure<IdentityOptions> (options => {
            // Thiết lập về Password
            options.Password.RequireDigit = false; // Không bắt phải có số
            options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
            options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
            options.Password.RequireUppercase = false; // Không bắt buộc chữ in
            options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
            options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

            // Cấu hình Lockout - khóa user
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); // Khóa 5 phút
            options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
            options.Lockout.AllowedForNewUsers = true;

            // Cấu hình về User.
            options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;  // Email là duy nhất

            // Cấu hình đăng nhập.
            options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
            options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
            options.SignIn.RequireConfirmedAccount = true; // phai xac thuc tai khoan, chi tiet Register.cshtml.cs
        });

        // cai dat cac redirect page login, logout khi truy cap vao cac page co [Authorize]
        builder.Services.ConfigureApplicationCookie(options => {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/khongduoctruycap.html";
        });

        // add authentication google, facebook,...
        builder.Services.AddAuthentication().AddGoogle(googleOptions => {
            var clientId = builder.Configuration["Authentication:Google:ClientId"];
            var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

            if (clientId != null && clientSecret != null)
            {
                googleOptions.ClientId = clientId;
                googleOptions.ClientSecret = clientSecret;
            }
            // default uri http://localhost:5146/signin-google
            googleOptions.CallbackPath = "/login-from-google";
        
        });

        builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

        builder.Services.AddAuthorization(options => {
            options.AddPolicy("ViewManageMenu", builder => {
                builder.RequireAuthenticatedUser(); // yeu cau user login
                builder.RequireRole(RoleName.Administrator);
            });
        });

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(cfg => {
            cfg.Cookie.Name = "appMVC";
            cfg.IdleTimeout = new TimeSpan(0, 30, 0); // thoi gian ton tai session (30p)
        });

        builder.Services.AddTransient<CartService>();
        
        builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
        builder.Services.AddTransient<SidebarAdminService>();
        
        

        // App Build ---------------------------------------------------------------------------------------------
        //////////////////////////////////////////////////////////////////////////////////////////////////////////

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseStaticFiles(new StaticFileOptions(){
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
            ),
            // url: /contents/image.jpg => Uploads/image.jpg
            RequestPath = "/contents"
        });

        app.UseSession();

        app.AddStatusCodePage(); // tuy bien response khi co loi~ tu 400 -> 599

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "first",
            pattern: "{url:regex(^((xemsanpham)|(viewproduct))$)}/{id:range(1,4)}",
            defaults: new {
                controller = "Product",
                action = "ViewProduct"
            }
        );

        app.MapAreaControllerRoute(
            name: "product",
            pattern: "/{controller}/{action=Index}/{id?}",
            areaName: "ProductManage"
        );

        // thuc hien khi cac controller khong co Area
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();


        app.Run();
    }
}
