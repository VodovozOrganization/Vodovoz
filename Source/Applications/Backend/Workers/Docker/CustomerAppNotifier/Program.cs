using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using System;
using System.Text.Json;
using CustomerAppNotifier.Options;
using CustomerAppNotifier.Services;
using Microsoft.Extensions.Configuration;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace CustomerAppNotifier
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
					{
						logging.ClearProviders();
						logging.AddNLog();
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
					})

					.ConfigureZabbixSenderFromDataBase(nameof(CustomerAppEventsSender))

					.AddMappingAssemblies(
						typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
						typeof(QS.Banks.Domain.Bank).Assembly,
						typeof(QS.HistoryLog.HistoryMain).Assembly,
						typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						typeof(QS.Attachments.Domain.Attachment).Assembly,
						typeof(EmployeeWithLoginMap).Assembly,
						typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
					)
					.AddDatabaseConnection()
					.AddCore()
					.AddInfrastructure()
					.AddTrackedUoW()

					.AddHostedService<CustomerAppEventsSender>()

					.AddSingleton(provider => new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					})
					.AddHttpClient<INotificationService, Services.NotificationService>(client =>
					{
						client.Timeout = TimeSpan.FromSeconds(15);
					});
					
					services.Configure<LogoutEventSendScheduleOptions>(settings =>
					{
						hostContext.Configuration.GetSection(LogoutEventSendScheduleOptions.Section).Bind(settings);
					});
					
					services.Configure<MobileAppOptions>(settings =>
					{
						hostContext.Configuration.GetSection(MobileAppOptions.Section).Bind(settings);
					});
					
					services.Configure<VodovozWebSiteOptions>(settings =>
					{
						hostContext.Configuration.GetSection(VodovozWebSiteOptions.Section).Bind(settings);
					});

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
					services.AddStaticHistoryTracker();
				});
		}
	}
}
