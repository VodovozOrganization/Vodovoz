using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Показатель производительности
	/// </summary>
	public enum ProductivityComparisionSign
	{
		/// <summary>
		/// Не менее
		/// </summary>
		[Display(Name = "Не менее")]
		NoLess,
		/// <summary>
		/// Меньше либо равно
		/// </summary>
		[Display(Name = "<=")]
		LessOrEqual,
		/// <summary>
		/// Больше либо равно
		/// </summary>
		[Display(Name = ">=")]
		MoreOrEqual
	}
}
