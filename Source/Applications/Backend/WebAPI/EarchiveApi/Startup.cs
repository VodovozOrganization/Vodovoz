using EarchiveApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
using MySql.Data.MySqlClient;

namespace EarchiveApi
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(x =>
				new MySqlConnection(GetCOnnectionString()));

			services.AddGrpc();

			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});
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
		}

		private string GetCOnnectionString()
		{
			var connectionStringBuilder = new MySqlConnectionStringBuilder();
			var domainDbConfig = Configuration.GetSection("DomainDB");
			connectionStringBuilder.Server = domainDbConfig.GetValue<string>("Server");
			connectionStringBuilder.Port = domainDbConfig.GetValue<uint>("Port");
			connectionStringBuilder.Database = domainDbConfig.GetValue<string>("Database");
			connectionStringBuilder.UserID = domainDbConfig.GetValue<string>("UserID");
			connectionStringBuilder.Password = domainDbConfig.GetValue<string>("Password");
			connectionStringBuilder.SslMode = MySqlSslMode.Disabled;
			connectionStringBuilder.DefaultCommandTimeout = domainDbConfig.GetValue<uint>("DefaultTimeout");

			return connectionStringBuilder.GetConnectionString(true);
		}
	}
}
