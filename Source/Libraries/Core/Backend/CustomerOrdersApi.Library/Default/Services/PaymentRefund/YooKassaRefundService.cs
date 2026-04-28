using CustomerOrdersApi.Library.Default.Services.PaymentRefund;
using CustomerOrdersApi.Library.Default.Services.PaymentRefund.Mappers;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using YooKassaApi.Client;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class YooKassaRefundService : PaymentRefundServiceBase, IPaymentRefundService
	{
		private readonly IYooKassaApiClient _yooKassaClient;
		private readonly IYooKassaMapper _mapper;

		public YooKassaRefundService(
			ILogger<YooKassaRefundService> logger,
			IYooKassaApiClient yooKassaClient,
			IYooKassaMapper mapper,
			IRefundOperationRepository refundOperationRepository,
			IRefundRequestValidator refundRequestValidator
			) : base(logger, refundOperationRepository, refundRequestValidator)
		{
			_yooKassaClient = yooKassaClient ?? throw new ArgumentNullException(nameof(yooKassaClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource)
			=> paymentSource is OnlinePaymentSource.FromVodovozWebSite;

		
		protected override async Task<RefundResultDto> ProcessRefundInternalAsync(RefundRequestDto request, string idempotenceKey, CancellationToken cancellationToken)
		{
			var paymentResponse = await _yooKassaClient.GetPaymentAsync(request.TransactionId, cancellationToken);

			if(!paymentResponse.Success || paymentResponse.Data is null)
			{
				Logger.LogWarning("Не удалось получить информацию о платеже {TransactionId}: {Error}",
					request.TransactionId, paymentResponse.ErrorMessage);

				return RefundResultDto.CreateError($"Не удалось получить информацию о платеже: {paymentResponse.ErrorMessage}");
			}

			var payment = paymentResponse.Data;

			if(payment.Status is not "succeeded")
			{
				Logger.LogWarning("Попытка возврата по платежу {TransactionId} со статусом {Status}",
					request.TransactionId, payment.Status);

				return RefundResultDto.CreateError($"Возврат возможен только для платежей в статусе 'succeeded'. Текущий статус: {payment.Status}");
			}

			if(payment.RefundedAmount is not null)
			{
				var refundedAmount = decimal.TryParse(payment.RefundedAmount.Value,
					NumberStyles.Any, CultureInfo.InvariantCulture, out var refunded);

				if(refundedAmount && refunded > 0)
				{
					Logger.LogWarning("Попытка повторного возврата по платежу {TransactionId}. Уже возвращено: {RefundedAmount}",
						request.TransactionId, payment.RefundedAmount.Value);

					return RefundResultDto.CreateError($"По данному платежу уже был выполнен возврат на сумму {payment.RefundedAmount.Value} {payment.RefundedAmount.Currency}");
				}
			}

			var paymentAmount = decimal.TryParse(payment.Amount.Value,
				NumberStyles.Any, CultureInfo.InvariantCulture, out var orderAmount);

			if(!paymentAmount)
			{
				Logger.LogWarning("Не удалось распарсить сумму платежа: {Amount}", payment.Amount.Value);
				return RefundResultDto.CreateError("Не удалось определить сумму платежа");
			}

			if(request.Amount != orderAmount)
			{
				Logger.LogWarning("Сумма возврата {RequestAmount} не совпадает с суммой платежа {OrderAmount}",
					request.Amount, orderAmount);

				return RefundResultDto.CreateError($"Сумма возврата {request.Amount} не совпадает с суммой платежа {orderAmount}. В текущей реализации поддерживается только полный возврат.");
			}

			var refundRequest = _mapper.MapToRefundRequest(request);

			var refundResponse = await _yooKassaClient.RefundAsync(refundRequest, idempotenceKey, cancellationToken);

			if(!refundResponse.Success || refundResponse.Data is null)
			{
				return RefundResultDto.CreateError($"Ошибка возврата: {refundResponse.ErrorMessage}");
			}

			var result = _mapper.MapToRefundResult(refundResponse);

			if(result.Success)
			{
				Logger.LogInformation("Успешно выполнен возврат для платежа {TransactionId}, refundId: {RefundId}",
					request.TransactionId, result.RefundId);
			}
			else
			{
				Logger.LogWarning("Возврат для платежа {TransactionId} не удался: {ErrorMessage}",
					request.TransactionId, result.ErrorMessage);
			}

			return result;
		}
	}
}
