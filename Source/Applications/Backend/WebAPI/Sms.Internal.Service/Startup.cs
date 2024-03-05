using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Sms.External.SmsRu;
using Sms.Internal.Service.Authentication;

namespace Sms.Internal.Service
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }


		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLog();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services.AddAuthentication()
				.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);
			services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
			services.AddGrpc().Services.AddAuthorization();
			services.Configure<SmsRuConfiguration>(Configuration.GetSection("SmsRu"));
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder.RegisterType<ApiKeyAuthenticationOptions>().AsSelf().AsImplementedInterfaces();
			builder.RegisterType<ApiKeyAuthenticationHandler>().AsSelf().AsImplementedInterfaces();

			builder.RegisterType<SmsRuSendController>().AsSelf().AsImplementedInterfaces();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseGrpcWeb();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<SmsService>().EnableGrpcWeb();
			});
		}
	}
}
