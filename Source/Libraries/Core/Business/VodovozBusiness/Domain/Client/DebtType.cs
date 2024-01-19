using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Nominative = "Тип задолженности",
		NominativePlural = "Типы задолженности")]
	public enum DebtType
	{
		[Display(Name = "Судебный")]
		Judicial,
		[Display(Name = "Списание")]
		WriteOff,
		[Display(Name = "Краткосрочный")]
		ShortTerm
	}
}
