using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IDiscountData : IDiscountDataBase
	{
		/// <summary>
		/// Основание скидки
		/// </summary>
		DiscountReason DiscountReason { get; }
	}
}
