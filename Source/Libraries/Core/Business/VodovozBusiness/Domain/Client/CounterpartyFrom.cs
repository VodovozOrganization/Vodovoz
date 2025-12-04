using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Откуда клиент
	/// </summary>
	public enum CounterpartyFrom
	{
		/// <summary>
		/// МП
		/// </summary>
		[Display(Name = "Мобильное приложение")]
		MobileApp = 54,
		/// <summary>
		/// Сайт ВВ
		/// </summary>
		[Display(Name = "Сайт ВВ")]
		WebSite = 55,
		/// <summary>
		/// ИИ Бот
		/// </summary>
		[Display(Name = "ИИ Бот")]
		AiBot = 60
	}
}
