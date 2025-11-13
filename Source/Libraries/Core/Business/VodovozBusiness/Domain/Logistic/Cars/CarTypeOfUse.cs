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
		/// Легковая (Ларгус)
		/// </summary>
		[Display(Name = "Легковая (Ларгус)")]
		Largus,

		/// <summary>
		/// Фургон (Transit Mini)
		/// </summary>
		[Display(Name = "Фургон (Transit Mini)")]
		Minivan,

		/// <summary>
		/// Грузовой
		/// </summary>
		[Display(Name = "Грузовой")]
		GAZelle,

		/// <summary>
		/// Фура
		/// </summary>
		[Display(Name = "Фура")]
		Truck,

		/// <summary>
		/// Погрузчик
		/// </summary>
		[Display(Name = "Погрузчик")]
		Loader
	}
}
