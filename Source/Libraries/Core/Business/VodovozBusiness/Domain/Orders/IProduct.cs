using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	public interface IProduct : ISaleItem
	{
		/// <summary>
		/// Id сущности
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Скидка
		/// </summary>
		decimal GetDiscount { get; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		bool IsDiscountInMoney { get; }
		/// <summary>
		/// Фактическая сумма
		/// </summary>
		decimal ActualSum { get; }
		/// <summary>
		/// Текущее количество
		/// </summary>
		decimal CurrentCount { get; }
		/// <summary>
		/// Этот товар - подарок?
		/// </summary>
		bool GiftItem { get; }
	}
}
