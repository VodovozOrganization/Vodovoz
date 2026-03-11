using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Payments;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public abstract class PaymentRefundServiceBase : IPaymentRefundService
	{
		protected readonly ILogger _logger;
		protected readonly IUnitOfWorkFactory _unitOfWorkFactory;
		protected readonly IHttpClientFactory _httpClientFactory;

		protected PaymentRefundServiceBase(
			ILogger logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		public abstract bool CanHandle(OnlinePaymentSource paymentSource);

		public abstract Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request, CancellationToken cancellationToken);

		/// <summary>
		/// Создает результат для успешного возврата
		/// </summary>
		protected virtual RefundResultDto CreateSuccessResult(string refundId)
		{
			return new RefundResultDto(true, refundId, default, default, default, default, DateTime.UtcNow, RefundStatus.SUCCEEDED, OnlineOrderPaymentStatus.Refunded);
		}

		/// <summary>
		/// Создает результат для ошибки возврата
		/// </summary>
		protected virtual RefundResultDto CreateErrorResult(
			string errorMessage,
			string cancellationParty = null,
			string cancellationReason = null)
		{
			return new RefundResultDto(false, default, default, errorMessage, cancellationParty, cancellationReason, DateTime.UtcNow, RefundStatus.FAIL, OnlineOrderPaymentStatus.Paid);
		}

		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		protected virtual void ValidateRequest(RefundRequestDto request)
		{
			if(request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			if(request.OnlineOrder is null)
			{
				throw new ArgumentException("OnlineOrder не может быть null", nameof(request));
			}

			if(string.IsNullOrEmpty(request.ExternalOrderId))
			{
				throw new ArgumentException("ExternalOrderId не может быть пустым", nameof(request));
			}

			if(string.IsNullOrEmpty(request.TransactionId))
			{
				throw new ArgumentException("TransactionId не может быть пустым", nameof(request));
			}

			if(request.Amount <= 0)
			{
				throw new ArgumentException("Сумма возврата должна быть больше 0", nameof(request));
			}
		}
	}
}
