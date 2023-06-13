using _06_MvcWeb.Products.Services;
using _06_MvcWeb.Data;
using _06_MvcWeb.ExtendMethods;
using _06_MvcWeb.Models;
using _06_MvcWeb.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace _06_MvcWeb
{
	public class Program
    {
        
        public static string ContentRootPath;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://0.0.0.0:8090");
            ContentRootPath = builder.Environment.ContentRootPath;
            
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.Configure<RazorViewEngineOptions>(options => {
                options.ViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);
            });
            builder.Services.AddSingleton<PlanetService>();
			//builder.Services.AddSingleton<ProductService>();
			//builder.Services.AddSingleton<ProductService,ProductService>();
			//builder.Services.AddSingleton(typeof(ProductService));
			//builder.Services.AddSingleton(typeof(ProductService), typeof(ProductService));
			builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AppMvcConnectionString")));
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddOptions();
            var mailsetting = builder.Configuration.GetSection("MailSettings");
            builder.Services.Configure<MailSettings>(mailsetting);
            builder.Services.AddSingleton<IEmailSender, SendMailService>();
            builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Thiết lập về Password
                options.Password.RequireDigit = false; // Không bắt phải có số
                options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
                options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
                options.Password.RequireUppercase = false; // Không bắt buộc chữ in
                options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
                options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất

                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = true;
            });
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";                                                                      //Trang login phải:   @page"/login"
                options.LogoutPath = "/logout";                                                                        //Trang logout phải @page "/logout"
                options.AccessDeniedPath = "/khongduoctruycap.html";
            });
            builder.Services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        var gconfig = builder.Configuration.GetSection("Authentication:Google");
                        options.ClientId = gconfig["ClientId"];
                        options.ClientSecret = gconfig["ClientSecret"];
                        options.CallbackPath = "/dang-nhap-tu-google";
                    });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("viewManageMenu",builder =>
                {
                    builder.RequireAuthenticatedUser();
                    builder.RequireRole(RoleName.Administrator);
                });
            });

			builder.Services.AddDistributedMemoryCache();           // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
			builder.Services.AddSession(cfg => {                    // Đăng ký dịch vụ Session
				cfg.Cookie.Name = "razorweb";             // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
				cfg.IdleTimeout = new TimeSpan(0, 30, 0);    // Thời gian tồn tại của Session
			});
            builder.Services.AddTransient<CartService>();
            builder.Services.AddTransient<AdminSidebarService>();









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

            app.UseStaticFiles(new StaticFileOptions()
                                {
                                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"Uploads")),
                                    RequestPath="/contents"
                                }
            );

            app.UseSession();

            app.AddStatusCodePages();





            app.UseRouting();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always
            });
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapControllerRoute(
                name: "first",
                pattern: "{url:regex(^((xemsanpham)|(viewproduct))$)}/{id:range(2,4)}",
                defaults: new
                {
                    controller = "First",
                    action = "ViewProduct",
                }
                );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}