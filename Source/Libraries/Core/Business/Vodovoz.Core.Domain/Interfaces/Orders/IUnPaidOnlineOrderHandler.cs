using System.Threading.Tasks;

namespace Vodovoz.Core.Domain.Interfaces.Orders
{
	public interface IUnPaidOnlineOrderHandler
	{
		Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders();
	}
}
