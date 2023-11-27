using MailjetEventsDistributorAPI.DataAccess;
using MailjetEventsDistributorAPI.HealthChecks;
using MailjetEventsDistributorAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using RabbitMQ.Infrastructure;
using VodovozHealthCheck;

namespace MailjetEventsDistributorAPI
{
	public class Startup
	{
		private ILogger<Startup> _logger;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
				});

			_logger = new Logger<Startup>(LoggerFactory.Create(logging =>
			logging.AddNLogWeb(NLogBuilder.ConfigureNLog("NLog.config").Configuration)));

			services.AddTransient<RabbitMQConnectionFactory>();
			services.AddTransient<IInstanceData, InstanceData>();

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "MailjetEventMessagesDistributorAPI", Version = "v1" });
			});

			services.ConfigureHealthCheckService<MailjetEventsDistributeHealthCheck>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseRequestResponseLogging();

			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MailjetEventMessagesDistributorAPI v1"));
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.ConfigureHealthCheckApplicationBuilder();
		}
	}
}
