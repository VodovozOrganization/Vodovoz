using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	/// <summary>
	/// Откуда пользователь
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
		[Display(Name = "Сайт")]
		WebSite = 55
	}
}
