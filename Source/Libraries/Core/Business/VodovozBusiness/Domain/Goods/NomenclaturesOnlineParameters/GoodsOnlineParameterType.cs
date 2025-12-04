using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	/// <summary>
	/// Тип парамтров номенкалтуры
	/// </summary>
	public enum GoodsOnlineParameterType
	{
		/// <summary>
		/// Для сайта ВВ
		/// </summary>
		[Display(Name = "Для сайта ВВ")]
		ForVodovozWebSite,
		/// <summary>
		/// Для МП
		/// </summary>
		[Display(Name = "Для МП")]
		ForMobileApp,
		/// <summary>
		/// Для сайта кулер-сэйл
		/// </summary>
		[Display(Name = "Для сайта кулер-сэйл")]
		ForKulerSaleWebSite,
		/// <summary>
		/// Для ИИ Бота
		/// </summary>
		[Display(Name = "Для ИИ Бота")]
		ForAiBot
	}
}
