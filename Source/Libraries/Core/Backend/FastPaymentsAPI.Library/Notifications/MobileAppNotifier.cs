using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Factories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Settings.Orders;

namespace FastPaymentsAPI.Library.Notifications
{
	public class MobileAppNotifier
	{
		private readonly ILogger<MobileAppNotifier> _logger;
		private readonly MobileAppClient _mobileAppClient;
		private readonly NotificationModel _notificationModel;
		private readonly IOrderSettings _orderSettings;
		private readonly IFastPaymentFactory _fastPaymentFactory;

		public MobileAppNotifier(
			ILogger<MobileAppNotifier> logger,
			MobileAppClient mobileAppClient,
			NotificationModel notificationModel,
			IOrderSettings orderSettings,
			IFastPaymentFactory fastPaymentFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mobileAppClient = mobileAppClient ?? throw new ArgumentNullException(nameof(mobileAppClient));
			_notificationModel = notificationModel ?? throw new ArgumentNullException(nameof(notificationModel));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_fastPaymentFactory = fastPaymentFactory ?? throw new ArgumentNullException(nameof(fastPaymentFactory));
		}

		public async Task NotifyPaymentStatusChangeAsync(FastPayment payment)
		{
			var paymentFromId = payment.PaymentByCardFrom.Id;
			if(paymentFromId != _orderSettings.GetPaymentByCardFromMobileAppByQrCodeId)
			{
				_logger.LogWarning("Попытка отправки уведомления на мобильное приложение для платежа с не соответствующем источником оплаты. " +
					"Источник оплаты: {paymentFrom}.", payment.PaymentByCardFrom.Name);
				return;
			}

			if(payment.OnlineOrderId == null)
			{
				_logger.LogWarning("Попытка отправки уведомления на мобильное приложение для платежа без номера онлайн заказа.");
				return;
			}

			if(payment.FastPaymentStatus == FastPaymentStatus.Processing)
			{
				_logger.LogWarning("Попытка отправки уведомления на мобильное приложение для платежа в статусе \"Обрабатывается\".");
				return;
			}

			var notification = _fastPaymentFactory.GetFastPaymentStatusChangeNotificationDto(payment);

			var notified = await TrySendNotification(notification, payment.CallbackUrlForMobileApp, 2);

			_notificationModel.SaveNotification(payment, FastPaymentNotificationType.MobileApp, notified);
		}

		public async Task RepeatNotifyPaymentStatusChangeAsync(FastPaymentNotification notification)
		{
			if(notification is null)
			{
				throw new ArgumentNullException(nameof(notification));
			}

			if(notification.SuccessfullyNotified)
			{
				return;
			}

			var notificationDto = _fastPaymentFactory.GetFastPaymentStatusChangeNotificationDto(notification.Payment);
			var url = notification.Payment.CallbackUrlForMobileApp;

			var notified = await TrySendNotification(notificationDto, url, 1);

			_notificationModel.SaveNotification(notification.Payment, FastPaymentNotificationType.MobileApp, notified);
		}

		private async Task<bool> TrySendNotification(FastPaymentStatusChangeNotificationDto paymentNotificationDto, string url, int attempts)
		{
			while(attempts > 0)
			{
				var notified = await SendNotification(paymentNotificationDto, url);
				if(notified)
				{
					return true;
				}

				attempts--;
			}

			return false;
		}

		private async Task<bool> SendNotification(FastPaymentStatusChangeNotificationDto paymentNotificationDto, string url)
		{
			try
			{
				_logger.LogInformation("Отправка уведомления о быстрой оплате на мобильное приложение для онлайн заказа {onlineOrderId}", paymentNotificationDto.PaymentDetails.OnlineOrderId);
				return await _mobileAppClient.NotifyPaymentStatusChangedAsync(paymentNotificationDto, url);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Невозможно отправить уведомление для мобильного приложения о смене статуса оплаты быстрого платежа.");
				return false;
			}
		}
	}
}
