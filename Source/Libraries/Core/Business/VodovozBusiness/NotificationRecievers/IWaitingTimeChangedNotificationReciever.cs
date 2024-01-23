using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface IWaitingTimeChangedNotificationReciever
	{
		Task NotifyOfWaitingTimeChanged(int orderId);
	}
}
