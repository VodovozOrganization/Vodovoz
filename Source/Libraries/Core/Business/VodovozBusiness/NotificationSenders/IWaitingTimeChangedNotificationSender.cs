using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	public interface IWaitingTimeChangedNotificationSender
	{
		Task NotifyOfWaitingTimeChanged(int orderId);
	}
}
