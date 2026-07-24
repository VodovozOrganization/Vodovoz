using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V2.Dto.Goods
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

		public override SaleItemType Type => SaleItemType.Water;
	}
}
