using Mango.Api.DependencyInjection;
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
using NLog.Web;
using System;

namespace Mango.Service
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
		public void ConfigureServices(IServiceCollection services)
		{
			var nlogConfig = Configuration.GetSection(_nLogSectionName);
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(nlogConfig);
				});

			var dbSection = Configuration.GetSection("DomainDB");
			if(!dbSection.Exists())
			{
				throw new ArgumentException("Не найдена секция DomainDB в конфигурации");
			}
			var connectionStringBuilder = new MySqlConnectionStringBuilder();
			connectionStringBuilder.Server = dbSection["Server"];
			connectionStringBuilder.Port = uint.Parse(dbSection["Port"]);
			connectionStringBuilder.Database = dbSection["Database"];
			connectionStringBuilder.UserID = dbSection["UserID"];
			connectionStringBuilder.Password = dbSection["Password"];
			connectionStringBuilder.SslMode = MySqlSslMode.None;
			connectionStringBuilder.DefaultCommandTimeout = 5;

			services.AddSingleton(x =>
				new MySqlConnection(connectionStringBuilder.ConnectionString));

			services.AddSingleton(x =>
				new MangoController(Configuration["Mango:VpbxApiKey"], Configuration["Mango:VpbxApiSalt"]));

			services.AddSingleton<CallsHostedService>();
			services.AddHostedService(provider => provider.GetService<CallsHostedService>());

			services.AddSingleton<PhonebookHostedService>();
			services.AddHostedService(provider => provider.GetService<PhonebookHostedService>());

			services.AddSingleton<NotificationHostedService>();
			services.AddHostedService(provider => provider.GetService<NotificationHostedService>());

			services.AddSingleton<ICallerService, CallerService>();
			services.AddScoped<ICallEventHandler, MangoHandler>();

			var messageTransportSettings = new MessageTransportSettings(Configuration);
			services.AddSingleton<IMessageTransportSettings>(messageTransportSettings);

			services.ConfigureMangoServices();
			services.AddCallsPublishing(messageTransportSettings);
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
