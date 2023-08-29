using System.Threading.Tasks;

namespace FirebaseCloudMessaging.Client
{
	public interface IFirebaseCloudMessagingService
	{
		Task SendFastDeliveryAddressCanceledNotification(string pushNotificationClientToken, int orderId);
		Task SendPushNotification(string pushNotificationClientToken, string title, string body);
		Task SendWakeUpNotification(string pushNotificationClientToken);
	}
}
