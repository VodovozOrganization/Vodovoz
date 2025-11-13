using Vodovoz.Core.Domain.Results;

namespace Vodovoz.FirebaseCloudMessaging
{
	public static class FirebaseCloudMessagingServiceErrors
	{
		public static Error SendingError =>
			new Error(
				typeof(FirebaseCloudMessagingServiceErrors),
				nameof(SendingError),
				"Ошибка отправки PUSH-сообщения");

		public static Error Unregistered =>
			new Error(
				typeof(FirebaseCloudMessagingServiceErrors),
				nameof(Unregistered),
				"Токен не зарегистрирован");
	}
}
