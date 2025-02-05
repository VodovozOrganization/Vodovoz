using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.NotificationSenders
{
	public interface IRouteListTransferHandByHandNotificationSender
	{
		Task<Result> NotifyOfOrderWithGoodsTransferingIsTransfered(int orderId);
	}
}
