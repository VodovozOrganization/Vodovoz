namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Общий контракт товара
	/// </summary>
	public interface IGoods : ICalculatingPrice
	{
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; }
	}
}
