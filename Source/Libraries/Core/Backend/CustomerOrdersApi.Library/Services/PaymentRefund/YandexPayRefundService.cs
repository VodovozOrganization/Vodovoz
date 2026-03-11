using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients;
using CustomerOrdersApi.Library.Services.PaymentRefund.Mappers;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public class YandexPayRefundService : PaymentRefundServiceBase, IPaymentRefundService
	{
		private readonly IYandexPayHttpClient _yandexPayClient;
		private readonly IYandexPayMapper _mapper;

		public YandexPayRefundService(
			ILogger<YandexPayRefundService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IYandexPayHttpClient yandexPayClient,
			IYandexPayMapper mapper,
			IHttpClientFactory httpClientFactory
			) : base(logger, unitOfWorkFactory, httpClientFactory)
		{
			_yandexPayClient = yandexPayClient ?? throw new ArgumentNullException(nameof(yandexPayClient));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		}

		public override bool CanHandle(OnlinePaymentSource paymentSource)
			=> paymentSource is OnlinePaymentSource.FromMobileAppByYandexSplit or OnlinePaymentSource.FromVodovozWebSiteByYandexSplit;

		public override async Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request, CancellationToken cancellationToken)
		{
			try
			{
				ValidateRequest(request);

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

				var refundRequest = _mapper.MapToRefundRequest(request);
				var refundResponse = await _yandexPayClient.RefundAsync(refundRequest, cancellationToken);

				if(!refundResponse.Success)
				{
					return CreateErrorResult($"Ошибка возврата: {refundResponse.ErrorMessage}");
				}

				return _mapper.MapToRefundResult(refundResponse);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка возврата");
				return CreateErrorResult("Техническая ошибка");
			}
		}
	}
}
