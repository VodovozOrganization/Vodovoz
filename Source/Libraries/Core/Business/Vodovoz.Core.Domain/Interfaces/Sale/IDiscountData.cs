namespace Vodovoz.Core.Domain.Interfaces.Sale
{
	/// <summary>
	/// Данные по скидке
	/// </summary>
	public interface IDiscountData
	{
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		bool IsDiscountInMoney { get; }
		/// <summary>
		/// Скидка в процентах
		/// </summary>
		decimal Discount { get; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		decimal DiscountMoney { get; }
	}
}
