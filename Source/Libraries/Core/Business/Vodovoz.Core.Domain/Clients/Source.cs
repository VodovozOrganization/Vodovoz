using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Внешние источники
	/// </summary>
	public enum Source
	{
		/// <summary>
		/// Мобильное приложение
		/// </summary>
		[Appellative (
			Gender = GrammaticalGender.Neuter,
			Nominative = "мобильное приложение Веселого Водовоза",
			Genitive = "мобильного приложения Веселого Водовоза"
		)]
		[Display(Name = "Мобильное приложение")]
		MobileApp = 54,
		/// <summary>
		/// Сайт ВВ
		/// </summary>
		[Appellative (
			Gender = GrammaticalGender.Masculine,
			Nominative = "сайт Веселого Водовоза",
			Genitive = "сайта Веселого Водовоза"
		)]
		[Display(Name = "Сайт ВВ")]
		VodovozWebSite = 55,
		/// <summary>
		/// Сайт Кулер Сэйл
		/// </summary>
		[Appellative (
			Gender = GrammaticalGender.Masculine,
			Nominative = "сайт Кулер Сэйл",
			Genitive = "сайта Кулер Сэйл"
		)]
		[Display(Name = "Сайт Кулер Сэйл")]
		KulerSaleWebSite = 56,
		/// <summary>
		/// ИИ Бот
		/// </summary>
		[Appellative (
			Gender = GrammaticalGender.Masculine,
			Nominative = "ИИ Бот",
			Genitive = "ИИ Бота"
		)]
		[Display(Name = "ИИ Бот")]
		AiBot = 57
	}
}
