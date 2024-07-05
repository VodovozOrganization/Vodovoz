﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.HistoryChanges;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
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
							typeof(EmployeeWithLoginMap).Assembly
						)
						.AddDatabaseConnection()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW()
						.AddHostedService<MonitoringArchivingWorker>()
						.AddHostedService<ClearFastDeliveryAvailabilityHistoryWorker>()
						.AddHostedService<PowerBiExportWorker>()
						.AddHostedService<TechInspectWorker>()
						.AddHostedService<FuelTransactionsControlWorker>()
						.ConfigureClearFastDeliveryAvailabilityHistoryWorker(hostContext)
						.ConfigurePowerBiExportWorker(hostContext)
						.ConfigureTextInspectWorker(hostContext)
						.AddFuelTransactionsControlWorker(hostContext)												
						.ConfigureZabbixSender(nameof(TechInspectWorker))
						.ConfigureZabbixSender(nameof(PowerBiExportWorker))
						.ConfigureZabbixSender(nameof(ClearFastDeliveryAvailabilityHistoryWorker))
						.ConfigureZabbixSender(nameof(FuelTransactionsControlWorker))
						.ConfigureZabbixSender(nameof(MonitoringArchivingWorker))
						;

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
				});
	}
}
