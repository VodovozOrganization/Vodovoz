using CustomerOrdersApi.Library;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using QS.Services;

namespace CustomerOrdersApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);
		
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerOrdersApi", Version = "v1" }); });

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				})
				
				.AddMessageTransportSettings()
				.AddMassTransit(busConf => busConf.ConfigureRabbitMq());

				/*configurator.ReceiveEndpoint("online-orders", x =>
				{
					x.ConfigureConsumeTopology = false;

					x.Bind<OnlineOrderInfoDto>(s =>
					{
						s.RoutingKey = "False";
						s.ExchangeType = ExchangeType.Direct;
					});
				});

				configurator.ReceiveEndpoint("online-orders-fault", x =>
				{
					x.ConfigureConsumeTopology = false;

					x.Bind<OnlineOrderInfoDto>(s =>
					{
						s.RoutingKey = "True";
						s.ExchangeType = ExchangeType.Direct;
					});
				});*/
				
			services.AddHttpClient();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.ApplicationServices.GetService<IUserService>();
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerOrdersApi v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}
