using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	/// <summary>
	/// Тип модели авто
	/// </summary>

	[Appellative(
		Nominative = "Тип модели авто",
		NominativePlural = "Типы моделей авто",
		Genitive = "Типа модели авто",
		GenitivePlural = "Типов моделей авто")]
	public enum CarTypeOfUse
	{
		/// <summary>
		/// Фургон (Ларгус)
		/// </summary>
		[Display(Name = "Фургон (Ларгус)")]
		Largus,

		/// <summary>
		/// Фура
		/// </summary>
		[Display(Name = "Фура")]
		Truck,

		/// <summary>
		/// Грузовой
		/// </summary>
		[Display(Name = "Грузовой")]
		GAZelle,

		/// <summary>
		/// Погрузчик
		/// </summary>
		[Display(Name = "Погрузчик")]
		Loader
	}
}
