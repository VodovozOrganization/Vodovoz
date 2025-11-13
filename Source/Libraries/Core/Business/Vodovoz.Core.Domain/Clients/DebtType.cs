using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	/// <summary>
	/// Тип задолженности
	/// </summary>
	[Appellative(
		Nominative = "Тип задолженности",
		NominativePlural = "Типы задолженности")]
	public enum DebtType
	{
		/// <summary>
		/// Краткосрочный
		/// </summary>
		[Display(Name = "Краткосрочный")]
		ShortTerm,
		/// <summary>
		/// Судебный
		/// </summary>
		[Display(Name = "Судебный")]
		Judicial,
		/// <summary>
		/// Списание
		/// </summary>
		[Display(Name = "Списание")]
		WriteOff
	}
}
