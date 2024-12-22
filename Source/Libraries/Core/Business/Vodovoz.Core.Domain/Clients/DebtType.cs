using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(
		Nominative = "Тип задолженности",
		NominativePlural = "Типы задолженности")]
	public enum DebtType
	{
		[Display(Name = "Краткосрочный")]
		ShortTerm,
		[Display(Name = "Судебный")]
		Judicial,
		[Display(Name = "Списание")]
		WriteOff
	}
}
