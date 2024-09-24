using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public abstract class BillWithoutShipmentEdoDocumentConsumer
	{
		private readonly IServiceScopeFactory _scopeFactory;

		protected BillWithoutShipmentEdoDocumentConsumer(
			IServiceScopeFactory scopeFactory,
			ILogger<BillWithoutShipmentEdoDocumentConsumer> logger)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		
		protected ILogger<BillWithoutShipmentEdoDocumentConsumer> Logger { get; }

		protected async Task SendDataToTaxcomApi(InfoForCreatingBillWithoutShipmentEdo data)
		{
			using var scope = _scopeFactory.CreateScope();
			var taxcomClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();

			try
			{
				await taxcomClient.SendDataForCreateBillWithoutShipmentByEdo(data);
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
