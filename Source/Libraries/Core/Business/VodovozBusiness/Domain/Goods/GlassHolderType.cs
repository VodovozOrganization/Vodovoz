using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Nominative = "Тип стаканодержателя",
		NominativePlural = "Типы стаканодержателей")]
	public enum GlassHolderType
	{
		[Display(Name = "Стаканодержатель отсутствует")]
		None,
		[Display(Name = "Стаканодержатель На магните")]
		Magnet,
		[Display(Name = "Стаканодержатель На шурупах")]
		Screw,
		[Display(Name = "Стаканодержатель Универсальный")]
		Universal
	}
}
