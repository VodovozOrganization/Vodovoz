using Vodovoz.Errors;

namespace Vodovoz.FirebaseCloudMessaging
{
	public static class FirebaseCloudMessagingServiceErrors
	{
		public static Error SendingError =>
			new Error(
				typeof(FirebaseCloudMessagingServiceErrors),
				nameof(SendingError),
				"Ошибка отправки PUSH-сообщения");
	}
}
