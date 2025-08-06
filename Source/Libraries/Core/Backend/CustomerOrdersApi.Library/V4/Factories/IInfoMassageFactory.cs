using Vodovoz.Core.Data.InfoMessages;

namespace CustomerOrdersApi.Library.V4.Factories
{
	public interface IInfoMassageFactory
	{
		InfoMessage CreateNeedPayOrderInfoMessage();
		InfoMessage CreateNotPaidOrderInfoMessage();
	}
}
