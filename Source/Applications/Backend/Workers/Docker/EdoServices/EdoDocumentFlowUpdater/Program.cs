using Autofac.Extensions.DependencyInjection;
using Edo.Docflow.Taxcom;
using EdoDocumentFlowUpdater.Configs;
using EdoDocumentFlowUpdater.Options;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.HistoryLog;
using QS.Project.Core;
using TaxcomEdo.Client;
using TaxcomEdo.Library;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.FileStorage;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;

namespace EdoDocumentFlowUpdater
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
						.AddTrackedUoW()
						.AddInfrastructure(ServiceLifetime.Singleton)
						.ConfigureOptions<ConfigureS3Options>()
						.AddFileStorage()
						.AddStaticHistoryTracker()
						.AddStaticScopeForEntity()
						.Configure<TaxcomEdoDocumentFlowUpdaterOptions>(
							hostContext.Configuration.GetSection(TaxcomEdoDocumentFlowUpdaterOptions.Path))
						.AddHttpClient()
						.AddTaxcomClient()
						
						.ConfigureZabbixSenderFromDataBase(nameof(TaxcomEdoDocumentFlowUpdater))
						.AddHostedService<TaxcomEdoDocumentFlowUpdater>()
						
						.AddMessageTransportSettings()
						.AddMassTransit(busConf =>
						{
							busConf.ConfigureRabbitMq();
						});

				});
	}
}
