using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters
{
	/// <summary>
	/// Параметры онлайн отображения товара
	/// </summary>
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
