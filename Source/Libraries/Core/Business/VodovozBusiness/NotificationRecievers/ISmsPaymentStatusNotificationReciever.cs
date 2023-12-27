using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface ISmsPaymentStatusNotificationReciever
	{
		Task NotifyOfSmsPaymentStatusChanged(int orderId);
	}
}
