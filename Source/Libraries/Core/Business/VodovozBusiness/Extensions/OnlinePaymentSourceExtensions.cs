using System;
using CustomerOrders.Contracts;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Orders;

namespace VodovozBusiness.Extensions
{
	public static class OnlinePaymentSourceExtensions
	{
		/// <summary>
		/// Получение Id источника оплаты ДВ из источника оплаты ИПЗ
		/// </summary>
		/// <param name="onlinePaymentSource">Источник оплаты ИПЗ <see cref="OnlinePaymentSource"/></param>
		/// <param name="orderSettings">Настройки для заказа <see cref="InvalidOperationException"/></param>
		/// <returns>Id источника оплаты ДВ или ошибка</returns>
		/// <exception cref="IOrderSettings">Ошибка, если пришел неивестный источник оплаты</exception>
		public static int ConvertToPaymentFromId(
			this OnlinePaymentSource? onlinePaymentSource,
			IOrderSettings orderSettings)
		{
			switch(onlinePaymentSource)
			{
				case OnlinePaymentSource.FromMobileApp:
					return orderSettings.PaymentByCardFromMobileAppId;
				case OnlinePaymentSource.FromMobileAppByQr:
					return orderSettings.GetPaymentByCardFromMobileAppByQrCodeId;
				case OnlinePaymentSource.FromMobileAppByYandexSplit:
					return orderSettings.PaymentByCardFromYandexSplitFromMobileAppId;
				case OnlinePaymentSource.FromVodovozWebSite:
					return orderSettings.PaymentByCardFromSiteId;
				case OnlinePaymentSource.FromVodovozWebSiteByQr:
					return orderSettings.GetPaymentByCardFromSiteByQrCodeId;
				case OnlinePaymentSource.FromVodovozWebSiteByYandexSplit:
					return orderSettings.PaymentByCardFromYandexSplitFromSiteId;
				case OnlinePaymentSource.FromAiBotByQr:
					return orderSettings.GetPaymentByCardFromAiBotByQrCodeId;
				default:
					throw new InvalidOperationException("Неизвестный источник онлайн оплаты");
			}
		}
		
		public static ExternalPaymentSource? ToExternalPaymentSource(this OnlinePaymentSource? source)
		{
			switch(source)
			{
				case null:
					return null;
				case OnlinePaymentSource.FromVodovozWebSite:
					return ExternalPaymentSource.FromVodovozWebSite;
				case OnlinePaymentSource.FromVodovozWebSiteByQr:
					return ExternalPaymentSource.FromVodovozWebSiteByQr;
				case OnlinePaymentSource.FromVodovozWebSiteByYandexSplit:
					return ExternalPaymentSource.FromVodovozWebSiteByYandexSplit;
				case OnlinePaymentSource.FromMobileApp:
					return ExternalPaymentSource.FromMobileApp;
				case OnlinePaymentSource.FromMobileAppByQr:
					return ExternalPaymentSource.FromMobileAppByQr;
				case OnlinePaymentSource.FromMobileAppByYandexSplit:
					return ExternalPaymentSource.FromMobileAppByYandexSplit;
				case OnlinePaymentSource.FromAiBotByQr:
					return ExternalPaymentSource.FromAiBotByQr;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение внешнего источника оплаты");
			}
		}
		
		public static OnlinePaymentSource? ToOnlinePaymentSource(this ExternalPaymentSource? source)
		{
			switch(source)
			{
				case null:
					return null;
				case ExternalPaymentSource.FromVodovozWebSite:
					return OnlinePaymentSource.FromVodovozWebSite;
				case ExternalPaymentSource.FromVodovozWebSiteByQr:
					return OnlinePaymentSource.FromVodovozWebSiteByQr;
				case ExternalPaymentSource.FromVodovozWebSiteByYandexSplit:
					return OnlinePaymentSource.FromVodovozWebSiteByYandexSplit;
				case ExternalPaymentSource.FromMobileApp:
					return OnlinePaymentSource.FromMobileApp;
				case ExternalPaymentSource.FromMobileAppByQr:
					return OnlinePaymentSource.FromMobileAppByQr;
				case ExternalPaymentSource.FromMobileAppByYandexSplit:
					return OnlinePaymentSource.FromMobileAppByYandexSplit;
				case ExternalPaymentSource.FromAiBotByQr:
					return OnlinePaymentSource.FromAiBotByQr;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, "Неизвестное значение внешнего источника оплаты");
			}
		}
	}
}
