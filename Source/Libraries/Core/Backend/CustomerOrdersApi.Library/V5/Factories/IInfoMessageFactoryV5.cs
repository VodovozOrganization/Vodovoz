using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public interface IInfoMessageFactoryV5
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
