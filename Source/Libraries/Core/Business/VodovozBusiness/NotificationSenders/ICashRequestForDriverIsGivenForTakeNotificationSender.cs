using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	public interface ICashRequestForDriverIsGivenForTakeNotificationSender
	{
		Task NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId);
	}
}
