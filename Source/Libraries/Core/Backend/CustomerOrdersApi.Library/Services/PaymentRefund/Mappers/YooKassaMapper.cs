using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using YooKassaApi.Library.Models;
using YooKassaApi.Library.Requests;
using YooKassaApi.Library.Responses;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public class YooKassaMapper : IYooKassaMapper
	{
		private readonly ILogger<YooKassaMapper> _logger;

		public YooKassaMapper(ILogger<YooKassaMapper> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public YooKassaRefundRequest MapToRefundRequest(RefundRequestDto request)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if(string.IsNullOrWhiteSpace(request.TransactionId))
			{
				throw new ArgumentException("TransactionId не может быть пустым", nameof(request));
			}

			_logger.LogDebug(
				"Маппинг RefundRequestDto в YooKassaRefundRequest. PaymentId: {PaymentId}, Amount: {Amount}",
				request.TransactionId,
				request.Amount);

			return new YooKassaRefundRequest
			{
				PaymentId = request.TransactionId,
				Amount = new YooKassaAmount
				{
					Value = request.Amount.ToString("F2", CultureInfo.InvariantCulture),
					Currency = "RUB"
				},
				Description = $"Возврат средств по заказу {request.ExternalOrderId}",
				Metadata = new Dictionary<string, string>
				{
					["external_order_id"] = request.ExternalOrderId,
					["online_order_id"] = request.OnlineOrder?.Id.ToString(),
					["refund_initiator"] = "customer_api",
					["request_id"] = Guid.NewGuid().ToString("N")
				}
			};
		}

		public RefundResultDto MapToRefundResult(YooKassaResult<YooKassaRefundResponse> yooKassaResponse)
		{
			if(yooKassaResponse is null)
			{
				return CreateErrorResult("Пустой ответ от платежной системы");
			}

			if(!yooKassaResponse.Success)
			{
				return CreateErrorResult(
					yooKassaResponse.ErrorMessage ?? "Неизвестная ошибка ЮKassa",
					yooKassaResponse.ErrorCode,
					yooKassaResponse.ErrorParameter);
			}

			var refund = yooKassaResponse.Data;
			if(refund is null)
			{
				return CreateErrorResult("Ответ не содержит данных о возврате");
			}

			_logger.LogDebug(
				"Возврат обработан. RefundId: {RefundId}, Status: {Status}",
				refund.Id,
				refund.Status);

			return new RefundResultDto
			{
				Success = refund.Status is YooKassaRefundStatus.Succeeded,
				RefundId = refund.Id,
				ErrorMessage = GetErrorMessage(refund)
			};
		}

		private static string GetErrorMessage(YooKassaRefundResponse refund)
		{
			if(refund.CancellationDetails is not null)
			{
				return $"Возврат отменен. Инициатор: {refund.CancellationDetails.Party}, причина: {refund.CancellationDetails.Reason}";
			}

			if(refund.Status == YooKassaRefundStatus.Canceled)
			{
				return "Возврат отменен по неизвестной причине";
			}

			if(refund.Status != YooKassaRefundStatus.Succeeded)
			{
				return $"Статус возврата: {refund.Status}";
			}

			return null;
		}

		private static RefundResultDto CreateErrorResult(string errorMessage, string errorCode = null, string errorParameter = null)
		{
			var fullErrorMessage = errorMessage;

			if(!string.IsNullOrEmpty(errorCode))
			{
				fullErrorMessage += $" (Код: {errorCode}";

				if(!string.IsNullOrEmpty(errorParameter))
				{
					fullErrorMessage += $", параметр: {errorParameter}";
				}

				fullErrorMessage += ")";
			}

			return new RefundResultDto
			{
				Success = false,
				ErrorMessage = fullErrorMessage
			};
		}
	}
}
