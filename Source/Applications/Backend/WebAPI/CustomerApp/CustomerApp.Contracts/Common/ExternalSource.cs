using System.ComponentModel.DataAnnotations;

namespace CustomerApp.Contracts.Common
{
	/// <summary>
	/// Внешний источник (ИПЗ)
	/// </summary>
	public enum ExternalSource
	{
		/// <summary>
		/// Мобильное приложение
		/// </summary>
		[Display(Name = "Мобильное приложение")]
		MobileApp = 54,
		/// <summary>
		/// Сайт ВВ
		/// </summary>
		[Display(Name = "Сайт ВВ")]
		VodovozWebSite = 55,
		/// <summary>
		/// Сайт Кулер Сэйл
		/// </summary>
		[Display(Name = "Сайт Кулер Сэйл")]	
		KulerSaleWebSite = 56,
		/// <summary>
		/// ИИ Бот
		/// </summary>
		[Display(Name = "ИИ Бот")]
		AiBot = 61
	}
}
