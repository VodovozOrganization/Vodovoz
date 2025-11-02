using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "тип страховки авто",
		NominativePlural = "типы страховок авто",
		Genitive = "типа страховки авто",
		GenitivePlural = "типов страховок авто")]
	public enum CarInsuranceType
	{
		[Display(Name = "Осаго")]
		Osago,
		[Display(Name = "Каско")]
		Kasko
	}
}
