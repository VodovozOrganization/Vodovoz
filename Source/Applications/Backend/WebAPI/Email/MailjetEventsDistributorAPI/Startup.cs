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
using MySqlConnector;
using NLog.Web;
using QS.Project.Core;
using RabbitMQ.Infrastructure;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Settings.Database;
using VodovozHealthCheck;

namespace MailjetEventsDistributorAPI
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
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services.AddMappingAssemblies()
				.AddSingleton<MySqlConnectionStringBuilder>((provider) => {
					var configuration = provider.GetRequiredService<IConfiguration>();
					var connectionString = configuration.GetSection("ConnectionStrings").GetValue<string>("Default");
					var builder = new MySqlConnectionStringBuilder(connectionString);

					return builder;
				})
				.AddSpatialSqlConfiguration()
				.AddNHibernateConfiguration()
				.AddDatabaseInfo()
				.AddDatabaseSingletonSettings()
				.AddCore()
				.AddTrackedUoW();

			services.AddTransient<RabbitMQConnectionFactory>();
			services.AddTransient<IInstanceData, InstanceData>();

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "MailjetEventMessagesDistributorAPI", Version = "v1" });
			});
			
			services.AddHttpClient();

			services.ConfigureHealthCheckService<MailjetEventsDistributeHealthCheck, ServiceInfoProvider>();
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

			app.UseVodovozHealthCheck();
		}
	}
}
