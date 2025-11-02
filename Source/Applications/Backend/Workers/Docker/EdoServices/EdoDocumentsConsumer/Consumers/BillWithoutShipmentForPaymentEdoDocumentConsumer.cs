using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForPaymentEdoDocumentConsumer :
		BillWithoutShipmentEdoDocumentConsumer,
		IConsumer<InfoForCreatingBillWithoutShipmentForPaymentEdo>
	{
		public BillWithoutShipmentForPaymentEdoDocumentConsumer(
			ILogger<BillWithoutShipmentForPaymentEdoDocumentConsumer> logger,
			ITaxcomApiClient taxcomApiClient)
			: base(taxcomApiClient, logger)
		{
		}

		public async Task Consume(ConsumeContext<InfoForCreatingBillWithoutShipmentForPaymentEdo> context)
		{
			var message = context.Message;
			Logger.LogInformation(
				"Отправляем информацию по счету на постоплату {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderWithoutShipmentInfo.Id);

			await SendDataToTaxcomApi(message);
		}
	}
}
