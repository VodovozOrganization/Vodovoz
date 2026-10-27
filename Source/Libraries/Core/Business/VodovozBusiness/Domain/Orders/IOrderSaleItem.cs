namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Позиция заказа
	/// </summary>
	public interface IOrderSaleItem : ISaleItem
	{
		/// <summary>
		/// Фактическое количество
		/// </summary>
		decimal? ActualCount { get; set; }
		/// <summary>
		/// Из недовоза
		/// </summary>
		bool CopiedFromUndelivery { get; }
	}
}
