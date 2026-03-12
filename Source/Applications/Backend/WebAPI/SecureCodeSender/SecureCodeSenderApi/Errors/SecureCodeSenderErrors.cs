using Vodovoz.Core.Domain.Results;

namespace SecureCodeSenderApi.Errors
{
	public static class SecureCodeSenderErrors
	{
		public static Error CodeMaxSentAttemptsExceeded(string source) =>
			new Error("422", $"Превышено максимальное количество отправок в {source}");

		public static Error TelegramCheckSendAbilityFailed(bool onlyOtherMethod = false)
		{
			const string onlyOtherMethodMessage = "Не удалось проверить доступность отправки в Телеграм. Пожалуйста, используйте другой метод отправки";
			
			return new Error(
				"422",
				onlyOtherMethod ? onlyOtherMethodMessage : onlyOtherMethodMessage + " или попробуйте позднее");
		}

		public static Error SentFailed() =>
			new Error(
				"422",
				"Не удалось отправить код в Телеграм. Пожалуйста, используйте другой метод отправки или попробуйте позднее");
	}
}
