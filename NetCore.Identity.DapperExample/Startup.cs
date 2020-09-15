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

            //Dapper.Identity ayarlar�
            services.ConfigureDapperConnectionProvider<SqlServerConnectionProvider>(Configuration.GetSection("DapperIdentity")) //Identity connection ayar�.
                    .ConfigureDapperIdentityCryptography(Configuration.GetSection("DapperIdentityCryptography")) //Identity kriptografi ayar�.
                    .ConfigureDapperIdentityOptions(new DapperIdentityOptions { UseTransactionalBehavior = false }); //Transaction i�lemlerini t�m i�lemlerde kullanmak i�in true yap.

            services.AddIdentity<AppUser, AppRole>(x =>
            {
                //Kullan�c� ad�nda ge�erli olan karakterleri belirtiyoruz.
                x.User.AllowedUserNameCharacters = "abc�defghi�jklmno�pqrs�tu�vwxyzABC�DEFGHI�JKLMNO�PQRS�TU�VWXYZ0123456789-._@+";
                x.Password.RequireDigit = false; //0-9 aras� say�sal karakter zorunlulu�unu kald�r�yoruz.
                x.Password.RequiredLength = 1; //En az ka� karakterli olmas� gerekti�ini belirtiyoruz.
                x.Password.RequireLowercase = false; //K���k harf zorunlulu�unu kald�r�yoruz.
                x.Password.RequireNonAlphanumeric = false; //Alfanumerik zorunlulu�unu kald�r�yoruz.
                x.Password.RequireUppercase = false; //B�y�k harf zorunlulu�unu kald�r�yoruz.
            })
                    .AddDapperIdentityFor<SqlServerConfiguration>()
                    .AddDefaultTokenProviders();

            //Kullan�c�'y� silindikten sonra ilgili cookie'yi siliyoruz.
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                o.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.AccessDeniedPath = new PathString("/yetkisiz-sayfa"); //Yetkisiz kullan�c�'n�n g�rece�i sayfa.
                opt.LoginPath = new PathString("/Giris-Yap"); //Login giri� sayfas�.
                opt.Cookie.Name = "AspNetCoreIdentity"; //Olu�turulacak Cookie'yi isimlendiriyoruz.
                opt.Cookie.HttpOnly = true; //K�t� niyetli insanlar�n client-side taraf�ndan Cookie'ye eri�mesini engelliyoruz.
                opt.Cookie.SameSite = SameSiteMode.Strict; //D�� kaynaklar�n Cookie'yi kullanmas�n� engelliyoruz.
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(30); //CookieBuilder nesnesinde tan�mlanan Expiration de�erinin varsay�lan de�erlerle ezilme ihtimaline kar��n tekrardan Cookie vadesi burada da belirtiliyor.
                opt.SlidingExpiration = true; //Expiration s�resinin yar�s� kadar s�re zarf�nda istekte bulunulursa e�er geri kalan yar�s�n� tekrar s�f�rlayarak ilk ayarlanan s�reyi tazeleyecektir.
            });

            //Singleton yap�da SqlConnection nesnesini register ediyoruz.
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
