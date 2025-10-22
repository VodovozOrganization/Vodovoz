using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Osrm;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Services;
using Vodovoz.Settings.Database.Orders;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.Orders;
using Vodovoz.Zabbix.Sender;

namespace FastDeliveryLateWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
					{
						logging.ClearProviders();
						logging.AddNLog();
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
					});

					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
						);
						
					services
						.AddDatabaseConnection()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW()
						.AddStaticHistoryTracker()
						.AddHostedService<FastDeliveryLateWorker>()
						.ConfigureFastDeliveryLateOptions(hostContext)
						.AddSingleton<IOrderSettings, OrderSettings>()
						.AddSingleton<IEmailService, EmailService>()
						.AddScoped<OrderStateKey>()
						.AddOsrm()
						;

					services.ConfigureZabbixSenderFromDataBase(nameof(FastDeliveryLateWorker));

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
				});
	}
}
