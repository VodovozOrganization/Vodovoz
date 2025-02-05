using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	public interface IFastDeliveryOrderAddedNotificationSender
	{
		Task NotifyOfFastDeliveryOrderAdded(int orderId);
	}
}
