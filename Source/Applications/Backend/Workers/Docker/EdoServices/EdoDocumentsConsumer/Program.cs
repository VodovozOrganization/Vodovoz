using EdoDocumentsConsumer.Consumers;
using MassTransit;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Library;

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
				.ConfigureLogging((context, builder) => {
					builder.AddNLog();
					builder.AddConfiguration(context.Configuration.GetSection(nameof(NLog)));
				})
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddHttpClient()
						.AddTaxcomClient()

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
