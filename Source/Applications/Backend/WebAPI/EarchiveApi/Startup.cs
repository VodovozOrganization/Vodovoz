using EarchiveApi.HealthChecks;
using EarchiveApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using VodovozHealthCheck;

namespace EarchiveApi
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
			services.AddDatabaseConnectionSettings();
			services.AddDatabaseConnectionString();
			services.AddSingleton(provider =>
			{
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				return new MySqlConnection(connectionStringBuilder.ConnectionString);
			});

			services.AddGrpc();

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services.ConfigureHealthCheckService<EarchiveApiHealthCheck, ServiceInfoProvider>();

			services
				.AddCore()
				.AddSpatialSqlConfiguration()
				.AddNHibernateConfiguration()
				.AddNotTrackedUoW()
				;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseGrpcWeb();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<EarchiveUpdService>().EnableGrpcWeb();

				endpoints.MapGet("/", async context =>
					await context.Response.WriteAsync("Use GRPC clietn for connection"));
			});

			app.UseVodovozHealthCheck();
		}
	}
}
