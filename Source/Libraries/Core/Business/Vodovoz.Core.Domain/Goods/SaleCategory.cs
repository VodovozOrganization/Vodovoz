using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Подтип категории "Товары"
	/// </summary>
	public enum SaleCategory
	{
		[Display(Name = "На продажу")]
		forSale,
		[Display(Name = "Не для продажи")]
		notForSale
	}
}
