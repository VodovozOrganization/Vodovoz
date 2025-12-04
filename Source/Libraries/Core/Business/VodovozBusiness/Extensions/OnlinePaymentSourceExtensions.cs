using System;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Extensions
{
	/// <summary>
	/// Расширения для конвертации источника оплаты ИПЗ
	/// </summary>
	public static class OnlinePaymentSourceExtensions
	{
		/// <summary>
		/// Получение Id источника оплаты ДВ из источника оплаты ИПЗ
		/// </summary>
		/// <param name="onlinePaymentSource">Источник оплаты ИПЗ <see cref="OnlinePaymentSource"/></param>
		/// <param name="orderSettings">Настройки для заказа <see cref="IOrderSettings"/></param>
		/// <returns>Id источника оплаты ДВ или ошибка</returns>
		/// <exception cref="InvalidOperationException">Ошибка, если пришел неивестный источник оплаты</exception>
		public static int ConvertToPaymentFromId(
			this OnlinePaymentSource onlinePaymentSource,
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
	}
}
