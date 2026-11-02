using CustomerOrdersApi.Library.V6.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using YandexPayApi.Client;
using YandexPayApi.Library.Models;

namespace CustomerOrdersApi.Library.V6.Services.PaymentRefund
{
	public class YandexPayRefundService : PaymentRefundServiceBase, IPaymentRefundService
	{
		private readonly IYandexPayApiClient _yandexPayClient;
		private readonly IYandexPayMapper _mapper;

		public YandexPayRefundService(
			ILogger<YandexPayRefundService> logger,
			IYandexPayApiClient yandexPayClient,
			IYandexPayMapper mapper,
			IRefundOperationRepository refundOperationRepository,
			IRefundRequestValidator refundRequestValidator
			) : base(logger, refundOperationRepository, refundRequestValidator)
		{
			_yandexPayClient = yandexPayClient ?? throw new ArgumentNullException(nameof(yandexPayClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource)
			=> paymentSource is OnlinePaymentSource.FromMobileAppByYandexSplit or OnlinePaymentSource.FromVodovozWebSiteByYandexSplit;

		protected override async Task<RefundResultDto> ProcessRefundInternalAsync(RefundRequestDto request, string idempotenceKey, CancellationToken cancellationToken)
		{
			var orderResponse = await _yandexPayClient.GetOrderAsync(request.TransactionId, cancellationToken);

			if(!orderResponse.Success)
			{
				return RefundResultDto.CreateError($"Не удалось получить заказ: {orderResponse.ErrorMessage}");
			}

			if(orderResponse.Data?.Order?.PaymentStatus is not YandexPayPaymentStatus.Captured)
			{
				Logger.LogWarning("Попытка возврата по заказу {ExternalOrderId} со статусом {Status}",
					request.ExternalOrderId, orderResponse.Data?.Order?.PaymentStatus);

				return RefundResultDto.CreateError("Возврат невозможен для текущего статуса заказа");
			}

			var amountIsDecimal = decimal.TryParse(
				orderResponse?.Data?.Order?.OrderAmount,
				NumberStyles.Any,
				CultureInfo.InvariantCulture,
				out var orderAmount);

			if(!amountIsDecimal || amountIsDecimal && request.Amount != orderAmount)
			{
				Logger.LogWarning("Сумма возврата {RequestAmount} не совпадает с суммой заказа {OrderAmount}",
					request.Amount, orderAmount);

				return RefundResultDto.CreateError($"Сумма возврата {request.Amount} не совпадает с суммой заказа {orderAmount}");
			}

			var refundRequest = _mapper.MapToRefundRequest(request, idempotenceKey);
			var refundResponse = await _yandexPayClient.RefundAsync(refundRequest, cancellationToken);

			if(!refundResponse.Success)
			{
				return RefundResultDto.CreateError($"Ошибка возврата: {refundResponse.ErrorMessage}");
			}

			return _mapper.MapToRefundResult(refundResponse);
		}
	}
}
