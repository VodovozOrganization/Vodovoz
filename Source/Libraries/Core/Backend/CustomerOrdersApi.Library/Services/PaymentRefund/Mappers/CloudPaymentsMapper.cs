using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public class CloudPaymentsMapper : ICloudPaymentsMapper
	{
		private readonly ILogger<CloudPaymentsMapper> _logger;

		public CloudPaymentsMapper(ILogger<CloudPaymentsMapper> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public CloudPaymentsRefundRequest MapToRefundRequest(RefundRequestDto request)
		{
			if(request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if(string.IsNullOrWhiteSpace(request.TransactionId))
			{
				throw new ArgumentException("TransactionId не может быть пустым", nameof(request));
			}

			if(!long.TryParse(request.TransactionId, out var transactionId))
			{
				_logger.LogWarning("Не удалось распарсить TransactionId: {TransactionId}", request.TransactionId);
				throw new ArgumentException($"Неверный формат TransactionId: {request.TransactionId}");
			}

			_logger.LogDebug(
				"Маппинг RefundRequestDto в CloudPaymentsRefundRequest. TransactionId: {TransactionId}, Amount: {Amount}",
				request.TransactionId,
				request.Amount);

			return new CloudPaymentsRefundRequest
			(
				OnlineOrder: request.OnlineOrder,
				ExternalOrderId: request.ExternalOrderId,
				Amount: request.Amount,
				TransactionId: transactionId.ToString()
			);
		}

		public RefundResultDto MapToRefundResult(CloudPaymentsResponse<CloudPaymentsRefundResult> cloudPaymentsResponse)
		{
			if(cloudPaymentsResponse == null)
			{
				return CreateErrorResult("Пустой ответ от платежной системы");
			}

			if(!cloudPaymentsResponse.Success)
			{
				return CreateErrorResult(cloudPaymentsResponse.Message ?? "Неизвестная ошибка CloudPayments");
			}

			return new RefundResultDto
			{
				Success = cloudPaymentsResponse.Success,
				RefundId = cloudPaymentsResponse.Model?.TransactionId.ToString(),
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
				ErrorMessage = fullErrorMessage
			};
		}
	}
}
