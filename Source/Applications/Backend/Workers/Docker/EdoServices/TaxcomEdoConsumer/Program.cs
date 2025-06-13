using Microsoft.Extensions.Hosting;
using System.Text;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using TaxcomEdo.Client;
using TaxcomEdoConsumer.Consumers;
using TaxcomEdoConsumer.Options;
using Vodovoz;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate;
using Vodovoz.Infrastructure.Persistance;

namespace TaxcomEdoConsumer
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
				.ConfigureLogging((context, builder) =>
				{
					builder.AddNLog();
					builder.AddConfiguration(context.Configuration.GetSection(nameof(NLog)));
				})
				.ConfigureServices((hostContext, services) =>
				{
					Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
					var configuration = hostContext.Configuration;

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
						.AddBusiness(configuration)
						.AddTaxcomEdoConsumerDependenciesGroup()
						.Configure<TaxcomEdoConsumerOptions>(configuration.GetSection(TaxcomEdoConsumerOptions.Path))
						.AddInfrastructure()
						.AddHttpClient()
						.AddTaxcomClient()

						.AddMessageTransportSettings()
						.AddMassTransit(busConf =>
						{
							busConf.AddConsumer<
								AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer,
								AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumerDefinition>();

							busConf.AddConsumer<
								OutgoingTaxcomDocflowUpdatedEventConsumer,
								OutgoingTaxcomDocflowUpdatedEventConsumerDefinition>();

							busConf.AddConsumer<
								AcceptingWaitingForCancellationDocflowEventConsumer,
								AcceptingWaitingForCancellationDocflowEventConsumerDefinition>();

							busConf.AddConsumer<
								TaxcomDocflowRequestCancellationEventConsumer,
								TaxcomDocflowRequestCancellationEventConsumerDefinition>();

							busConf.AddConsumer<TaxcomDocflowSendEventConsumer, TaxcomDocflowSendEventConsumerDefinition>();

							busConf.ConfigureRabbitMq();
						});

						services.AddStaticScopeForEntity();
				});
	}
}
