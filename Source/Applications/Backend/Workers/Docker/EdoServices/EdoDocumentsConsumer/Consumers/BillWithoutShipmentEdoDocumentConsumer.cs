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

		protected async Task SendDataToTaxcomApi(InfoForCreatingBillWithoutShipmentEdo data)
		{
			try
			{
				await _taxcomApiClient.SendDataForCreateBillWithoutShipmentByEdo(data);
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
