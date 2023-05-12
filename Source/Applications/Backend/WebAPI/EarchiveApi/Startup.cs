using EarchiveApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

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

			var connectionStrBuilder = new MySqlConnectionStringBuilder();

			var domainDbConfig = Configuration.GetSection("DomainDB");

			connectionStrBuilder.Server = domainDbConfig.GetValue<string>("Server");
			connectionStrBuilder.Port = domainDbConfig.GetValue<uint>("Port");
			connectionStrBuilder.Database = domainDbConfig.GetValue<string>("Database");
			connectionStrBuilder.UserID = domainDbConfig.GetValue<string>("UserID");
			connectionStrBuilder.Password = domainDbConfig.GetValue<string>("Password");
			connectionStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = connectionStrBuilder.GetConnectionString(true);

			services.AddGrpc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<EarchiveUpdService>();

				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
				});
			});
		}
	}
}
