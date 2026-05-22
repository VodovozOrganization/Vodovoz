namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Общий контракт товара
	/// </summary>
	public interface IGoodsWithManyDiscounts : ICalculatingPriceWithManyDiscounts
	{
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; }
	}
}
