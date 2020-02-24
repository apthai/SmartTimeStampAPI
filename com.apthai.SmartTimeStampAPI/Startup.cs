using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using com.apthai.SmartTimeStampAPI.Extention;
using com.apthai.SmartTimeStampAPI.Interfaces;
using com.apthai.SmartTimeStampAPI.Repositories;
using com.apthai.SmartTimeStampAPI.Services;

namespace com.apthai.SmartTimeStampAPI
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

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSwaggerGen(Options =>
            {
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
                Options.DescribeAllEnumsAsStrings();
                // กำหนด รายระเอียด Document Swagger
                Options.SwaggerDoc("V1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "AP (Thailand) Smart Time Stamp API ",
                    Version = "V1",
                    Description = "API for Smart Time Stamp system",
                    TermsOfService = "Term Of Services"
                });
                Options.EnableAnnotations();
            });
            services.AddSingleton<IMobileRepository, MobileRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // ประกาศตัวแปล Globel ที่ดึงจาก Appsetting.Json( Web.config )
            SettingServiceProvider.VirtualDirectory = Environment.GetEnvironmentVariable("VirtualDirectory");
            SettingServiceProvider.SMSApiUrl = Environment.GetEnvironmentVariable("SMSApiUrl");
            SettingServiceProvider.API_Key = Environment.GetEnvironmentVariable("API_Key");
            SettingServiceProvider.API_Token = Environment.GetEnvironmentVariable("API_Token");

            if(Environment.GetEnvironmentVariable("DevEnvironment", EnvironmentVariableTarget.User)!=null 
                && Environment.GetEnvironmentVariable("DevEnvironment", EnvironmentVariableTarget.User).ToLower()=="yes")
            {
                SettingServiceProvider.VirtualDirectory = Configuration.GetSection("AppSettings:VirtualDirectory").Value;
                SettingServiceProvider.SMSApiUrl = Configuration.GetSection("AppSettings:SMSApiUrl").Value;
                SettingServiceProvider.API_Key = Configuration.GetSection("AppSettings:API_Key").Value;
                SettingServiceProvider.API_Token = Configuration.GetSection("AppSettings:API_Token").Value;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger().UseSwaggerUI(c =>
            {
                //c.SwaggerEndpoint(Configuration["AppSettings:VirtualDirectory"] + "/swagger/V1/swagger.json", "AP (Thailand) Siteservice Mobile API");
                c.SwaggerEndpoint(SettingServiceProvider.VirtualDirectory + "/swagger/V1/swagger.json", "AP (Thailand) Smart Time Stamp API");
                c.RoutePrefix = "swagger";
            });

            // ในกรณีที่จะใช้ Header Key ที่ Verify บน Http Header
            //List<string> APIKey = Configuration.GetSection("AppSettings:APIKey").Value.Split(",").ToList();
            //List<string> APIHeader = Configuration.GetSection("AppSettings:APIHeader").Value.Split(",").ToList();

            //for (int i = 0; i < APIHeader.Count(); i++)
            //{
            //    KeyAndTokenObject obj = new KeyAndTokenObject();
            //    obj.ApiHeader = APIHeader[i].ToString();
            //    obj.ApiKey = APIKey[i].ToString();
            //    KeyAndTokenExtention.KeyAndTokenProvider.Add(obj);
            //}

            
        }
    }
}
