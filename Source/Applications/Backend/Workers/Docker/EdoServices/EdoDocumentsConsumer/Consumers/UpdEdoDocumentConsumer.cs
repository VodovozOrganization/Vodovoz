using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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

				var result = await _taxcomApiClient.SendDataForCreateUpdByEdo(message, context.CancellationToken);

				if(result.IsFailure)
				{
					var error = result.Errors.First();

					_logger.LogError(
						"Ошибка при отправке информации по УПД {OrderId} в TaxcomApi: {ErrorCode} - {ErrorMessage}",
						message.OrderInfoForEdo.Id, error.Code, error.Message);
				}
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Непредвиденная ошибка при обработке отправки УПД {OrderId} в TaxcomApi",
					message.OrderInfoForEdo.Id);
			}
		}
	}
}
