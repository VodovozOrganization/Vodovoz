using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Data.Orders;

namespace EdoDocumentsConsumer
{
	public class BillEdoDocumentConsumer : IConsumer<OrderInfoForEdo>
	{
		private readonly ILogger<BillEdoDocumentConsumer> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public BillEdoDocumentConsumer(
			ILogger<BillEdoDocumentConsumer> logger,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		public async Task Consume(ConsumeContext<OrderInfoForEdo> context)
		{
			var message = context.Message;
			_logger.LogInformation(
				"Отправляем информацию по заказу {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.Id);

			await SendDataToTaxcomApi(message);
		}

		private async Task SendDataToTaxcomApi(OrderInfoForEdo data)
		{
			using var scope = _scopeFactory.CreateScope();
			var taxcomService = scope.ServiceProvider.GetService<TaxcomService>();
			await taxcomService.SendDataForCreateBillByEdo(data);
		}
	}
}
