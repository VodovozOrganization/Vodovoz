using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.NotificationRecievers
{
	public interface IRouteListTransferHandByHandNotificationSender
	{
		Task<Result> NotifyOfOrderWithGoodsTransferingIsTransfered(int orderId);
	}
}
