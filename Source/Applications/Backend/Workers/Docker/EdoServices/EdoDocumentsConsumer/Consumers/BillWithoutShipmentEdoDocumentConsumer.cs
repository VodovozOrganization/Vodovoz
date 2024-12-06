using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public abstract class BillWithoutShipmentEdoDocumentConsumer
	{
		private readonly ITaxcomApiClient _taxcomApiClient;

		protected BillWithoutShipmentEdoDocumentConsumer(
			ITaxcomApiClient taxcomApiClient,
			ILogger<BillWithoutShipmentEdoDocumentConsumer> logger)
		{
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		
		protected ILogger<BillWithoutShipmentEdoDocumentConsumer> Logger { get; }

		protected async Task SendDataToTaxcomApi<T>(T data)
			where T : InfoForCreatingBillWithoutShipmentEdo
		{
			try
			{
				switch (data)
				{
					case InfoForCreatingBillWithoutShipmentForDebtEdo debtData:
						await _taxcomApiClient.SendDataForCreateBillWithoutShipmentForDebtByEdo(debtData);
						break;
					case InfoForCreatingBillWithoutShipmentForPaymentEdo paymentData:
						await _taxcomApiClient.SendDataForCreateBillWithoutShipmentForPaymentByEdo(paymentData);
						break;
					case InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo advancePaymentData:
						await _taxcomApiClient.SendDataForCreateBillWithoutShipmentForAdvancePaymentByEdo(advancePaymentData);
						break;
				}
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при отправке {OrderWithoutShipment} {OrderId} в TaxcomApi",
					data.GetBillWithoutShipmentInfoTitle(),
					data.OrderWithoutShipmentInfo.Id);
			}
		}
	}
}
