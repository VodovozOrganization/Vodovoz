using System;
using System.Threading.Tasks;
using EdoDocumentsConsumer.Services;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillEdoDocumentConsumer : IConsumer<InfoForCreatingEdoBill>
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

		public async Task Consume(ConsumeContext<InfoForCreatingEdoBill> context)
		{
			var message = context.Message;
			_logger.LogInformation(
				"Отправляем информацию по заказу {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
				message.OrderInfoForEdo.Id);

			await SendDataToTaxcomApi(message);
		}

		private async Task SendDataToTaxcomApi(InfoForCreatingEdoBill data)
		{
			using var scope = _scopeFactory.CreateScope();
			var taxcomService = scope.ServiceProvider.GetService<TaxcomService>();
			await taxcomService.SendDataForCreateBillByEdo(data);
		}
	}
}
