using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace TrueMarkWorker
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

					services.AddHttpClient();

					services
						.AddMappingAssemblies(
							typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
							typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
							typeof(QS.Banks.Domain.Bank).Assembly,
							typeof(QS.HistoryLog.HistoryMain).Assembly,
							typeof(QS.Project.Domain.TypeOfEntity).Assembly,
							typeof(QS.Attachments.Domain.Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly
						);

					services
						.AddDatabaseConnection()
						.AddCore()
						.AddInfrastructure()
						.AddTrackedUoW()
						.AddStaticHistoryTracker()
						;

					services
						.AddHostedService<TrueMarkWorker>()
						.ConfigureTrueMarkWorker(hostContext)
						;

					services.ConfigureZabbixSenderFromDataBase(nameof(TrueMarkWorker));

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
				});
	}
}
