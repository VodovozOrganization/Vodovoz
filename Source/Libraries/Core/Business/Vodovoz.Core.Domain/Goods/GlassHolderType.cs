using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Тип стаканодержателя
	/// </summary>
	[Appellative(
		Nominative = "Тип стаканодержателя",
		NominativePlural = "Типы стаканодержателей")]
	public enum GlassHolderType
	{
		/// <summary>
		/// Стаканодержатель отсутствует
		/// </summary>
		[Display(Name = "Стаканодержатель отсутствует")]
		None,
		/// <summary>
		/// Стаканодержатель На магните
		/// </summary>
		[Display(Name = "Стаканодержатель На магните")]
		Magnet,
		/// <summary>
		/// Стаканодержатель На шурупах
		/// </summary>
		[Display(Name = "Стаканодержатель На шурупах")]
		Screw,
		/// <summary>
		/// Стаканодержатель Универсальный
		/// </summary>
		[Display(Name = "Стаканодержатель Универсальный")]
		Universal
	}
}
