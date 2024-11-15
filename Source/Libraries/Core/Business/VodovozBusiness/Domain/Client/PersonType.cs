using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(
		Nominative = "Форма контрагента",
		NominativePlural = "Формы контрагентов")]
	public enum PersonType
	{
		[Display(Name = "Физическое лицо")]
		natural,
		[Display(Name = "Юридическое лицо")]
		legal
	}
}
