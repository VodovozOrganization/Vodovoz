using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client.ClientClassification
{
	[Appellative(
		Nominative = "Классификация контрагента",
		NominativePlural = "Классификации контрагентов")]
	public enum CounterpartyCompositeClassification
	{
		[Display(Name = "AX")]
		AX,
		[Display(Name = "AY")]
		AY,
		[Display(Name = "AZ")]
		AZ,
		[Display(Name = "BX")]
		BX,
		[Display(Name = "BY")]
		BY,
		[Display(Name = "BZ")]
		BZ,
		[Display(Name = "CX")]
		CX,
		[Display(Name = "CY")]
		CY,
		[Display(Name = "CZ")]
		CZ,
		[Display(Name = "Новый")]
		New
	}
}
