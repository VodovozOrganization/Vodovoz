using CustomerAppsApi.Library.V3.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V3.Dto.Goods
{
	/// <summary>
	/// Прочий товар(чай, кофе и другая сопутка)
	/// </summary>
	public class OtherSaleItemDto : SaleItemDto
	{
		/// <summary>
		/// Атрибуты
		/// </summary>
		public OtherSaleItemAttributes Attributes { get; set; }
		/// <inheritdoc/>
		public override SaleItemType Type => SaleItemType.Other;
	}
}
