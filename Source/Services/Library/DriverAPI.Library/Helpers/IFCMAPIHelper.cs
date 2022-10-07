using System.Threading.Tasks;

namespace DriverAPI.Library.Helpers
{
	public interface IFCMAPIHelper
	{
		Task SendPushNotification(string pushNotificationClientToken, string header, string body);
	}
}