using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public class YandexPayMapper : IYandexPayMapper
	{
		private readonly ILogger<YandexPayMapper> _logger;

		public YandexPayMapper(ILogger<YandexPayMapper> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public YandexPayRefundRequest MapToRefundRequest(RefundRequestDto request)
		{
			if(request == null)
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
				ExternalOperationId = GenerateExternalOperationId(request),
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

			var (refundStatus, orderPaymentStatus) = MapRefundStatus(operation.Status);

			return new RefundResultDto
			{
				Success = true,
				RefundId = operation.OperationId,
				//ExternalOperationId = operation.ExternalOperationId,
				RefundStatus = refundStatus
			};
		}

		/// <summary>
		/// Проверяет, является ли возврат полным
		/// </summary>
		private static bool IsFullRefund(RefundRequestDto request) => request.Amount == request.OnlineOrder?.OnlineOrderSum;

		/// <summary>
		/// Генерирует внешний ID операции для идемпотентности
		/// </summary>
		private static string GenerateExternalOperationId(RefundRequestDto request)
		{
			// Формат: refund_{orderId}_{timestamp}_{random} //Подумать над этим
			var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			var random = Guid.NewGuid().ToString("N")[..8];
			return $"refund_{request.OnlineOrder?.Id}_{timestamp}_{random}";
		}

		/// <summary>
		/// Маппит статус операции из YandexPay во внутренние статусы
		/// </summary>
		private (RefundStatus refundStatus, OnlineOrderPaymentStatus orderStatus) MapRefundStatus(YandexPayOperationStatus status)
		{
			return status switch
			{
				YandexPayOperationStatus.Success => (RefundStatus.SUCCEEDED, OnlineOrderPaymentStatus.Refunded),
				YandexPayOperationStatus.Pending => (RefundStatus.PENDING, OnlineOrderPaymentStatus.Refunding),
				YandexPayOperationStatus.Processing => (RefundStatus.PENDING, OnlineOrderPaymentStatus.Refunding),
				YandexPayOperationStatus.Fail => (RefundStatus.FAIL, OnlineOrderPaymentStatus.Paid),
				YandexPayOperationStatus.Canceled => (RefundStatus.CANCELED, OnlineOrderPaymentStatus.Paid),
				_ => (RefundStatus.FAIL, OnlineOrderPaymentStatus.Paid)
			};
		}

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
				ErrorMessage = fullErrorMessage,
				RefundStatus = RefundStatus.FAIL,
				NewPaymentStatus = OnlineOrderPaymentStatus.Refunding
			};
		}
	}
}
