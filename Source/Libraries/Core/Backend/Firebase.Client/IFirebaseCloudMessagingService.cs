using System.Threading.Tasks;

namespace FirebaseCloudMessaging.Client
{
	public interface IFirebaseCloudMessagingService
	{
		Task SendFastDeliveryAddressCanceledMessage(string pushNotificationClientToken, int orderId);
		Task SendMessage(string pushNotificationClientToken, string title, string body);
		Task SendWakeUpMessage(string pushNotificationClientToken);
	}
}
