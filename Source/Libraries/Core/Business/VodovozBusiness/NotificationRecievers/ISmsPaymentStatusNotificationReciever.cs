using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface ISmsPaymentStatusNotificationSender
	{
		Task NotifyOfSmsPaymentStatusChanged(int orderId);
	}
}
