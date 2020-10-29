using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApsCache.ConfigOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ApsCache
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApsSettings>(_configuration.GetSection(ApsSettings.ApsConfigSectionName));

            //needed to avoid an issue https://github.com/dotnet/aspnetcore/issues/8302
            // mostly to translate the body of the inbound request to the body of the upstream request
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseApsCacheModule();

            app.MapWhen(
                context => true,// have this handle all request
                appBranch => {
                    appBranch.UseApsCacheHandler();
                }
            );
        }
    }
}
