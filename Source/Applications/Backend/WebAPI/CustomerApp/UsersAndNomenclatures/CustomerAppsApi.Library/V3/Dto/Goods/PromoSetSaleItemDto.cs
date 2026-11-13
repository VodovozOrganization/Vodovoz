using CustomerAppsApi.Library.V3.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V3.Dto.Goods
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
