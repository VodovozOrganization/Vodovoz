using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Промонабор
	/// </summary>
	public class PromoSetSaleItemDto : SaleItemDto
	{
		/// <summary>
		/// Атрибуты
		/// </summary>
		public PromoSetSaleItemAttributes Attributes { get; set; }
		/// <inheritdoc/>
		public override SaleItemType Type => SaleItemType.PromoSet;
	}
}
