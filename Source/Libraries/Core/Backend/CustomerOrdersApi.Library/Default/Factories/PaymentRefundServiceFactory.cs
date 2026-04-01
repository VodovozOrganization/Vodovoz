using CustomerOrdersApi.Library.Services.PaymentRefund;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Factories
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
				_logger.LogDebug("Источник оплаты не указан");
				throw new InvalidOperationException("Источник оплаты не указан, невозможно определить сервис возврата");
			}

			var service = _refundServices.FirstOrDefault(s => s.CanHandle(paymentSource.Value))
				?? throw new NotSupportedException($"Не найден сервис для {paymentSource}");

			_logger.LogDebug("Выбран сервис возврата: {ServiceType}", service.GetType().Name);
			return service;
		}

		public IPaymentRefundService GetRefundService(OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				throw new ArgumentNullException(nameof(onlineOrder));
			}

			return GetRefundService(onlineOrder.OnlinePaymentSource);
		}
	}
}
