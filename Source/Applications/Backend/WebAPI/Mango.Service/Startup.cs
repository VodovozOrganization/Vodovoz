using Mango.Api;
using Mango.CallsPublishing;
using Mango.Client;
using Mango.Core.Handlers;
using Mango.Service.Handlers;
using Mango.Service.HostedServices;
using Mango.Service.Services;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NHibernate;
using NLog.Web;
using System;
using Vodovoz.Settings.Database.Pacs;
using Vodovoz.Settings.Pacs;
using QS.Project.Core;

namespace Mango.Service
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

			services.AddDatabaseConnectionSettings();
			services.AddDatabaseConnectionString();
			services.AddSingleton(provider => {
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				return new MySqlConnection(connectionStringBuilder.ConnectionString);
			});

			services.AddSingleton(x =>
				new MySqlConnection(connectionStringBuilder.ConnectionString));

			services.AddSingleton(x => new MangoController(
				_loggerFactory.CreateLogger<MangoController>(), 
				Configuration["Mango:VpbxApiKey"], 
				Configuration["Mango:VpbxApiSalt"])
			);

			services.AddSingleton<CallsHostedService>();
			services.AddHostedService(provider => provider.GetService<CallsHostedService>());

			services.AddSingleton<PhonebookHostedService>();
			services.AddHostedService(provider => provider.GetService<PhonebookHostedService>());

			services.AddSingleton<NotificationHostedService>();
			services.AddHostedService(provider => provider.GetService<NotificationHostedService>());

			services.AddSingleton<ICallerService, CallerService>();
			services.AddScoped<ICallEventHandler, MangoHandler>();

			var messageTransportSettings = new ConfigTransportSettings();
			Configuration.GetSection("MessageTransport").Bind(messageTransportSettings);
			services.AddSingleton<IMessageTransportSettings>(messageTransportSettings);

			services.ConfigureMangoServices();
			services.AddCallsPublishing();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

#if DEBUG
			app.UseMiddleware<PerformanceMiddleware>();
#endif

			app.UseRouting();
			app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
		}
	}
}
