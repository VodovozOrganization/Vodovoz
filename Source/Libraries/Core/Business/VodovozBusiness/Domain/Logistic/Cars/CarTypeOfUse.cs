using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	public enum CarTypeOfUse
	{
		[Display(Name = "Фургон (Ларгус)")]
		Largus,
		[Display(Name = "Фура")]
		Truck,
		[Display(Name = "Грузовой")]
		GAZelle
	}
}
