using System;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;
using Task = System.Threading.Tasks.Task;

namespace EdoDocumentsConsumer.Consumers
{
	public class UpdEdoDocumentConsumer : IConsumer<InfoForCreatingEdoUpd>
	{
		private readonly ILogger<UpdEdoDocumentConsumer> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		public UpdEdoDocumentConsumer(
			ILogger<UpdEdoDocumentConsumer> logger,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		public async Task Consume(ConsumeContext<InfoForCreatingEdoUpd> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Отправляем информацию по заказу {OrderId} в TaxcomApi, для создания и отправки УПД по ЭДО",
					message.OrderInfoForEdo.Id);

				await SendUpdDataToTaxcomApi(message);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при отправке информации по УПД {OrderId} в TaxcomApi",
					message.OrderInfoForEdo.Id);
			}
		}

		private async Task SendUpdDataToTaxcomApi(InfoForCreatingEdoUpd updData)
		{
			using var scope = _scopeFactory.CreateScope();
			var taxcomClient = scope.ServiceProvider.GetService<ITaxcomApiClient>();
			await taxcomClient.SendDataForCreateUpdByEdo(updData);
		}
	}
}
