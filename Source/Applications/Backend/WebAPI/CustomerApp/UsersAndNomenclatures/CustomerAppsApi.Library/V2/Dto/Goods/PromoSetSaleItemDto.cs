using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

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
		public override SaleItemType Type => SaleItemType.PromoSet;
	}
}
