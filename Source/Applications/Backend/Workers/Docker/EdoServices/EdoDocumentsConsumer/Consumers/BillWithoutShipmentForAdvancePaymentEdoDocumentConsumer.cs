using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer :
		BillWithoutShipmentEdoDocumentConsumer,
		IConsumer<InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo>
	{
		public BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer(
			ILogger<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer> logger,
			ITaxcomApiClient taxcomApiClient)
			: base(taxcomApiClient, logger)
		{
		}

		public async Task Consume(ConsumeContext<InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo> context)
		{
			var message = context.Message;
			Logger.LogInformation(
				"Отправляем информацию по счету на предоплату {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderWithoutShipmentInfo.Id);

			await SendDataToTaxcomApi(message);
		}
	}
}
