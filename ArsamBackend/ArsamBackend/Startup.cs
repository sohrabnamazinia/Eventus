using System;
using ArsamBackend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ArsamBackend.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using ActionFilters.ActionFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ArsamBackend.Services;
using ArsamBackend.Security;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace ArsamBackend
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;
        private IConfiguration _config { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _config = configuration;
            this.env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region CORS
            services.AddCors(o => o.AddPolicy(Constants.CORSPolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            #endregion CORS
            #region SetPasswordComplexity
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.User.AllowedUserNameCharacters = Constants.PasswordAllowedUserNameCharacters;
                if (env.IsDevelopment())
                {
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 3;
                    options.Password.RequireUppercase = false;
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                }
                else
                {
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Password.RequiredLength = 10;
                    options.Password.RequiredUniqueChars = 3;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                }

            });
            #endregion SetPasswordComplexity
            #region Auth Policies
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("TokenSignKey")));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser().Build();
                });
                options.DefaultPolicy = options.GetPolicy(JwtBearerDefaults.AuthenticationScheme);
            }
            );
            #endregion Auth Policies
            #region Services
            services.AddControllers().AddNewtonsoftJson(options => 
            {
              options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IMinIOService, MinIOService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<NotBlocked>();
            services.AddTransient<TokenManagerMiddleware>();

            #endregion Services
            #region Db
            services.AddIdentity<AppUser, IdentityRole>(options => 
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders(); 
            services.AddDbContextPool<AppDbContext>(
            options => options.UseSqlServer(_config.GetConnectionString(Constants.ConnectionStringKey)).UseLazyLoadingProxies());
            services.AddSingleton<DataProtectionPurposeStrings>();

            
            services.AddDistributedRedisCache(options => 
            {
                options.Configuration = _config.GetConnectionString("Redis");
                options.InstanceName = "master";
            });
            #endregion Db
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region Ordered Middlewares

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCheckUserIsPremium();

            app.UseRouting();

            app.UseHttpsRedirection();

            if (!env.IsProduction())
            {
                app.UseCors(Constants.CORSPolicyName);
            }

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<TokenManagerMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(Constants.RouteName, Constants.RoutePattern);
            });
            #endregion Ordered Middleware
        }
    }
}
