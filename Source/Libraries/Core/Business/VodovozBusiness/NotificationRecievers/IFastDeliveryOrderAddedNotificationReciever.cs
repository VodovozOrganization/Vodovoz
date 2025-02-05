using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface IFastDeliveryOrderAddedNotificationSender
	{
		Task NotifyOfFastDeliveryOrderAdded(int orderId);
	}
}
