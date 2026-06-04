using Autofac.Extensions.DependencyInjection;
using Edo.Common;
using Edo.Transport;
using EdoContactsUpdater.Configs;
using EdoContactsUpdater.Converters;
using EdoContactsUpdater.Worker;
using EdoService.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using MessageTransport;
using QS.Project.Core;
using TaxcomEdo.Client;
using Vodovoz.Core.Application.Problems.Services;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Core.Data.NHibernate.Repositories.Edo;
using Vodovoz.Core.Data.Repositories;
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
						.AddEdo()
						.AddEdoServicesLibrary()
						.AddTrackedUoW()
						.AddMessageTransportSettings()
						.AddEdoMassTransit()

						.AddInfrastructure(ServiceLifetime.Singleton)
						.AddStaticHistoryTracker()
						.AddStaticScopeForEntity()

						.Configure<TaxcomContactsUpdaterOptions>(hostContext.Configuration.GetSection(TaxcomContactsUpdaterOptions.Path))

						.AddSingleton<IEdoContactStateCodeConverter, EdoContactStateCodeConverter>()
						.AddHttpClient()
						.AddTaxcomClient()
						.ConfigureZabbixSenderFromDataBase(nameof(TaxcomEdoContactsUpdaterService))
						.ConfigureZabbixSenderFromDataBase(nameof(OrderContactProblemUpdateWorker))
						.AddScoped<IEdoRepository, EdoRepository>()
						.AddScoped<IOrderContactProblemUpdateService, OrderContactProblemUpdateService>()
						;
					
					services.AddHostedService<TaxcomEdoContactsUpdaterService>();
					services.AddHostedService<OrderContactProblemUpdateWorker>();
				});
	}
}
