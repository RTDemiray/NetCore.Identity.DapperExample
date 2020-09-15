using Identity.Dapper;
using Identity.Dapper.Models;
using Identity.Dapper.SqlServer.Connections;
using Identity.Dapper.SqlServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.Identity.DapperExample.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace NetCore.Identity.DapperExample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddAuthentication();
            services.AddAuthorization();

            //Dapper.Identity ayarlarý
            services.ConfigureDapperConnectionProvider<SqlServerConnectionProvider>(Configuration.GetSection("DapperIdentity")) //Identity connection ayarý.
                    .ConfigureDapperIdentityCryptography(Configuration.GetSection("DapperIdentityCryptography")) //Identity kriptografi ayarý.
                    .ConfigureDapperIdentityOptions(new DapperIdentityOptions { UseTransactionalBehavior = false }); //Transaction iþlemlerini tüm iþlemlerde kullanmak için true yap.

            services.AddIdentity<AppUser, AppRole>(x =>
            {
                //Kullanýcý adýnda geçerli olan karakterleri belirtiyoruz.
                x.User.AllowedUserNameCharacters = "abcçdefghiýjklmnoöpqrsþtuüvwxyzABCÇDEFGHIÝJKLMNOÖPQRSÞTUÜVWXYZ0123456789-._@+";
                x.Password.RequireDigit = false; //0-9 arasý sayýsal karakter zorunluluðunu kaldýrýyoruz.
                x.Password.RequiredLength = 1; //En az kaç karakterli olmasý gerektiðini belirtiyoruz.
                x.Password.RequireLowercase = false; //Küçük harf zorunluluðunu kaldýrýyoruz.
                x.Password.RequireNonAlphanumeric = false; //Alfanumerik zorunluluðunu kaldýrýyoruz.
                x.Password.RequireUppercase = false; //Büyük harf zorunluluðunu kaldýrýyoruz.
            })
                    .AddDapperIdentityFor<SqlServerConfiguration>()
                    .AddDefaultTokenProviders();

            //Kullanýcý'yý silindikten sonra ilgili cookie'yi siliyoruz.
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                o.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.AccessDeniedPath = new PathString("/yetkisiz-sayfa"); //Yetkisiz kullanýcý'nýn göreceði sayfa.
                opt.LoginPath = new PathString("/Giris-Yap"); //Login giriþ sayfasý.
                opt.Cookie.Name = "AspNetCoreIdentity"; //Oluþturulacak Cookie'yi isimlendiriyoruz.
                opt.Cookie.HttpOnly = true; //Kötü niyetli insanlarýn client-side tarafýndan Cookie'ye eriþmesini engelliyoruz.
                opt.Cookie.SameSite = SameSiteMode.Strict; //Dýþ kaynaklarýn Cookie'yi kullanmasýný engelliyoruz.
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(30); //CookieBuilder nesnesinde tanýmlanan Expiration deðerinin varsayýlan deðerlerle ezilme ihtimaline karþýn tekrardan Cookie vadesi burada da belirtiliyor.
                opt.SlidingExpiration = true; //Expiration süresinin yarýsý kadar süre zarfýnda istekte bulunulursa eðer geri kalan yarýsýný tekrar sýfýrlayarak ilk ayarlanan süreyi tazeleyecektir.
            });

            //Singleton yapýda SqlConnection nesnesini register ediyoruz.
            services.AddSingleton<IDbConnection>((sp) => new SqlConnection(Configuration["DapperIdentity:ConnectionString"].ToString()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{Controller=Home}/{Action=Index}");
            });
        }
    }
}
