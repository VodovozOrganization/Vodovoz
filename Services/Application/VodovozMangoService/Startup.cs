using System.IO;
using LettuceEncrypt;
using MangoService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using VodovozMangoService.HostedServices;

namespace VodovozMangoService
{
	public class Startup
    {
        private readonly VodovozMangoConfiguration vodovozConfiguration;

        public Startup(IConfiguration configuration, VodovozMangoConfiguration vodovozConfiguration)
        {
            this.vodovozConfiguration = vodovozConfiguration;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(x =>
                new MySqlConnection(vodovozConfiguration.ConnectionStringBuilder.GetConnectionString(true)));
            services.AddSingleton(x =>
                new MangoController(vodovozConfiguration.VpbxApiKey, vodovozConfiguration.VpbxApiSalt));
            services.AddSingleton<NotificationHostedService>();
            services.AddHostedService<NotificationHostedService>(provider => provider.GetService<NotificationHostedService>());

            services.AddSingleton<CallsHostedService>();
            services.AddHostedService<CallsHostedService>(provider => provider.GetService<CallsHostedService>());
            
            services.AddControllers();

            services.AddLettuceEncrypt(options =>
                {
                    options.DomainNames = new []{"mango.vod.qsolution.ru"};
                    options.AcceptTermsOfService = true;
                    options.EmailAddress = "fix@qsolution.ru";
                })
                .PersistDataToDirectory(new DirectoryInfo("/var/lib/letsencrypt"), "vodovoz");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();
            #if DEBUG
            app.UseMiddleware<PerformanceMiddleware>();
            #endif
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
