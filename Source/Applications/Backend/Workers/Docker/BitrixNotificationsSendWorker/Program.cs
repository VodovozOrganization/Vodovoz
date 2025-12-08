using Autofac.Extensions.DependencyInjection;
using BitrixNotificationsSend.Library;
using BitrixNotificationsSendWorker.CashlessDebts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace BitrixNotificationsSendWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((ctx, builder) =>
				{
					builder.AddNLog();
					builder.AddConfiguration(ctx.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly)
						.AddDatabaseConnection()
						.AddCore()
						.AddRepositories()
						.AddTrackedUoW()
						.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
						.AddBitrixNotificationsSendServices()
						.ConfigureZabbixSenderFromDataBase(nameof(CashlessDebtsNotificationsSendWorker));

					services
						.AddDatabaseConfigurationExposer(config =>
						{
							config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>();
						});

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

					services
						.AddOpenTelemetry()
						.ConfigureResource(resource => resource.AddService("bitrix-notifications-send.worker"))
						.WithTracing(tracing =>
						{
							tracing
								.AddHttpClientInstrumentation();

							tracing.AddOtlpExporter();
						});

					services.AddHostedService<CashlessDebtsNotificationsSendWorker>();
				});
	}
}
