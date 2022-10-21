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
		private readonly IMobileAppNotificationService _mobileAppNotificationService;

		public FastPaymentStatusChangeNotifier(
			ILogger<FastPaymentStatusChangeNotifier> logger,
			IFastPaymentAPIFactory fastPaymentAPIFactory,
			IVodovozSiteNotificationService vodovozSiteNotificationService,
			IMobileAppNotificationService mobileAppNotificationService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fastPaymentAPIFactory = fastPaymentAPIFactory ?? throw new ArgumentNullException(nameof(fastPaymentAPIFactory));
			_vodovozSiteNotificationService =
				vodovozSiteNotificationService ?? throw new ArgumentNullException(nameof(vodovozSiteNotificationService));
			_mobileAppNotificationService = 
				mobileAppNotificationService ?? throw new ArgumentNullException(nameof(mobileAppNotificationService));
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
		
		public void NotifyMobileApp(int? onlineOrderId, int paymentFrom, decimal amount, bool paymentSucceeded, string callbackUrl)
		{
			if(paymentFrom != (int)RequestFromType.FromMobileAppByQr || !onlineOrderId.HasValue)
			{
				return;
			}
			
			try
			{
				_logger.LogInformation($"Уведомляем мобильное приложение о изменении статуса оплаты онлайн-заказа: {onlineOrderId.Value}");
				var notification =
					_fastPaymentAPIFactory.GetFastPaymentStatusChangeNotificationDto(onlineOrderId.Value, amount, paymentSucceeded);
				_logger.LogInformation($"Статус оплаты онлайн-заказа: {onlineOrderId.Value} {notification.PaymentStatus}");
				_mobileAppNotificationService.NotifyOfFastPaymentStatusChangedAsync(notification, callbackUrl);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					$"Не удалось уведомить мобильное приложение об изменении статуса оплаты онлайн-заказа {onlineOrderId.Value}");
			}
		}
	}
}
