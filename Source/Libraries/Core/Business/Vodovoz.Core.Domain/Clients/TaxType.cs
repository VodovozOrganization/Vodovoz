using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
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
