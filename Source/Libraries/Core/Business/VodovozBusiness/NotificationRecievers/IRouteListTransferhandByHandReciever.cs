using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.NotificationRecievers
{
	public interface IRouteListTransferhandByHandReciever
	{
		Task<Result> NotifyOfOrderWithGoodsTransferingIsTransfered(int orderId);
	}
}
