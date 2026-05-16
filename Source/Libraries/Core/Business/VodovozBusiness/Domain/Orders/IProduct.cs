using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Более детальный контракт товара
	/// </summary>
	public interface IProduct : IGoods
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
	}
}
