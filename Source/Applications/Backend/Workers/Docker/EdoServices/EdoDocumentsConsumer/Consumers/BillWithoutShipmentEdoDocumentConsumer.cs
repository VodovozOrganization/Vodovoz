using System;
using System.Threading.Tasks;
using EdoDocumentsConsumer.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentEdoDocumentConsumer : IConsumer<InfoForCreatingBillWithoutShipmentEdo>
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

		public async Task Consume(ConsumeContext<InfoForCreatingBillWithoutShipmentEdo> context)
		{
			var message = context.Message;
			_logger.LogInformation(
				"Отправляем информацию по счету без отгрузки {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderWithoutShipmentInfo.Id);

			await SendDataToTaxcomApi(message);
		}

		private async Task SendDataToTaxcomApi(InfoForCreatingBillWithoutShipmentEdo data)
		{
			using var scope = _scopeFactory.CreateScope();
			var taxcomService = scope.ServiceProvider.GetService<TaxcomService>();
			await taxcomService.SendDataForCreateBillWithoutShipmentByEdo(data);
		}
	}
}
