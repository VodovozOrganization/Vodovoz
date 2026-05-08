using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Domain.Orders
{
	public interface IDiscount
	{
		bool IsDiscountInMoney { get; }
		IObservableList<DiscountReason> DiscountReasons { get; }
		void SetDiscount(bool isDiscountInMoney, decimal discount, DiscountReason discountReason);
	}
}
