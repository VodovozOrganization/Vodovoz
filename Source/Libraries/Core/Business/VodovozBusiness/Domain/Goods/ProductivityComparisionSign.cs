using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum ProductivityComparisionSign
	{
		[Display(Name = "Не менее")]
		NoLess,
		[Display(Name = "<=")]
		LessOrEqual,
		[Display(Name = ">=")]
		MoreOrEqual
	}
}
