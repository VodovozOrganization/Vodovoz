using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V2.Dto.Goods
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
		public override SaleItemType Type => SaleItemType.Other;
	}
}
