using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum PumpType
	{
		[Display(Name = "Механическая")]
		Mechanical,
		[Display(Name = "Электронная")]
		Electronic
	}
}
