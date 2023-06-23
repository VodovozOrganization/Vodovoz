using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods.NomenclatureOnlineParameters
{
	public enum NomenclatureOnlineParameterType
	{
		[Display(Name = "Для сайта ВВ")]
		ForVodovozWebSite,
		[Display(Name = "Для МП")]
		ForMobileApp,
		[Display(Name = "Для сайта кулер-сэйл")]
		ForKulerSaleWebSite
	}
}
