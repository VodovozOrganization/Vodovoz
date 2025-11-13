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
using System;
using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
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
					services.AddMappingAssemblies(
						typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						typeof(QS.Banks.Domain.Bank).Assembly,
						typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						typeof(QS.HistoryLog.HistoryMain).Assembly,
						typeof(EmployeeWithLoginMap).Assembly,
						typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly);

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
							var messageSettings = context.GetRequiredService<IMessageTransportSettings>();
							rabbitCfg.Host(messageSettings.Host, (ushort)messageSettings.Port, messageSettings.VirtualHost,
								rabbitHostCfg =>
								{
									rabbitHostCfg.Username(messageSettings.Username);
									rabbitHostCfg.Password(messageSettings.Password);

									if(messageSettings.UseSSL)
									{
										rabbitHostCfg.UseSsl(ssl =>
										{
											if(Enum.TryParse<SslPolicyErrors>(messageSettings.AllowSslPolicyErrors, out var allowedPolicyErrors))
											{
												ssl.AllowPolicyErrors(allowedPolicyErrors);
											}

											ssl.Protocol = SslProtocols.Tls12;
										});
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
