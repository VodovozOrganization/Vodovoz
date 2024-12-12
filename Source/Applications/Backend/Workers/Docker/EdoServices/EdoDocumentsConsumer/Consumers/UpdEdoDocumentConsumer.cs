using System;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;
using Task = System.Threading.Tasks.Task;

namespace EdoDocumentsConsumer.Consumers
{
	public class UpdEdoDocumentConsumer : IConsumer<InfoForCreatingEdoUpd>
	{
		private readonly ILogger<UpdEdoDocumentConsumer> _logger;
		private readonly ITaxcomApiClient _taxcomApiClient;

		public UpdEdoDocumentConsumer(
			ILogger<UpdEdoDocumentConsumer> logger,
			ITaxcomApiClient taxcomApiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
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
				const string errorMessage = "Ошибка при отправке информации по УПД";
				_logger.LogError(e,
					errorMessage + " {OrderId} в TaxcomApi",
					message.OrderInfoForEdo.Id);
			}
		}

		private async Task SendUpdDataToTaxcomApi(InfoForCreatingEdoUpd updData)
		{
			await _taxcomApiClient.SendDataForCreateUpdByEdo(updData);
		}
	}
}
