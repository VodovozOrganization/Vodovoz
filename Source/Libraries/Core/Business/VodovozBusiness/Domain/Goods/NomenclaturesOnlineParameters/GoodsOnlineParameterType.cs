using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	public enum GoodsOnlineParameterType
	{
		[Display(Name = "Для сайта ВВ")]
		ForVodovozWebSite,
		[Display(Name = "Для МП")]
		ForMobileApp,
		[Display(Name = "Для сайта кулер-сэйл")]
		ForKulerSaleWebSite
	}
}
