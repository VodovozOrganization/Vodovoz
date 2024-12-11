using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(
		Nominative = "Тип модели авто",
		NominativePlural = "Типы моделей авто")]
	public enum CarTypeOfUse
	{
		[Display(Name = "Фургон (Ларгус)")]
		Largus,
		[Display(Name = "Фура")]
		Truck,
		[Display(Name = "Грузовой")]
		GAZelle,
		[Display(Name = "Погрузчик")]
		Loader
	}
}
