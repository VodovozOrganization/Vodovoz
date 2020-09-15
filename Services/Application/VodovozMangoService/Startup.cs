using System.IO;
using LettuceEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VodovozMangoService
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

            app.UseHttpsRedirection();
            #if DEBUG
            app.UseMiddleware<PerformanceMiddleware>();
            #endif
            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
