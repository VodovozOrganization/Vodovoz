using CustomerAppsApi.Library.V3.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V3.Dto.Goods
{
	/// <summary>
	/// Вода
	/// </summary>
	public class WaterSaleItemDto : SaleItemDto
	{
		/// <summary>
		/// Атрибуты
		/// </summary>
		public WaterSaleItemAttributes Attributes { get; set; }
		/// <inheritdoc/>
		public override SaleItemType Type => SaleItemType.Water;
	}
}
