using CustomerApp.Contracts.Sale;
using Vodovoz.Domain.Goods.Rent;

namespace VodovozBusiness.Domain.Orders.Cart
{
	/// <summary>
	/// Интерфейс бесплатного пакета аренды из корзины ИПЗ
	/// </summary>
	public interface IFreeRentPackageCartItem : ICartItemBase
	{
		/// <summary>
		/// Бесплатный пакет аренды
		/// </summary>
		FreeRentPackage RentPackage { get; }
	}
}
