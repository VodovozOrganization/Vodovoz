using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Responses;

namespace EdoDocumentsConsumer.Consumers
{
	public abstract class BillWithoutShipmentEdoDocumentConsumer
	{
		private readonly ITaxcomApiClientSdkVersion _taxcomApiClient;

		protected BillWithoutShipmentEdoDocumentConsumer(
			ITaxcomApiClientSdkVersion taxcomApiClient,
			ILogger<BillWithoutShipmentEdoDocumentConsumer> logger)
		{
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		
		protected ILogger<BillWithoutShipmentEdoDocumentConsumer> Logger { get; }

		protected async Task SendDataToTaxcomApi<T>(T data)
			where T : InfoForCreatingBillWithoutShipmentEdo
		{
			TaxcomResponse result = null;
			
			try
			{
				switch (data)
				{
					case InfoForCreatingBillWithoutShipmentForDebtEdo debtData:
						result = await _taxcomApiClient.SendDataForCreateBillWithoutShipmentForDebtByEdo(debtData);
						break;
					case InfoForCreatingBillWithoutShipmentForPaymentEdo paymentData:
						result = await _taxcomApiClient.SendDataForCreateBillWithoutShipmentForPaymentByEdo(paymentData);
						break;
					case InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo advancePaymentData:
						result = await _taxcomApiClient.SendDataForCreateBillWithoutShipmentForAdvancePaymentByEdo(advancePaymentData);
						break;
				}

				if(result is { Ok: false })
				{
					Logger.LogError(
						"Ошибка при отправке {OrderWithoutShipment} {OrderId} в TaxcomApi. Ошибка {ErrorMessage}",
						data.GetBillWithoutShipmentInfoTitle(),
						data.OrderWithoutShipmentInfo.Id,
						result.ErrorMessage);
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
