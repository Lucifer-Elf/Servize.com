using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Session;
using Servize.Authentication;
using Servize.Controllers;
using Servize.Domain.Mapper;
using Servize.Domain.Model.OrderDetail;
using Servize.Domain.Repositories;
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Servize.Utility.Cors;
using Servize.Utility;
using Servize.Utility.Configurations;

namespace Servize
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myWebOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // controller Registered
            services.AddCors(options =>
             {
                 options.AddPolicy(name: MyAllowSpecificOrigins,
                                   builder =>
                                   {
                                       builder.AllowAnyOrigin()
                                       .AllowAnyHeader()
                                       .AllowAnyMethod();

                                   });
             });


            services.AddScoped<ProviderRespository>();
            services.AddScoped<CategoryRepository>();
            services.AddScoped<ClientRepository>();
            services.AddScoped<CartController>();
            services.AddScoped<Utility.Utilities>();
            services.AddScoped<Cart>();
            services.AddScoped<ContextTransaction>();

            /*string connectionString = @$"Server={AzureVault.GetValue("DbServer")};
                                        Database={AzureVault.GetValue("DatabaseName")};
                                        User Id ={AzureVault.GetValue("DbUserId")};
                                        Password={AzureVault.GetValue("DbPassword")};
                                        MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";*/

          string connectionString = "Server=servizetest.database.windows.net;" +
                                        "Database=serviceTestDb;" +
                                       " User Id =servizeAdmin;" +
                                        "Password=@Lfred1205;" +
                                        "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            //EnitiyFrameWork
            services.AddDbContext<ServizeDBContext>(options => options.UseSqlServer(connectionString));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // instance of http 
            services.AddScoped(sp => Cart.GetCart(sp));  // diffenrt instance to differnt user

            //Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ServizeDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders()
                .AddTokenProvider("ServizeApp", typeof(DataProtectorTokenProvider<ApplicationUser>));


            //Add Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            })

           //Adding Jwt Bearer
           .AddJwtBearer(options =>
           {
               options.SaveToken = true;
               options.RequireHttpsMetadata = false;
               options.TokenValidationParameters = new TokenValidationParameters()
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateIssuerSigningKey = true,
                   ValidAudience = Configuration["JWT:ValidAudience"],
                   ValidIssuer = Configuration["JWT:ValidIssuer"],
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JWT:Secret"])),


               };
           })

     .AddGoogle(options =>
        {
            options.ClientId = AzureVault.GetValue("GoogleClientId");
            options.ClientSecret = AzureVault.GetValue("GoogleSecret");
            // to change call back Url
            //options.CallbackPath
        })
     .AddFacebook(options =>
     {
         options.AppId = Utility.Configurations.Configuration.GetValue<string>("AppId");
         options.AppSecret = Utility.Configurations.Configuration.GetValue<string>("AppSecret");
         // to change call back Url
         //options.CallbackPath
     });
     

            services.AddSession(options =>
            {
                options.Cookie.Name = "otp";
                options.IdleTimeout = TimeSpan.FromSeconds(45);
                options.Cookie.IsEssential = true;
            });

            var config = new AutoMapper.MapperConfiguration(config =>
            {
                config.AddProfile(new MapperSetting());
            });
            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            services.AddDistributedMemoryCache();
            services.AddScoped<IAuthService, AuthService>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.GetService<ServizeDBContext>().Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseCookiePolicy();
            app.UseAuthentication();   // add to pipline 
            app.UseAuthorization();

            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
