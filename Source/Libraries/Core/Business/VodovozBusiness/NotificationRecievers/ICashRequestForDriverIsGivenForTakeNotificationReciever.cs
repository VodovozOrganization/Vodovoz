using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface ICashRequestForDriverIsGivenForTakeNotificationSender
	{
		Task NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId);
	}
}