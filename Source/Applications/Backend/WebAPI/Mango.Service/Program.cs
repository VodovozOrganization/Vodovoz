using Autofac.Extensions.DependencyInjection;
using Mango.CallsPublishing;
using Mango.Client;
using Mango.Core.Settings;
using Mango.Service.Consumers.Definitions;
using Mango.Service.HostedServices;
using Mango.Service.Services;
using MassTransit;
using MessageTransport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.Project.Core;
using System.Reflection;
using System.Security.Authentication;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Settings;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Mango;
using Vodovoz.Settings.Mango;
using Vodovoz.Settings.Pacs;

namespace Mango.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) => {
					builder.AddNLogWeb();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					var configuration = hostContext.Configuration;
					services.AddDatabaseConnection();
					services.AddCore();
					services.AddNotTrackedUoW();

					services.AddSingleton(provider =>
					{
						var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
						return new MySqlConnection(connectionStringBuilder.ConnectionString);
					});

					services.AddSingleton(x => new MangoController(
						x.GetRequiredService<ILogger<MangoController>>(),
						configuration["Mango:VpbxApiKey"],
						configuration["Mango:VpbxApiSalt"])
					);

					services.AddSingleton<ISettingsController, SettingsController>();
					services.AddSingleton<IMangoUserSettngs, MangoUserSettings>();

					services.AddSingleton<CallsHostedService>();
					services.AddHostedService(provider => provider.GetService<CallsHostedService>());

					services.AddSingleton<PhonebookHostedService>();
					services.AddHostedService(provider => provider.GetService<PhonebookHostedService>());

					services.AddSingleton<NotificationHostedService>();
					services.AddHostedService(provider => provider.GetService<NotificationHostedService>());

					services.AddSingleton<ICallerService, CallerService>();

					services.AddScoped<IMangoConfigurationSettings, ConfigurationMangoSettings>();

					services.AddMessageTransportSettings();

					services.AddMassTransit(busCfg =>
					{
						busCfg.AddConsumers(Assembly.GetAssembly(typeof(MangoCallEventConsumerDefinition)));

						busCfg.UsingRabbitMq((context, rabbitCfg) =>
						{
							var ts = context.GetRequiredService<IMessageTransportSettings>();
							rabbitCfg.Host(ts.Host, (ushort)ts.Port, ts.VirtualHost,
								rabbitHostCfg =>
								{
									rabbitHostCfg.Username(ts.Username);
									rabbitHostCfg.Password(ts.Password);
									if(ts.UseSSL)
									{
										rabbitHostCfg.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
									}
								}
							);

							rabbitCfg.AddMangoTopology(context);
							rabbitCfg.ConfigureEndpoints(context);
						});
					});
				});
	}
}
