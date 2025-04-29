using Autofac.Extensions.DependencyInjection;
using EdoContactsUpdater.Configs;
using EdoContactsUpdater.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using TaxcomEdo.Client;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace EdoContactsUpdater
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
				.ConfigureLogging((context, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(context.Configuration.GetSection(nameof(NLog)));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMappingAssemblies(
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
						.AddTrackedUoW()
						.AddInfrastructure(ServiceLifetime.Singleton)
						.AddStaticHistoryTracker()
						.AddStaticScopeForEntity()
						
						.Configure<TaxcomContactsUpdaterOptions>(hostContext.Configuration.GetSection(TaxcomContactsUpdaterOptions.Path))

						.AddSingleton<IEdoContactStateCodeConverter, EdoContactStateCodeConverter>()
						.AddHttpClient()
						.AddTaxcomClient()
						.ConfigureZabbixSenderFromDataBase(nameof(TaxcomEdoContactsUpdaterService));
					
					services.AddHostedService<TaxcomEdoContactsUpdaterService>();
				});
	}
}
