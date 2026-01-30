using Autofac;
using Autofac.Extensions.DependencyInjection;
using DatabaseServiceWorker.ExportTo1c;
using DatabaseServiceWorker.PowerBiWorker;
using DatabaseServiceWorker.PowerBiWorker.Exporters;
using DatabaseServiceWorker.PowerBiWorker.Factories;
using ExportTo1c.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models;
using Vodovoz.Settings.Database.Delivery;
using Vodovoz.Tools;
using Vodovoz.Zabbix.Sender;

namespace DatabaseServiceWorker
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
				.ConfigureContainer<ContainerBuilder>(builder =>
				{
					builder.RegisterType<DataArchiver>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<FastDeliveryAvailabilityHistoryModel>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<FastDeliveryAvailabilityHistorySettings>().AsImplementedInterfaces().SingleInstance();
				})
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
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW()

						.AddHostedService<MonitoringArchivingWorker>()
						.ConfigureZabbixSenderFromDataBase(nameof(MonitoringArchivingWorker))
						
						.AddHostedService<ClearFastDeliveryAvailabilityHistoryWorker>()
						.ConfigureZabbixSenderFromDataBase(nameof(ClearFastDeliveryAvailabilityHistoryWorker))
						.ConfigureClearFastDeliveryAvailabilityHistoryWorker(hostContext)
						
						.AddHostedService<TechInspectWorker>()
						.ConfigureTechInspectWorker(hostContext)
						.ConfigureZabbixSenderFromDataBase(nameof(TechInspectWorker))
						
						.AddHostedService<FuelTransactionsControlWorker>()
						.AddFuelTransactionsControlWorker(hostContext)
						.ConfigureZabbixSenderFromDataBase(nameof(FuelTransactionsControlWorker))

						.AddHostedService<ExportTo1cWorker>()
						.AddExportTo1c()
						.ConfigureExportTo1cWorker(hostContext)
						.ConfigureZabbixSenderFromDataBase(nameof(ExportTo1cWorker))

						.AddHostedService<ExportTo1cApiWorker>()
						.AddExportTo1cApi()
						.ConfigureExportTo1cApiWorker(hostContext)
						.ConfigureZabbixSenderFromDataBase(nameof(ExportTo1cApiWorker))						
																	

						// Пока отключаем воркер экпорта в PowerBi
						// .AddHostedService<PowerBiExportWorker>()
						// .ConfigurePowerBiExportWorker(hostContext)
						// .ConfigureZabbixSenderFromDataBase(nameof(PowerBiExportWorker))
						// .AddScoped<IPowerBiConnectionFactory, PowerBiConnectionFactory>()
						// .AddScoped<IPowerBiExporter, PowerBiExporter>()
						;

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
				});
	}
}
