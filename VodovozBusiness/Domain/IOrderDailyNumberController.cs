using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain
{
	public interface IOrderDailyNumberController
	{
		void UpdateDailyNumber(Order order);
	}
}
