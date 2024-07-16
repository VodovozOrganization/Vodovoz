using System;
using Vodovoz.Settings;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Settings.Database.Orders
{
	public class OrderSettings : IOrderSettings
	{
		private readonly ISettingsController _settingsController;

		public OrderSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int PaymentByCardFromMobileAppId => _settingsController.GetIntValue("PaymentByCardFromMobileAppId");
		public int PaymentByCardFromOnlineStoreId => _settingsController.GetIntValue("PaymentByCardFromOnlineStoreId");
		public int PaymentByCardFromSiteId => _settingsController.GetIntValue("PaymentByCardFromSiteId");
		public int PaymentByCardFromSmsId => _settingsController.GetIntValue("sms_payment_by_card_from_id");
		public int PaymentFromTerminalId => _settingsController.GetIntValue("paymentfrom_terminal_id");
		public int OldInternalOnlineStoreId => _settingsController.GetIntValue("OldInternalOnlineStoreId");
		public int GetPaymentByCardFromMarketplaceId => _settingsController.GetIntValue("payment_by_card_from_marketplace_id");
		public int PaymentFromSmsYuKassaId => _settingsController.GetIntValue("payment_by_card_from_sms_yukassa_id");
		public int GetPaymentByCardFromFastPaymentServiceId =>
			_settingsController.GetIntValue("payment_by_card_from_fast_payment_service_id");
		public int GetPaymentByCardFromAvangardId =>
			_settingsController.GetIntValue("payment_by_card_from_avangard_id");
		public int GetPaymentByCardFromSiteByQrCodeId =>
			_settingsController.GetIntValue("payment_by_card_from_site_by_qr_code_id");
		public int GetPaymentByCardFromMobileAppByQrCodeId =>
			_settingsController.GetIntValue("payment_by_card_from_mobile_app_by_qr_code_id");
		public int GetPaymentByCardFromKulerSaleId =>
			_settingsController.GetIntValue("payment_by_card_from_kuler_sale_id");
		public int GetDiscountReasonStockBottle10PercentsId =>
			_settingsController.GetIntValue("discount_reason_stock_bottle_10_percents");
		public int GetDiscountReasonStockBottle20PercentsId =>
			_settingsController.GetIntValue("discount_reason_stock_bottle_20_percents");
		public int GetClientsSecondOrderDiscountReasonId =>
			_settingsController.GetIntValue("clients_second_order_discount_reason_id");
		public int ReferFriendDiscountReasonId =>
			_settingsController.GetValue<int>(nameof(ReferFriendDiscountReasonId));
		public int FastDeliveryLateDiscountReasonId =>
			_settingsController.GetValue<int>(nameof(FastDeliveryLateDiscountReasonId));
		public int GetOrderRatingForMandatoryProcessing =>
			_settingsController.GetIntValue(nameof(GetOrderRatingForMandatoryProcessing));
		public DateTime GetDateAvailabilityRatingOrder =>
			_settingsController.GetValue<DateTime>(nameof(GetDateAvailabilityRatingOrder));

		public int[] PaymentsByCardFromAvangard =>
			new[]
			{
				GetPaymentByCardFromFastPaymentServiceId,
				GetPaymentByCardFromSiteByQrCodeId,
				GetPaymentByCardFromMobileAppByQrCodeId,
				GetPaymentByCardFromAvangardId
			};

		public int[] PaymentsByCardFromNotToSendSalesReceipts =>
			new[]
			{
				PaymentByCardFromMobileAppId,
				PaymentByCardFromOnlineStoreId,
				PaymentByCardFromSiteId,
				PaymentByCardFromSmsId,
				GetPaymentByCardFromMarketplaceId
			};

		public int[] PaymentsByCardFromForNorthOrganization =>
			new[]
			{
				PaymentByCardFromMobileAppId,
				PaymentByCardFromOnlineStoreId,
				PaymentByCardFromSiteId
			};

	}
}
