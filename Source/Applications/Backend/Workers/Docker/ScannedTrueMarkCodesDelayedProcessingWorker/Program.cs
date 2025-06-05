using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using QS.Project.Core;
using ScannedTrueMarkCodesDelayedProcessing.Library;
using ScannedTrueMarkCodesDelayedProcessing.Library.Option;
using System;
using System.Text;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace ScannedTrueMarkCodesDelayedProcessingWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
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
						.Configure<ScannedCodesDelayedProcessingOptions>(
							hostContext.Configuration.GetSection(nameof(ScannedCodesDelayedProcessingOptions)));

					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
						)
						.AddDatabaseConnection()
						.AddNHibernateConventions()
						.AddCore()
						.AddApplicationServices()
						.AddRepositories()
						.AddTrackedUoW()
						.AddScannedTrueMarkCodesDelayedProcessing()
						.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
						.ConfigureZabbixSenderFromDataBase(nameof(ScannedCodesDelayedProcessingWorker));

					services
						.AddOpenTelemetry()
						.ConfigureResource(resource => resource.AddService("scanned-true-mark-codes-delayed-processing.worker"))
						.WithTracing(tracing =>
						{
							tracing
								.AddHttpClientInstrumentation();

							tracing.AddOtlpExporter();
						});

					services.AddHostedService<ScannedCodesDelayedProcessingWorker>();
				});
	}
}
