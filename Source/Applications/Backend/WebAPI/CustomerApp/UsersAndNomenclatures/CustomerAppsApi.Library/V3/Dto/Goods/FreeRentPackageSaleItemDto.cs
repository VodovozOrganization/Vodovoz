using CustomerAppsApi.Library.V3.Dto.Goods.Attributes;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V3.Dto.Goods
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
