using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer :
		BillWithoutShipmentEdoDocumentConsumer,
		IConsumer<InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo>
	{
		public BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer(
			ILogger<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer> logger,
			IServiceScopeFactory scopeFactory)
			: base(scopeFactory, logger)
		{
		}

		public async Task Consume(ConsumeContext<InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo> context)
		{
			var message = context.Message as InfoForCreatingBillWithoutShipmentEdo;
			Logger.LogInformation(
				"Отправляем информацию по счету на предоплату {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderWithoutShipmentInfo.Id);

			await SendDataToTaxcomApi(message);
		}
	}
}
