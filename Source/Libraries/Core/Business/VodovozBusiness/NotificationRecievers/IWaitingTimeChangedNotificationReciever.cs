using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface IWaitingTimeChangedNotificationSender
	{
		Task NotifyOfWaitingTimeChanged(int orderId);
	}
}
