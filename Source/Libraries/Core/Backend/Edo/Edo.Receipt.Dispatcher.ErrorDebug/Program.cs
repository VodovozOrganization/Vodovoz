using Autofac.Extensions.DependencyInjection;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Persistance;

namespace Edo.Receipt.Dispatcher.ErrorDebug
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
						.AddCoreDataRepositories()
						.AddCore()
						.AddTrackedUoW()
						.AddMessageTransportSettings()
						.AddEdoReceiptDispatcherErrorDebug()

						.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
					;

					services.AddHostedService<InitDbConnectionOnHostStartedService>();
				});
	}
}
