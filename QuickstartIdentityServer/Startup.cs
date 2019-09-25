using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using DnsClient;
using EFCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickstartIdentityServer.Authentcation;
using QuickstartIdentityServer.Entitys;
using QuickstartIdentityServer.Service;

namespace QuickstartIdentityServer
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
            //Configure DbContext
            //services.AddAbpDbContext<UcenterDbContext>(options =>
            //{
            //    DbContextOptionsConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
            //});
            services.AddDbContextPool<UcenterDbContext>(options => options.UseMySql(Configuration.GetConnectionString("Default")));
            services.AddIdentityServer()
                .AddExtensionGrantValidator<SmsAuthCodeValidator>()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResourceResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());
            //.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
            //.AddProfileService<ProfileService>();
            //从配置文件中获取ServiceDiscovery
            services.Configure<ServiceDisvoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            //单例注册ConsulClient
            services.AddSingleton<IDnsQuery>(p =>
            {
                var serviceConfigration = p.GetRequiredService<IOptions<ServiceDisvoveryOptions>>().Value;
                return new LookupClient(serviceConfigration.Consul.DnsEndpoint.ToIPEndPoint());

            });
            services.AddSingleton<HttpClient>(new HttpClient());
            services.AddScoped(typeof(UcenterDbContext));
            services.AddScoped<IAuthService,AuthServce>();
            services.AddScoped<IUserServise,UserService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
           

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseIdentityServer();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
