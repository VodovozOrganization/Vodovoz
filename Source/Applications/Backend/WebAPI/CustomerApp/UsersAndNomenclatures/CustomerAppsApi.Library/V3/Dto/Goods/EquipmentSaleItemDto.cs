using CustomerAppsApi.Library.V3.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V3.Dto.Goods
{
	/// <summary>
	/// Оборудование
	/// </summary>
	public class EquipmentSaleItemDto : SaleItemDto
	{
		/// <summary>
		/// Атрибуты
		/// </summary>
		public EquipmentSaleItemAttributes Attributes { get; set; }
		/// <inheritdoc/>
		public override SaleItemType Type => SaleItemType.Equipment;
	}
}
