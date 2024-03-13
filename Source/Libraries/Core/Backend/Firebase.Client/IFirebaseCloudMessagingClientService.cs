using System.Threading.Tasks;

namespace FirebaseCloudMessaging.Client
{
	public interface IFirebaseCloudMessagingClientService
	{
		Task SendMessage(string recipientToken, string title, string body, object data = null);
	}
}
