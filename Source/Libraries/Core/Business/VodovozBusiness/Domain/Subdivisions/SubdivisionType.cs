using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[Appellative(Gender = GrammaticalGender.Masculine
		Nominative = "тип подразделения",
		NominativePlural = "типы подразделений")]
	public enum SubdivisionType
	{
		[Display(Name = "Стандартный")]
		Default,
		[Display(Name = "Логистика")]
		Logistic,
		[Display(Name = "Офис")]
		Office
	}
}
