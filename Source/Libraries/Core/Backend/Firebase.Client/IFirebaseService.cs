using System.Threading.Tasks;

namespace Firebase.Client
{
	public interface IFirebaseService
	{
		Task SendFastDeliveryAddressCanceledNotification(string pushNotificationClientToken, int orderId);
		Task SendPushNotification(string pushNotificationClientToken, string title, string body);
		Task SendWakeUpNotification(string pushNotificationClientToken);
	}
}
