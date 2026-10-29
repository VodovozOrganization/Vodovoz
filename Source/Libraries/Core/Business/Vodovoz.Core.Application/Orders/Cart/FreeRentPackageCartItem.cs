using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Cart
{
	public class FreeRentPackageCartItem : IFreeRentPackageCartItem
	{
		/// <inheritdoc/>
		public FreeRentPackage RentPackage { get; set; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
		/// <inheritdoc/>
		public SaleItemType ItemType => SaleItemType.RentPackage;
	}
}
