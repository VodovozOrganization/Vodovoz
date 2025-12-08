using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Единица измерения мотивации
	/// </summary>
	public enum NomenclatureMotivationUnitType
	{
		[Display(Name = "Руб. за шт.")] Item,
		[Display(Name = "% от стоимости")] Percent
	}
}
