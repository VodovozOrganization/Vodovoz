namespace CustomerOrders.Contracts.Interfaces
{
	public interface IDiscountDataBase
	{
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		bool IsDiscountInMoney { get; }
		/// <summary>
		/// Скидка
		/// </summary>
		decimal Discount { get; }
	}
}
