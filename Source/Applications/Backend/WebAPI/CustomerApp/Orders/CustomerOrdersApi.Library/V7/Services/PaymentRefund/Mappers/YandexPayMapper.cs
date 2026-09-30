using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using YandexPayApi.Library.Models;
using YandexPayApi.Library.Requests;
using YandexPayApi.Library.Responses;

namespace CustomerOrdersApi.Library.V7.Services.PaymentRefund.Mappers
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

			if(request.IsFullRefund())
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
			if(yandexPayResponse is null)
			{
				return RefundResultDto.CreateError("Пустой ответ от платежной системы");
			}

			if(!yandexPayResponse.Success)
			{
				return RefundResultDto.CreateError(
					yandexPayResponse.ErrorMessage ?? "Неизвестная ошибка YandexPay");
			}

			var operation = yandexPayResponse.Data?.Operation;
			if(operation is null)
			{
				return RefundResultDto.CreateError("Ответ не содержит данных об операции");
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
	}
}
