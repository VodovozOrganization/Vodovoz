namespace Vodovoz.Core.Domain.Interfaces
{
	/// <summary>
	/// Значение скидки
	/// </summary>
	public interface IDiscountValue
	{
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		bool IsDiscountMoney { get; }
		/// <summary>
		/// Скидка в процентах
		/// </summary>
		decimal Discount { get; }
		/// <summary>
		/// Скидка в денежном эквиваленте
		/// </summary>
		decimal DiscountMoney { get; }
	}
}
