using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForDebtEdoDocumentConsumer :
		BillWithoutShipmentEdoDocumentConsumer,
		IConsumer<InfoForCreatingBillWithoutShipmentForDebtEdo>
	{
		public BillWithoutShipmentForDebtEdoDocumentConsumer(
			ILogger<BillWithoutShipmentForDebtEdoDocumentConsumer> logger,
			ITaxcomApiClient taxcomApiClient)
			: base(taxcomApiClient, logger)
		{
		}

		public async Task Consume(ConsumeContext<InfoForCreatingBillWithoutShipmentForDebtEdo> context)
		{
			var message = context.Message;
			
			Logger.LogInformation(
				"Отправляем информацию по счету без отгрузки на долг {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderWithoutShipmentInfo.Id);

			await SendDataToTaxcomApi(message);
		}
	}
}
