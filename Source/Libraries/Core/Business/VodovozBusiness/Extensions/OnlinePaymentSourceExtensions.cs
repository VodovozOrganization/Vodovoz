using System;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Extensions
{
	public static class OnlinePaymentSourceExtensions
	{
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
				case OnlinePaymentSource.FromVodovozWebSite:
					return orderSettings.PaymentByCardFromSiteId;
				case OnlinePaymentSource.FromVodovozWebSiteByQr:
					return orderSettings.GetPaymentByCardFromSiteByQrCodeId;
				default:
					throw new InvalidOperationException("Неизвестный источник онлайн оплаты");
			}
		}
	}
}
