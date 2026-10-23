using Vodovoz.Domain.Goods.Rent;

namespace VodovozBusiness.Domain.Orders.Cart
{
	/// <summary>
	/// Интерфейс бесплатного пакета аренды из корзины ИПЗ
	/// </summary>
	public interface IFreeRentPackageCartItem : ICartItem
	{
		/// <summary>
		/// Бесплатный пакет аренды
		/// </summary>
		FreeRentPackage RentPackage { get; }
	}
}
