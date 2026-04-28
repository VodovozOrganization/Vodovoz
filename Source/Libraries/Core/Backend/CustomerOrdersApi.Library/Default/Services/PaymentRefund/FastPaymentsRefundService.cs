using CustomerOrdersApi.Library.Default.Services.PaymentRefund;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
using FastPaymentsApi.Client;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Logging;
using System;
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
			IFastPaymentsApiClient fastPaymentApiClient,
			IRefundOperationRepository refundOperationRepository
		) : base(logger, refundOperationRepository)
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

			Logger.LogInformation(
				"Начало отмены платежа QR для заказа {ExternalOrderId}, Ticket: {Ticket}",
				request.ExternalOrderId,
				ticket);

			Logger.LogInformation("Пришел запрос на возврат средств по платежу с сессией: {Ticket}", ticket);

			var reverseRequest = new ReverseTicketRequestDTO
			{
				Ticket = ticket,
				Amount = request.Amount * 100
			};

			try
			{
				var reverseOrderResponse = await _fastPaymentApiClient.ReverseOrderAsync(reverseRequest, cancellationToken);

				if(reverseOrderResponse is null)
				{
					Logger.LogError("Получен пустой ответ от API при возврате платежа {Ticket}", ticket);
					return RefundResultDto.CreateError("Ошибка при получении ответа от сервиса платежей");
				}

				if(!string.IsNullOrWhiteSpace(reverseOrderResponse.ErrorMessage))
				{
					Logger.LogError(
						"Ошибка при возврате средств по платежу {Ticket}. Ошибка: {ErrorMessage}",
						ticket,
						reverseOrderResponse.ErrorMessage);

					return RefundResultDto.CreateError($"Ошибка возврата платежа: {reverseOrderResponse.ErrorMessage}");
				}

				Logger.LogInformation(
					"Возврат по платежу QR успешно выполнен для заказа {ExternalOrderId}, Ticket: {Ticket}",
					request.ExternalOrderId,
					ticket);

				return RefundResultDto.CreateSuccess();
			}
			catch(Exception ex)
			{
				Logger.LogError(ex,
					"Неожиданная ошибка при возврате средств по платежу {Ticket}",
					ticket);

				return RefundResultDto.CreateError($"Неожиданная ошибка: {ex.Message}");
			}
		}
	}
}
