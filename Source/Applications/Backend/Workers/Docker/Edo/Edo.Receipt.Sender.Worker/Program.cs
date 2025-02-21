using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulKassa;
using NLog.Extensions.Logging;
using System;
using System.Text;

namespace Edo.Receipt.Sender.Worker
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
					services.Configure<CashboxesSetting>(hostContext.Configuration);
					services.AddModulKassa()

						//	.AddMappingAssemblies(
						//		typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
						//		typeof(QS.Banks.Domain.Bank).Assembly,
						//		typeof(QS.HistoryLog.HistoryMain).Assembly,
						//		typeof(QS.Project.Domain.TypeOfEntity).Assembly,
						//		typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder).Assembly,
						//		typeof(QS.BusinessCommon.HMap.MeasurementUnitsMap).Assembly
						//	)
						//	.AddDatabaseConnection()
						//	.AddNHibernateConventions()
						//	.AddCoreDataRepositories()
						//	.AddCore()
						//	.AddTrackedUoW()
						//	.AddMessageTransportSettings()
						//	.AddEdoDocflow()

						//	.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
						;

					services.AddHostedService<WorkerService>();
				});
	}
}
