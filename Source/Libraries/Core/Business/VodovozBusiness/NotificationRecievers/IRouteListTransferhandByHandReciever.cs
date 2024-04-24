using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface IRouteListTransferhandByHandReciever
	{
		Task NotifyOfOrderWithGoodsTransferingIsTransfered(int orderId);
	}
}
