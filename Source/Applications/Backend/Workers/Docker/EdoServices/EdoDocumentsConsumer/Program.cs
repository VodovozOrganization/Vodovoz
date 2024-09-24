using Autofac.Extensions.DependencyInjection;
using EdoDocumentsConsumer.Consumers;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using TaxcomEdo.Library;
using Vodovoz;
using Vodovoz.Application;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.FileStorage;
using Vodovoz.Infrastructure.Persistance;

namespace EdoDocumentsConsumer
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
						.AddBusiness(hostContext.Configuration)
						.AddInfrastructure()
						.AddApplication()
						.AddFileStorage()

						.AddMessageTransportSettings()
						.AddMassTransit(busConf =>
						{
							busConf.AddConsumer<BillEdoDocumentConsumer, BillEdoDocumentConsumerDefinition>();
							busConf.AddConsumer<UpdEdoDocumentConsumer, UpdEdoDocumentConsumerDefinition>();
							busConf.AddConsumer<BillWithoutShipmentForDebtEdoDocumentConsumer,
								BillWithoutShipmentForDebtEdoDocumentConsumerDefinition>();
							busConf.AddConsumer<BillWithoutShipmentForPaymentEdoDocumentConsumer,
								BillWithoutShipmentForPaymentEdoDocumentConsumerDefinition>();
							busConf.AddConsumer<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer,
								BillWithoutShipmentForAdvancePaymentEdoDocumentConsumerDefinition>();

							busConf.ConfigureRabbitMq();
						});

				});
	}
}
