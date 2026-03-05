using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public interface IInfoMessageFactoryV5
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
		InfoMessage CreateAutoOrderDiscountInfoMessage(decimal discount, DiscountUnits units);
	}
}
