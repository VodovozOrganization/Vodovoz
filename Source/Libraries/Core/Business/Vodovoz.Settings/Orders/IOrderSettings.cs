using System;

namespace Vodovoz.Settings.Orders
{
	public interface IOrderSettings
	{
		int PaymentByCardFromMobileAppId { get; }
		int PaymentByCardFromSiteId { get; }
		int PaymentByCardFromSmsId { get; }
		int PaymentByCardFromOnlineStoreId { get; }
		int PaymentFromTerminalId { get; }
		int PaymentFromSmsYuKassaId { get; }
		int OldInternalOnlineStoreId { get; }
		/// <summary>
		/// Id источника оплаты Яндекс Сплит Сайт
		/// </summary>
		int PaymentByCardFromYandexSplitFromSiteId { get; }
		/// <summary>
		/// Id источника оплаты Яндекс Сплит МП
		/// </summary>
		int PaymentByCardFromYandexSplitFromMobileAppId { get; }
		int GetPaymentByCardFromMarketplaceId { get; }
		int GetPaymentByCardFromFastPaymentServiceId { get; }
		int GetPaymentByCardFromAvangardId { get; }
		int GetPaymentByCardFromSiteByQrCodeId { get; }
		int GetPaymentByCardFromMobileAppByQrCodeId { get; }
		int GetPaymentByCardFromKulerSaleId { get; }
		int[] PaymentsByCardFromNotToSendSalesReceipts { get; }
		int[] PaymentsByCardFromForNorthOrganization { get; }
		int[] PaymentsByCardFromAvangard { get; }
		int GetDiscountReasonStockBottle10PercentsId { get; }
		int GetDiscountReasonStockBottle20PercentsId { get; }
		int GetClientsSecondOrderDiscountReasonId { get; }
		int ReferFriendDiscountReasonId { get; }
		int FastDeliveryLateDiscountReasonId { get; }
		int GetOrderRatingForMandatoryProcessing { get; }
		DateTime GetDateAvailabilityRatingOrder { get; }

		/// <summary>
		/// Id оснований для скидки ОКС
		/// </summary>
		int[] OksDiscountReasonsIds { get; }

		/// <summary>
		/// Id оснований для скидки Замена
		/// </summary>
		int[] ProductChangeDiscountReasonsIds { get; }

		/// <summary>
		/// Id оснований для скидки Довоз
		/// </summary>
		int[] AdditionalDeliveryDiscountReasonsIds { get; }
	}
}
