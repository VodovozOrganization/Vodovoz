using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace Vodovoz.Extensions
{
	public static class OnlinePaymentSourceExtensions
	{
		public static int ConvertToPaymentFromId(
			this OnlinePaymentSource onlinePaymentSource,
			IOrderParametersProvider orderParametersProvider)
		{
			switch(onlinePaymentSource)
			{
				case OnlinePaymentSource.FromMobileApp:
					return orderParametersProvider.PaymentByCardFromMobileAppId;
				case OnlinePaymentSource.FromVodovozWebSite:
					return orderParametersProvider.PaymentByCardFromSiteId;
				case OnlinePaymentSource.FromVodovozWebSiteByQr:
					return orderParametersProvider.GetPaymentByCardFromSiteByQrCodeId;
				default:
					throw new InvalidOperationException("Неизвестный источник онлайн оплаты");
			}
		}
	}
}
