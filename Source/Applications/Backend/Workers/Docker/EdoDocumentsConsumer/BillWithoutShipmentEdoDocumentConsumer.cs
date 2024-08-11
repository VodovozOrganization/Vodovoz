using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Data.Orders.OrdersWithoutShipment;

namespace EdoDocumentsConsumer
{
	public class BillWithoutShipmentEdoDocumentConsumer : IConsumer<OrderWithoutShipmentInfo>
	{
		private readonly ILogger<BillWithoutShipmentEdoDocumentConsumer> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public BillWithoutShipmentEdoDocumentConsumer(
			ILogger<BillWithoutShipmentEdoDocumentConsumer> logger,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		public async Task Consume(ConsumeContext<OrderWithoutShipmentInfo> context)
		{
			var message = context.Message;
			_logger.LogInformation(
				"Отправляем информацию по счету без отгрузки {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.Id);

			await SendDataToTaxcomApi(message);
		}

		private async Task SendDataToTaxcomApi(OrderWithoutShipmentInfo data)
		{
			using var scope = _scopeFactory.CreateScope();
			var taxcomService = scope.ServiceProvider.GetService<TaxcomService>();
			await taxcomService.SendDataForCreateBillWithoutShipmentByEdo(data);
		}
	}
}
