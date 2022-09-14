using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Services;
using Microsoft.Extensions.Logging;
using System;
using FastPaymentsAPI.Library.DTO_s;

namespace FastPaymentsAPI.Library.Managers
{
	public class FastPaymentStatusChangeNotifier : IFastPaymentStatusChangeNotifier
	{
		private readonly ILogger<FastPaymentStatusChangeNotifier> _logger;
		private readonly IFastPaymentAPIFactory _fastPaymentAPIFactory;
		private readonly IVodovozSiteNotificationService _vodovozSiteNotificationService;

		public FastPaymentStatusChangeNotifier(
			ILogger<FastPaymentStatusChangeNotifier> logger,
			IFastPaymentAPIFactory fastPaymentAPIFactory,
			IVodovozSiteNotificationService vodovozSiteNotificationService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentAPIFactory = fastPaymentAPIFactory ?? throw new ArgumentNullException(nameof(fastPaymentAPIFactory));
			_vodovozSiteNotificationService =
				vodovozSiteNotificationService ?? throw new ArgumentNullException(nameof(vodovozSiteNotificationService));
		}

		public void NotifyVodovozSite(int? onlineOrderId, int paymentFrom, decimal amount, bool paymentSucceeded)
		{
			if(paymentFrom != (int)RequestFromType.FromSiteByQr || !onlineOrderId.HasValue)
			{
				return;
			}

			try
			{
				_logger.LogInformation($"Уведомляем сайт о изменении статуса оплаты онлайн-заказа: {onlineOrderId.Value}");
				var notification =
					_fastPaymentAPIFactory.GetFastPaymentStatusChangeNotificationDto(onlineOrderId.Value, amount, paymentSucceeded);
				_logger.LogInformation($"Статус оплаты онлайн-заказа: {onlineOrderId.Value} {notification.PaymentStatus}");
				_vodovozSiteNotificationService.NotifyOfFastPaymentStatusChangedAsync(notification);
			}
			catch(Exception e)
			{
				_logger.LogError(e, $"Не удалось уведомить сайт об изменении статуса оплаты онлайн-заказа {onlineOrderId.Value}");
			}
		}
	}
}
