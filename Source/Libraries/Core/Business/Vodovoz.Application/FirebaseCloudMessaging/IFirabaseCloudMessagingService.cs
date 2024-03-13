using Vodovoz.Errors;

namespace Vodovoz.Application.FirebaseCloudMessaging
{
	public interface IFirabaseCloudMessagingService
	{
		Result SendMessage(string recipientToken, string title, string body, object data = null);
	}
}
