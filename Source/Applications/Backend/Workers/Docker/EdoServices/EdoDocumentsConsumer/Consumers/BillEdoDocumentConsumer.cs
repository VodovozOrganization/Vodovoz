using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillEdoDocumentConsumer : IConsumer<InfoForCreatingEdoBill>
	{
		private readonly ILogger<BillEdoDocumentConsumer> _logger;
		private readonly ITaxcomApiClient _taxcomApiClient;

		public BillEdoDocumentConsumer(
			ILogger<BillEdoDocumentConsumer> logger,
			ITaxcomApiClient taxcomApiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
		}

		public async Task Consume(ConsumeContext<InfoForCreatingEdoBill> context)
		{
			var message = context.Message;

			try
			{
				_logger.LogInformation(
					"Отправляем информацию по заказу {OrderId} в TaxcomApi, для создания и отправки счета по ЭДО",
					message.OrderInfoForEdo.Id);

				await SendDataToTaxcomApi(message);
			}
			catch(Exception e)
			{
				const string errorMessage = "Ошибка при отправке информации по счету";
				
				_logger.LogError(e,
					errorMessage + " {OrderId} в TaxcomApi",
					message.OrderInfoForEdo.Id);
			}
		}

		private async Task SendDataToTaxcomApi(InfoForCreatingEdoBill data)
		{
			await _taxcomApiClient.SendDataForCreateBillByEdo(data);
		}
	}
}
