using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	public interface ISmsPaymentStatusNotificationSender
	{
		Task NotifyOfSmsPaymentStatusChanged(int orderId);
	}
}
