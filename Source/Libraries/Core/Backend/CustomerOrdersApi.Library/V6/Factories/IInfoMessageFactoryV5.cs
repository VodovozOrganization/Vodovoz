using CustomerOrders.Contracts.InfoMessages;

namespace CustomerOrdersApi.Library.V6.Factories
{
	public interface IInfoMessageFactoryV5
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
