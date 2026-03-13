using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients;
using CustomerOrdersApi.Library.Services.PaymentRefund.Mappers;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class YooKassaRefundService : PaymentRefundServiceBase, IPaymentRefundService
	{
		private readonly IYooKassaHttpClient _yooKassaClient;
		private readonly IYooKassaMapper _mapper;

		public YooKassaRefundService(
			ILogger<YooKassaRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IYooKassaHttpClient yooKassaClient,
			IYooKassaMapper mapper,
			IHttpClientFactory httpClientFactory
			) : base(logger, unitOfWorkFactory, httpClientFactory)
		{
			_yooKassaClient = yooKassaClient ?? throw new ArgumentNullException(nameof(yooKassaClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource)
			=> paymentSource is OnlinePaymentSource.FromVodovozWebSite;

		public override async Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request, CancellationToken cancellationToken)
		{
			try
			{
				ValidateRequest(request);

				var paymentResponse = await _yooKassaClient.GetPaymentAsync(request.TransactionId, cancellationToken);

				if(!paymentResponse.Success || paymentResponse.Data is null)
				{
					_logger.LogWarning("Не удалось получить информацию о платеже {TransactionId}: {Error}",
						request.TransactionId, paymentResponse.ErrorMessage);

					return CreateErrorResult($"Не удалось получить информацию о платеже: {paymentResponse.ErrorMessage}");
				}

				var payment = paymentResponse.Data;

				if(payment.Status is not YooKassaPaymentStatus.Succeeded)
				{
					_logger.LogWarning("Попытка возврата по платежу {TransactionId} со статусом {Status}",
						request.TransactionId, payment.Status);

					return CreateErrorResult($"Возврат возможен только для платежей в статусе 'succeeded'. Текущий статус: {payment.Status}");
				}

				if(payment.RefundedAmount is not null)
				{
					var refundedAmount = decimal.TryParse(payment.RefundedAmount.Value,
						NumberStyles.Any, CultureInfo.InvariantCulture, out var refunded);

					if(refundedAmount && refunded > 0)
					{
						_logger.LogWarning("Попытка повторного возврата по платежу {TransactionId}. Уже возвращено: {RefundedAmount}",
							request.TransactionId, payment.RefundedAmount.Value);

						return CreateErrorResult($"По данному платежу уже был выполнен возврат на сумму {payment.RefundedAmount.Value} {payment.RefundedAmount.Currency}");
					}
				}

				var paymentAmount = decimal.TryParse(payment.Amount.Value,
					NumberStyles.Any, CultureInfo.InvariantCulture, out var orderAmount);

				if(!paymentAmount)
				{
					_logger.LogWarning("Не удалось распарсить сумму платежа: {Amount}", payment.Amount.Value);
					return CreateErrorResult("Не удалось определить сумму платежа");
				}

				if(request.Amount != orderAmount)
				{
					_logger.LogWarning("Сумма возврата {RequestAmount} не совпадает с суммой платежа {OrderAmount}",
						request.Amount, orderAmount);

					return CreateErrorResult($"Сумма возврата {request.Amount} не совпадает с суммой платежа {orderAmount}. В текущей реализации поддерживается только полный возврат.");
				}

				var idempotenceKey = GenerateIdempotenceKey(request);

				var refundRequest = _mapper.MapToRefundRequest(request);

				var refundResponse = await _yooKassaClient.RefundAsync(refundRequest, idempotenceKey, cancellationToken);

				if(!refundResponse.Success || refundResponse.Data is null)
				{
					return CreateErrorResult($"Ошибка возврата: {refundResponse.ErrorMessage}");
				}

				var result = _mapper.MapToRefundResult(refundResponse);

				if(result.Success)
				{
					_logger.LogInformation("Успешно выполнен возврат для платежа {TransactionId}, refundId: {RefundId}",
						request.TransactionId, result.RefundId);
				}
				else
				{
					_logger.LogWarning("Возврат для платежа {TransactionId} не удался: {ErrorMessage}",
						request.TransactionId, result.ErrorMessage);
				}

				return result;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка возврата для платежа {TransactionId}", request?.TransactionId);
				return CreateErrorResult("Техническая ошибка при обработке возврата");
			}
		}

		/// <summary>
		/// Генерирует ключ идемпотентности для запроса
		/// </summary>
		private static string GenerateIdempotenceKey(RefundRequestDto request)
		{
			// Используем комбинацию идентификаторов для обеспечения уникальности
			// Формат: refund_{paymentId}_{timestamp}_{random}
			var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			var random = Guid.NewGuid().ToString("N")[..8];
			return $"refund_{request.TransactionId}_{timestamp}_{random}";
		}

		/// <summary>
		/// Создает результат с ошибкой
		/// </summary>
		private static RefundResultDto CreateErrorResult(string errorMessage)
		{
			return new RefundResultDto
			{
				Success = false,
				ErrorMessage = errorMessage,
				RefundStatus = RefundStatus.FAIL,
				NewPaymentStatus = OnlineOrderPaymentStatus.Paid,
			};
		}
	}
}
