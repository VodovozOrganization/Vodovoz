using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Organizations
{
	/// <summary>
	/// Уровень подразделения в структуре
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "уровень подразделения",
		NominativePlural = "уровни подразделений")]
	public enum SubdivisionLevel
	{
		/// <summary>
		/// Центр финансовой ответственности
		/// </summary>
		[Display(Name = "ЦФО")]
		Cfo,
		/// <summary>
		/// Отдел
		/// </summary>
		[Display(Name = "Отдел")]
		Department,
		/// <summary>
		/// Сектор
		/// </summary>
		[Display(Name = "Сектор")]
		Sector
	}
}
