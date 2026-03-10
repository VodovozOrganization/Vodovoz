using Vodovoz.Core.Data;

namespace CustomerOrdersApi.Library.V4.Factories
{
	public interface IInfoMessageFactoryV4
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
