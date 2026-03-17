using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public class YandexPayMapper : IYandexPayMapper
	{
		private readonly ILogger<YandexPayMapper> _logger;

		public YandexPayMapper(ILogger<YandexPayMapper> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public YandexPayRefundRequest MapToRefundRequest(RefundRequestDto request, string idempotenceKey)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if(string.IsNullOrWhiteSpace(request.ExternalOrderId))
			{
				throw new ArgumentException("ExternalOrderId не может быть пустым", nameof(request));
			}

			_logger.LogDebug(
				"Маппинг RefundRequestDto в YandexPayRefundRequest. OrderId: {OrderId}, Amount: {Amount}",
				request.ExternalOrderId,
				request.Amount);

			var refundRequest = new YandexPayRefundRequest
			{
				RefundAmount = request.Amount,
				ExternalOperationId = idempotenceKey,
				OrderId = request.TransactionId
			};

			if(IsFullRefund(request))
			{
				refundRequest.TargetCart = new YandexPayTargetCart
				{
					Items = new List<YandexPayCartItem>()
				};
			}

			return refundRequest;
		}

		public RefundResultDto MapToRefundResult(YandexPayResult<YandexPayRefundResponse> yandexPayResponse)
		{
			if(yandexPayResponse == null)
			{
				return CreateErrorResult("Пустой ответ от платежной системы");
			}

			if(!yandexPayResponse.Success)
			{
				return CreateErrorResult(
					yandexPayResponse.ErrorMessage ?? "Неизвестная ошибка YandexPay",
					yandexPayResponse.ErrorCode);
			}

			var operation = yandexPayResponse.Data?.Operation;
			if(operation == null)
			{
				return CreateErrorResult("Ответ не содержит данных об операции");
			}

			_logger.LogDebug(
				"Возврат успешно обработан. OperationId: {OperationId}, Status: {Status}",
				operation.OperationId,
				operation.Status);

			return new RefundResultDto
			{
				Success = true,
				RefundId = operation.OperationId
			};
		}

		/// <summary>
		/// Проверяет, является ли возврат полным
		/// </summary>
		private static bool IsFullRefund(RefundRequestDto request) => request.Amount == request.OnlineOrder?.OnlineOrderSum;

		/// <summary>
		/// Создает результат с ошибкой
		/// </summary>
		private static RefundResultDto CreateErrorResult(string errorMessage, string errorCode = null)
		{
			var fullErrorMessage = string.IsNullOrEmpty(errorCode)
				? errorMessage
				: $"{errorMessage} (Код: {errorCode})";

			return new RefundResultDto
			{
				Success = false,
				ErrorMessage = fullErrorMessage
			};
		}
	}
}
