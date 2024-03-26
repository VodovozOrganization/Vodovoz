using System.Threading.Tasks;
using Vodovoz.Errors;

namespace Vodovoz.Application.FirebaseCloudMessaging
{
	public interface IFirebaseCloudMessagingService
	{
		Task<Result> SendFastDeliveryAddressCanceledMessage(string pushNotificationClientToken, int orderId);
		Task<Result> SendMessage(string recipientToken, string title, string body);
		Task<Result> SendWakeUpMessage(string pushNotificationClientToken);
	}
}
