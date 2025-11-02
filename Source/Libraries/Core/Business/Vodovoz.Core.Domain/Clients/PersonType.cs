using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Форма контрагента
	/// </summary>
	[Appellative(
		Nominative = "Форма контрагента",
		NominativePlural = "Формы контрагентов")]
	public enum PersonType
	{
		/// <summary>
		/// Физическое лицо
		/// </summary>
		[Display(Name = "Физическое лицо")]
		natural,
		/// <summary>
		/// Юридическое лицо
		/// </summary>
		[Display(Name = "Юридическое лицо")]
		legal
	}
}
