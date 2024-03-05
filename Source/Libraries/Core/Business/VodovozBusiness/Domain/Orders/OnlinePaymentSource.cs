using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OnlinePaymentSource
	{
		[Display(Name = "Сайт ВВ")]
		FromVodovozWebSite,
		[Display(Name = "Сайт ВВ по QR")]
		FromVodovozWebSiteByQr,
		[Display(Name = "МП по QR")]
		FromMobileApp
	}
}
