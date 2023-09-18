using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum BracingTypeForCupHolder
	{
		[Display(Name = "Нет")]
		None,
		[Display(Name = "На магнитах")]
		Magnets,
		[Display(Name = "На шурупах")]
		Screws,
		[Display(Name = "Любой")]
		Any
	}
}
