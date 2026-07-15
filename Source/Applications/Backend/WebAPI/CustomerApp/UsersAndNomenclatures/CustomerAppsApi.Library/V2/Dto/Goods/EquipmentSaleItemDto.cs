using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;

namespace CustomerAppsApi.Library.V2.Dto.Goods
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
