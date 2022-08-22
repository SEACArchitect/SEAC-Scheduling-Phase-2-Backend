using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Nri_Webapplication_Backend.Managers;
using Nri_Webapplication_Backend.Helpers;
using Nri_Webapplication_Backend.DTO;

namespace Nri_Webapplication_Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

      
            services.AddTransient<ConnectionHelper>(_ => new ConnectionHelper(Configuration["ConnectionStrings:DefaultConnection"]));

            // services.AddDbContext<UserContext>(options =>
            //    options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IRespondHelper, RespondHelper>();
            services.AddTransient<IAutoSentMailHelper, AutoSentMailHelper>();
            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<IUserRoleManager, UserRoleManager>();
            services.AddTransient<IContentManager, ContentManager>();
            services.AddTransient<ITrainerManager, TrainerManager>();
            services.AddTransient<IRoomManager, RoomManager>();
            services.AddTransient<IPlannerManager, PlannerManager>();
            services.AddTransient<IMyworkManager, MyworkManager>();
            services.AddTransient<IIcsSentMailManager, IcsSentMailManager>();
            services.AddTransient<IMailManager, MailManager>();
            services.AddTransient<IEventManager, EventManager>();
            services.AddTransient<IReportManager, ReportManager>();
            services.AddSingleton<ICommonHelper, CommonHelper>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ICalendarManager, CalendarManager>();


            // var connection = Configuration.GetConnectionString("DefaultConnection");

            //Generate swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nri-Webapplication-Backend", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Token id on header.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
            services.AddCors(options =>
            {
                options.AddPolicy("CORS_Policy", builder => builder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                );
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddJwtBearer(options =>
   {
       options.TokenValidationParameters = new TokenValidationParameters
       {
           ValidateIssuer = true,
           ValidateAudience = true,
           ValidateLifetime = true,
           ValidateIssuerSigningKey = true,
           ValidIssuer = Configuration["JsonWebTokenSetting:Issuer"],
           ValidAudience = Configuration["JsonWebTokenSetting:Issuer"],
           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JsonWebTokenSetting:Key"]))
       };
   });


        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(UI =>
            {
                //UI.SwaggerEndpoint(@"https://scheduling.seasiacenter.com/api/swagger/v1/swagger.json", "Nri-Webapplication-Backend");
                UI.SwaggerEndpoint(@"https://scheduling-dev.seasiacenter.com/api/swagger/v1/swagger.json", "Nri-Webapplication-Backend");
                //UI.SwaggerEndpoint("/swagger/v1/swagger.json", "Nri-Webapplication-Backend");
            });

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CORS_Policy");


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
