using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	public enum OnlinePaymentSource
	{
		[Display(Name = "Сайт ВВ")]
		FromVodovozWebSite,
		[Display(Name = "Сайт ВВ по QR")]
		FromVodovozWebSiteByQr,
		[Display(Name = "Мобильное приложение")]
		FromMobileApp,
		[Display(Name = "Мобильное приложение по QR")]
		FromMobileAppByQr
	}
}
