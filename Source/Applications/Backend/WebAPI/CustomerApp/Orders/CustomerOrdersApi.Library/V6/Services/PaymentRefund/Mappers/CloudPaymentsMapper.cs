using CloudPaymentsApi.Library.Models;
using CloudPaymentsApi.Library.Requests;
using CloudPaymentsApi.Library.Responses;
using CustomerOrdersApi.Library.V6.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using System;

namespace CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers
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
			if(request is null)
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
				externalOrderId: request.ExternalOrderId,
				amount: request.Amount,
				transactionId: transactionId.ToString()
			);
		}

		public RefundResultDto MapToRefundResult(CloudPaymentsResponse<CloudPaymentsRefundResult> cloudPaymentsResponse)
		{
			if(cloudPaymentsResponse is null)
			{
				return RefundResultDto.CreateError("Пустой ответ от платежной системы");
			}

			if(!cloudPaymentsResponse.Success)
			{
				return RefundResultDto.CreateError(cloudPaymentsResponse.Message ?? "Неизвестная ошибка CloudPayments");
			}

			return new RefundResultDto
			{
				Success = cloudPaymentsResponse.Success,
				RefundId = cloudPaymentsResponse.Model?.TransactionId.ToString(),
			};
		}
	}
}
