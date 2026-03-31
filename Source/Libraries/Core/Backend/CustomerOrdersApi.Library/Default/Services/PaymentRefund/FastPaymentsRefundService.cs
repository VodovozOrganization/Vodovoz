using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using FastPaymentsApi.Client;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class FastPaymentsRefundService : PaymentRefundServiceBase
	{
		private readonly IFastPaymentsApiClient _fastPaymentApiClient;

		public FastPaymentsRefundService(
			ILogger<FastPaymentsRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory,
			IFastPaymentsApiClient fastPaymentApiClient,
			IRefundOperationRepository refundOperationRepository
		) : base(logger, unitOfWorkFactory, httpClientFactory, refundOperationRepository)
		{
			_fastPaymentApiClient = fastPaymentApiClient ?? throw new ArgumentNullException(nameof(fastPaymentApiClient));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource)
			=> paymentSource is OnlinePaymentSource.FromMobileAppByQr 
				or OnlinePaymentSource.FromVodovozWebSiteByQr 
				or OnlinePaymentSource.FromAiBotByQr;

		protected override async Task<RefundResultDto> ProcessRefundInternalAsync(RefundRequestDto request, string idempotenceKey, CancellationToken cancellationToken)
		{
			var ticket = request.TransactionId;

			_logger.LogInformation(
				"Начало отмены платежа QR для заказа {ExternalOrderId}, Ticket: {Ticket}",
				request.ExternalOrderId,
				ticket);

			_logger.LogInformation("Пришел запрос на возврат средств по платежу с сессией: {Ticket}", ticket);

			var reverseRequest = new ReverseTicketRequestDTO
			{
				Ticket = ticket,
				Amount = request.Amount * 100 // Авангард считает сумму в копейках
			};

			try
			{
				var reverseOrderResponse = await _fastPaymentApiClient.ReverseOrderAsync(reverseRequest, cancellationToken);

				if(reverseOrderResponse == null)
				{
					_logger.LogError("Получен пустой ответ от API при возврате платежа {Ticket}", ticket);
					return CreateErrorResult("Ошибка при получении ответа от сервиса платежей");
				}

				if(!string.IsNullOrWhiteSpace(reverseOrderResponse.ErrorMessage))
				{
					_logger.LogError(
						"Ошибка при возврате средств по платежу {Ticket}. Ошибка: {ErrorMessage}",
						ticket,
						reverseOrderResponse.ErrorMessage);

					return CreateErrorResult($"Ошибка возврата платежа: {reverseOrderResponse.ErrorMessage}");
				}

				_logger.LogInformation(
					"Возврат по платежу QR успешно выполнен для заказа {ExternalOrderId}, Ticket: {Ticket}",
					request.ExternalOrderId,
					ticket);

				return CreateSuccessResult(ticket);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Неожиданная ошибка при возврате средств по платежу {Ticket}",
					ticket);

				return CreateErrorResult($"Неожиданная ошибка: {ex.Message}");
			}
		}
	}
}
