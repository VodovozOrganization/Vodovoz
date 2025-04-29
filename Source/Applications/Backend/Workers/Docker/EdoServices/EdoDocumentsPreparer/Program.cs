using System.Text;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using TaxcomEdo.Library;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Zabbix.Sender;
using VodovozHealthCheck;
using DriverApi.Notifications.Client;

namespace EdoDocumentsPreparer
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
					Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
					
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
						.AddBusiness(hostContext.Configuration)
						.AddDriverApiNotificationsSenders()
						.AddInfrastructure()
						.AddHostedService<EdoDocumentsPreparerWorker>()
						.AddPreparerDependencyGroup()
						.ConfigurePreparer(hostContext.Configuration)

						.AddMessageTransportSettings()
						.AddMassTransit(busConf => busConf.ConfigureRabbitMq())
						.AddStaticScopeForEntity()
						.ConfigureZabbixSenderFromDataBase(nameof(EdoDocumentsPreparerWorker));
						;
				});
	}
}
