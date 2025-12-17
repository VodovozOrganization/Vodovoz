using System;
using System.Threading.Tasks;
using FastPaymentEventsSender.Services;
using FastPaymentsApi.Contracts.Requests;
using FastPaymentsAPI.Library.Factories;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Settings.Orders;

namespace FastPaymentEventsSender.Notifications
{
	/// <inheritdoc/>
	public class FastPaymentStatusUpdatedNotifier : IFastPaymentStatusUpdatedNotifier
	{
		private readonly ILogger<FastPaymentStatusUpdatedNotifier> _logger;
		private readonly IMobileAppNotifier _mobileAppNotifier;
		private readonly IWebSiteNotifier _webSiteNotifier;
		private readonly IAiBotNotifier _aiBotNotifier;
		private readonly IDriverAPIService _driverApiService;
		private readonly IOrderSettings _orderSettings;
		private readonly IFastPaymentFactory _fastPaymentFactory;

		public FastPaymentStatusUpdatedNotifier(
			ILogger<FastPaymentStatusUpdatedNotifier> logger,
			IMobileAppNotifier mobileAppNotifier,
			IWebSiteNotifier webSiteNotifier,
			IAiBotNotifier aiBotNotifier,
			IDriverAPIService driverApiService,
			IOrderSettings orderSettings,
			IFastPaymentFactory fastPaymentFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mobileAppNotifier = mobileAppNotifier ?? throw new ArgumentNullException(nameof(mobileAppNotifier));
			_webSiteNotifier = webSiteNotifier ?? throw new ArgumentNullException(nameof(webSiteNotifier));
			_aiBotNotifier = aiBotNotifier ?? throw new ArgumentNullException(nameof(aiBotNotifier));
			_driverApiService = driverApiService ?? throw new ArgumentNullException(nameof(driverApiService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_fastPaymentFactory = fastPaymentFactory ?? throw new ArgumentNullException(nameof(fastPaymentFactory));
		}

		/// <inheritdoc/>
		public async Task NotifyPaymentStatusChangeAsync(FastPaymentStatusUpdatedEvent @event)
		{
			var payment = @event.FastPayment;
			
			if(payment.FastPaymentStatus == FastPaymentStatus.Processing)
			{
				_logger.LogWarning("Попытка отправки уведомлений для платежа в статусе \"Обрабатывается\"");
				return;
			}

			if(await TryNotifyDriver(payment))
			{
				@event.DriverNotified = true;
			}

			if(payment.OnlineOrderId == null)
			{
				_logger.LogWarning("Попытка отправки уведомления в ИПЗ без номера онлайн заказа");
				@event.HttpCode = 0;
				return;
			}

			var notification = _fastPaymentFactory.GetFastPaymentStatusChangeNotificationDto(payment);
			var httpCode = await TrySendNotification(notification, payment);
			@event.HttpCode = httpCode;
		}
		
		private async Task<bool> TryNotifyDriver(FastPayment fastPayment)
		{
			if(fastPayment?.Order == null)
			{
				return false;
			}

			var orderNumber = fastPayment.ExternalId;

			try
			{
				await _driverApiService.NotifyOfFastPaymentStatusChangedAsync(orderNumber);
				return true;
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Не удалось уведомить службу DriverApi об изменении статуса оплаты заказа {OrderNumber}",
					orderNumber);
				return false;
			}
		}

		private async Task<int> TrySendNotification(FastPaymentStatusChangeNotificationDto notification, FastPayment payment)
		{
			try
			{
				var paymentFromId = payment.PaymentByCardFrom.Id;
				var paymentFromName = payment.PaymentByCardFrom.Name;

				if(paymentFromId == _orderSettings.GetPaymentByCardFromMobileAppByQrCodeId)
				{
					return await _mobileAppNotifier.NotifyPaymentStatusChangedAsync(notification, payment.CallbackUrlForMobileApp);
				}

				if(paymentFromId == _orderSettings.GetPaymentByCardFromSiteByQrCodeId)
				{
					return await _webSiteNotifier.NotifyPaymentStatusChangedAsync(notification);
				}

				if(paymentFromId == _orderSettings.GetPaymentByCardFromAiBotByQrCodeId)
				{
					return await _aiBotNotifier.NotifyPaymentStatusChangedAsync(notification, payment.CallbackUrlForMobileApp);
				}

				_logger.LogWarning(
					"Попытка отправки уведомления на неизвестный источник, заказ {OnlineOrderId} источник оплаты: {PaymentFrom}",
					notification.PaymentDetails.OnlineOrderId,
					paymentFromName);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при отправке уведомления в ИПЗ о смене статуса быстрого платежа");
			}
			
			return 0;
		}
	}
}
