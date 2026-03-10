using CustomerOrdersApi.Library.Services.PaymentRefund;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Factories
{
	public class PaymentRefundServiceFactory : IPaymentRefundServiceFactory
	{
		private readonly IEnumerable<IPaymentRefundService> _refundServices;
		private readonly ILogger<PaymentRefundServiceFactory> _logger;

		public PaymentRefundServiceFactory(
			IEnumerable<IPaymentRefundService> refundServices,
			ILogger<PaymentRefundServiceFactory> logger)
		{
			_refundServices = refundServices ?? throw new ArgumentNullException(nameof(refundServices));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IPaymentRefundService GetRefundService(OnlinePaymentSource? paymentSource)
		{
			if(!paymentSource.HasValue)
			{
				_logger.LogDebug("Источник оплаты не указан, используем сервис без возврата");
				throw new InvalidOperationException("Источник оплаты не указан, невозможно определить сервис возврата");
			}

			_logger.LogDebug("Получение сервиса возврата для источника оплаты: {PaymentSource}", paymentSource);

			var service = paymentSource.Value switch
			{
				// CloudPayments
				OnlinePaymentSource.FromMobileApp => GetService<CloudPaymentsRefundService>(),

				// YooKassa
				//OnlinePaymentSource.FromVodovozWebSite => GetService<YooKassaRefundService>(),

				// Yandex Split
				/*OnlinePaymentSource.FromMobileAppByYandexSplit => GetService<YandexSplitRefundService>(),
				OnlinePaymentSource.FromVodovozWebSiteByYandexSplit => GetService<YandexSplitRefundService>(),*/

				// QR
				/*OnlinePaymentSource.FromVodovozWebSiteByQr => ,
				OnlinePaymentSource.FromMobileAppByQr => ,*/

				// Неизвестный источник
				_ => throw new NotSupportedException($"Не найден сервис возврата для источника оплаты {paymentSource}")
			};

			_logger.LogDebug("Выбран сервис возврата: {ServiceType}", service.GetType().Name);
			return service;
		}

		public IPaymentRefundService GetRefundService(OnlineOrder onlineOrder)
		{
			if(onlineOrder == null)
			{
				throw new ArgumentNullException(nameof(onlineOrder));
			}

			if(onlineOrder.OnlineOrderPaymentStatus != OnlineOrderPaymentStatus.Paid)
			{
				_logger.LogDebug("Заказ {ExternalOrderId} не оплачен, возврат не требуется",
					onlineOrder.ExternalOrderId);
				throw new InvalidOperationException($"Сервис возврата {typeof(T).Name} не зарегистрирован");
			}

			return GetRefundService(onlineOrder.OnlinePaymentSource);
		}

		private IPaymentRefundService GetService<T>() where T : IPaymentRefundService
		{
			return _refundServices.FirstOrDefault(s => s is T);
		}
	}
}
