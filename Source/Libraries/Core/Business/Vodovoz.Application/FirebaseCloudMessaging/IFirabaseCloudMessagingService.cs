using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.Application.FirebaseCloudMessaging
{
	public interface IFirabaseCloudMessagingService
	{
		Task<Result> SendFastDeliveryAddressCanceledMessage(string pushNotificationClientToken, int orderId);
		Task<Result> SendMessage(string recipientToken, string title, string body, object data = null);
		Task<Result> SendWakeUpMessage(string pushNotificationClientToken);
	}
}
