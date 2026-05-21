using CustomerOrders.Contracts.InfoMessages;

namespace CustomerOrdersApi.Library.V4.Factories
{
	public interface IInfoMessageFactoryV4
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
