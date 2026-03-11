using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V4.Factories
{
	public interface IInfoMessageFactory
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
