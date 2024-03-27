using System.Threading.Tasks;

namespace Vodovoz.NotificationRecievers
{
	public interface ICashRequestForDriverIsGivenForTakeNotificationReciever
	{
		Task NotifyOfCashRequestForDriverIsGivenForTake(int cashRequestId);
	}
}