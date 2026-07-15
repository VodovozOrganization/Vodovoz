using CustomerAppsApi.Library.V2.Dto.Goods.Attributes;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Бесплатный пакет аренды
	/// </summary>
	public class FreeRentPackageSaleItemDto : SaleItemDto
	{
		/// <summary>
		/// Атрибуты
		/// </summary>
		public FreeRentPackageAttributes Attributes { get; set; }
		/// <inheritdoc/>
		public override SaleItemType Type => SaleItemType.RentPackage;
	}
}
