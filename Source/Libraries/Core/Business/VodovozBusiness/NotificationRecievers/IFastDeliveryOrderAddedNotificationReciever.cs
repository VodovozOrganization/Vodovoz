using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface IFastDeliveryOrderAddedNotificationReciever
	{
		Task NotifyOfFastDeliveryOrderAdded(int orderId);
	}
}
