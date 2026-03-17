using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

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

			string cancellationParty = null;
			string cancellationReason = null;

			if(refund.CancellationDetails != null)
			{
				cancellationParty = MapCancellationParty(refund.CancellationDetails.Party);
				cancellationReason = MapCancellationReason(refund.CancellationDetails.Reason);

				_logger.LogWarning(
					"Возврат отменен. Инициатор: {Party}, причина: {Reason}",
					cancellationParty,
					cancellationReason);
			}

			return new RefundResultDto
			{
				Success = refund.Status is YooKassaRefundStatus.Succeeded,
				RefundId = refund.Id,
				ErrorMessage = GetErrorMessage(refund),
				CancellationParty = cancellationParty,
				CancellationReason = cancellationReason
			};
		}

		/// <summary>
		/// Маппит инициатора отмены
		/// </summary>
		private static string MapCancellationParty(string party)
		{
			return party switch
			{
				YooKassaCancellationParty.YooKassa => "yoo_kassa",
				YooKassaCancellationParty.RefundNetwork => "refund_network",
				_ => party
			};
		}

		/// <summary>
		/// Маппит причину отмены
		/// </summary>
		private static string MapCancellationReason(string reason)
		{
			return reason switch
			{
				YooKassaCancellationReason.GeneralDecline => "general_decline",
				YooKassaCancellationReason.InsufficientFunds => "insufficient_funds",
				YooKassaCancellationReason.RejectedByPayee => "rejected_by_payee",
				YooKassaCancellationReason.RejectedByTimeout => "rejected_by_timeout",
				YooKassaCancellationReason.YooMoneyAccountClosed => "yoo_money_account_closed",
				YooKassaCancellationReason.PaymentArticleNumberNotFound => "payment_article_number_not_found",
				YooKassaCancellationReason.PaymentBasketIdNotFound => "payment_basket_id_not_found",
				YooKassaCancellationReason.PaymentTruCodeNotFound => "payment_tru_code_not_found",
				YooKassaCancellationReason.SomeArticlesAlreadyRefunded => "some_articles_already_refunded",
				YooKassaCancellationReason.TooManyRefundingArticles => "too_many_refunding_articles",
				_ => reason
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
