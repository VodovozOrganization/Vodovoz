using CustomerOrdersApi.Library.V5.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Services.PaymentRefund
{
	public sealed class RefundRequestValidator : IRefundRequestValidator
	{
		private readonly ILogger<RefundRequestValidator> _logger;

		public RefundRequestValidator(ILogger<RefundRequestValidator> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public RefundResultDto Validate(RefundRequestDto request)
		{
			if(request is null)
			{
				_logger.LogWarning("Получен пустой запрос");
				return RefundResultDto.CreateError("Запрос не может быть пустым");
			}

			if(request.OnlineOrder is null)
			{
				_logger.LogWarning("OnlineOrder не может быть null для заказа {ExternalOrderId}", request?.ExternalOrderId);
				return RefundResultDto.CreateError("OnlineOrder не может быть null");
			}

			if(request.OnlineOrder.OnlineOrderPaymentStatus is not OnlineOrderPaymentStatus.Paid)
			{
				_logger.LogWarning("Заказ {ExternalOrderId} не оплачен, возврат не требуется", request.OnlineOrder.ExternalOrderId);
				return RefundResultDto.CreateError("Заказ не оплачен, возврат не требуется");
			}

			if(string.IsNullOrEmpty(request.ExternalOrderId))
			{
				_logger.LogWarning("ExternalOrderId не может быть пустым для заказа {OnlineOrderId}", request?.OnlineOrder?.Id);
				return RefundResultDto.CreateError("ExternalOrderId не может быть пустым");
			}

			if(string.IsNullOrEmpty(request.TransactionId))
			{
				_logger.LogWarning("TransactionId не может быть пустым для заказа {ExternalOrderId}", request?.ExternalOrderId);
				return RefundResultDto.CreateError("TransactionId не может быть пустым");
			}

			if(request.Amount <= 0)
			{
				_logger.LogWarning("Сумма возврата {Amount} должна быть больше 0 для заказа {ExternalOrderId}",
					request?.Amount, request?.ExternalOrderId);
				return RefundResultDto.CreateError("Сумма возврата должна быть больше 0");
			}

			return null;
		}
	}
}
