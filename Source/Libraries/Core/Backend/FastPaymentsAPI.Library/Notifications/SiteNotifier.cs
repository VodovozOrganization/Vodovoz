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
	public class SiteNotifier
	{
		private readonly ILogger<SiteNotifier> _logger;
		private readonly SiteClient _siteClient;
		private readonly NotificationModel _notificationModel;
		private readonly IOrderSettings _orderSettings;
		private readonly IFastPaymentFactory _fastPaymentFactory;

		public SiteNotifier(
			ILogger<SiteNotifier> logger,
			SiteClient siteClient,
			NotificationModel notificationModel,
			IOrderSettings orderSettings,
			IFastPaymentFactory fastPaymentFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_siteClient = siteClient ?? throw new ArgumentNullException(nameof(siteClient));
			_notificationModel = notificationModel ?? throw new ArgumentNullException(nameof(notificationModel));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_fastPaymentFactory = fastPaymentFactory ?? throw new ArgumentNullException(nameof(fastPaymentFactory));
		}

		public async Task NotifyPaymentStatusChangeAsync(FastPayment payment)
		{
			var paymentFromId = payment.PaymentByCardFrom.Id;
			if(paymentFromId != _orderSettings.GetPaymentByCardFromSiteByQrCodeId)
			{
				_logger.LogWarning("Попытка отправки уведомления на сайт для платежа с не соответствующем источником оплаты. " +
					"Источник оплаты: {paymentFrom}.", payment.PaymentByCardFrom.Name);
				return;
			}

			if(payment.OnlineOrderId == null)
			{
				_logger.LogWarning("Попытка отправки уведомления на сайт для платежа без номера онлайн заказа.");
				return;
			}

			if(payment.FastPaymentStatus == FastPaymentStatus.Processing)
			{
				_logger.LogWarning("Попытка отправки уведомления на сайт для платежа в статусе \"Обрабатывается\".");
				return;
			}

			var notification = _fastPaymentFactory.GetFastPaymentStatusChangeNotificationDto(payment);

			var notified  = await TrySendNotification(notification, 2);

			_notificationModel.SaveNotification(payment, FastPaymentNotificationType.Site, notified);
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

			var notified = await TrySendNotification(notificationDto, 1);

			_notificationModel.SaveNotification(notification.Payment, FastPaymentNotificationType.Site, notified);
		}

		private async Task<bool> TrySendNotification(FastPaymentStatusChangeNotificationDto paymentNotificationDto, int attempts)
		{
			while(attempts > 0)
			{
				var notified = await SendNotification(paymentNotificationDto);
				if(notified)
				{
					return true;
				}

				attempts--;
			}

			return false;
		}

		private async Task<bool> SendNotification(FastPaymentStatusChangeNotificationDto paymentNotificationDto)
		{
			try
			{
				_logger.LogInformation("Отправка уведомления о быстрой оплате на сайт для онлайн заказа {onlineOrderId}", paymentNotificationDto.PaymentDetails.OnlineOrderId);
				return await _siteClient.NotifyPaymentStatusChangedAsync(paymentNotificationDto);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Невозможно отправить уведомление для сайта о смене статуса оплаты быстрого платежа.");
				return false;
			}
		}
	}
}
