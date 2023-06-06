using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class OrderParametersProvider : IOrderParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public OrderParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int PaymentByCardFromMobileAppId => _parametersProvider.GetIntValue("PaymentByCardFromMobileAppId");
		public int PaymentByCardFromOnlineStoreId => _parametersProvider.GetIntValue("PaymentByCardFromOnlineStoreId");
		public int PaymentByCardFromSiteId => _parametersProvider.GetIntValue("PaymentByCardFromSiteId");
		public int PaymentByCardFromSmsId => _parametersProvider.GetIntValue("sms_payment_by_card_from_id");
		public int PaymentFromTerminalId => _parametersProvider.GetIntValue("paymentfrom_terminal_id");
		public int OldInternalOnlineStoreId => _parametersProvider.GetIntValue("OldInternalOnlineStoreId");
		public int GetPaymentByCardFromMarketplaceId => _parametersProvider.GetIntValue("payment_by_card_from_marketplace_id");
		public int PaymentFromSmsYuKassaId => _parametersProvider.GetIntValue("payment_by_card_from_sms_yukassa_id");
		public int GetPaymentByCardFromFastPaymentServiceId =>
			_parametersProvider.GetIntValue("payment_by_card_from_fast_payment_service_id");
		public int GetPaymentByCardFromAvangardId =>
			_parametersProvider.GetIntValue("payment_by_card_from_avangard_id");
		public int GetPaymentByCardFromSiteByQrCodeId =>
			_parametersProvider.GetIntValue("payment_by_card_from_site_by_qr_code_id");
		public int GetPaymentByCardFromMobileAppByQrCodeId =>
			_parametersProvider.GetIntValue("payment_by_card_from_mobile_app_by_qr_code_id");
		public int GetDiscountReasonStockBottle10PercentsId =>
			_parametersProvider.GetIntValue("discount_reason_stock_bottle_10_percents");
		public int GetDiscountReasonStockBottle20PercentsId =>
			_parametersProvider.GetIntValue("discount_reason_stock_bottle_20_percents");

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
