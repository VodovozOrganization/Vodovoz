using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V5.Factories
{
	public interface IInfoMessageFactory
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
