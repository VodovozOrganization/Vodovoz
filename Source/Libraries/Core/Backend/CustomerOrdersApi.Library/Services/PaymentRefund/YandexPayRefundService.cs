using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Mappers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Orders;
using YandexPayApi.Client;
using YandexPayApi.Library.Models;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class YandexPayRefundService : PaymentRefundServiceBase, IPaymentRefundService
	{
		private readonly IYandexPayApiClient _yandexPayClient;
		private readonly IYandexPayMapper _mapper;

		public YandexPayRefundService(
			ILogger<YandexPayRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IYandexPayApiClient yandexPayClient,
			IYandexPayMapper mapper,
			IHttpClientFactory httpClientFactory,
			IRefundOperationRepository refundOperationRepository
			) : base(logger, unitOfWorkFactory, httpClientFactory, refundOperationRepository)
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
				return CreateErrorResult($"Не удалось получить заказ: {orderResponse.ErrorMessage}");
			}

			if(orderResponse.Data?.Order?.PaymentStatus is not YandexPayPaymentStatus.Captured)
			{
				_logger.LogWarning("Попытка возврата по заказу {ExternalOrderId} со статусом {Status}",
					request.ExternalOrderId, orderResponse.Data?.Order?.PaymentStatus);

				return CreateErrorResult("Возврат невозможен для текущего статуса заказа");
			}

			var amountIsDecimal = decimal.TryParse(
				orderResponse?.Data?.Order?.OrderAmount,
				NumberStyles.Any,
				CultureInfo.InvariantCulture,
				out var orderAmount);

			if(!amountIsDecimal || amountIsDecimal && request.Amount != orderAmount)
			{
				_logger.LogWarning("Сумма возврата {RequestAmount} не совпадает с суммой заказа {OrderAmount}",
					request.Amount, orderAmount);

				return CreateErrorResult($"Сумма возврата {request.Amount} не совпадает с суммой заказа {orderAmount}");
			}

			var refundRequest = _mapper.MapToRefundRequest(request, idempotenceKey);
			var refundResponse = await _yandexPayClient.RefundAsync(refundRequest, cancellationToken);

			if(!refundResponse.Success)
			{
				return CreateErrorResult($"Ошибка возврата: {refundResponse.ErrorMessage}");
			}

			return _mapper.MapToRefundResult(refundResponse);
		}
	}
}
