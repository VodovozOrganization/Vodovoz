using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForPaymentEdoDocumentConsumer :
		BillWithoutShipmentEdoDocumentConsumer,
		IConsumer<InfoForCreatingBillWithoutShipmentForPaymentEdo>
	{
		public BillWithoutShipmentForPaymentEdoDocumentConsumer(
			ILogger<BillWithoutShipmentForPaymentEdoDocumentConsumer> logger,
			IServiceScopeFactory scopeFactory)
			: base(scopeFactory, logger)
		{
		}

		public async Task Consume(ConsumeContext<InfoForCreatingBillWithoutShipmentForPaymentEdo> context)
		{
			var message = context.Message as InfoForCreatingBillWithoutShipmentEdo;
			Logger.LogInformation(
				"Отправляем информацию по счету на постоплату {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderWithoutShipmentInfo.Id);

			await SendDataToTaxcomApi(message);
		}
	}
}
