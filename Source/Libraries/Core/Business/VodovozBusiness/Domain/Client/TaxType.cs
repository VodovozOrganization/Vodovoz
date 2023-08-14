using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum TaxType
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "С НДС")]
		WithVat,
		[Display(Name = "Без НДС")]
		WithoutVat
	}
}
