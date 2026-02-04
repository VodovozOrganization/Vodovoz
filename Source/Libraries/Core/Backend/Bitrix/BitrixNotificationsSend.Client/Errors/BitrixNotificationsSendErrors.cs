using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Client.Errors
{
	public static class BitrixNotificationsSendErrors
	{
		/// <summary>
		/// Ошибка при отправке уведомления о долгах по безналу клиентов в Битрикс24
		/// </summary>
		public static Error SendCounterpartiesCashlessDebtsNotificationError =>
			new Error(
				typeof(BitrixNotificationsSendErrors),
				nameof(SendCounterpartiesCashlessDebtsNotificationError),
				"Ошибка при отправке уведомления о долгах по безналу клиентов в Битрикс24");

		/// <summary>
		/// Ошибка при отправке уведомления о долгах по безналу клиентов в Битрикс24
		/// </summary>
		public static Error CreateSendCounterpartiesCashlessDebtsNotificationError(string message) =>
			new Error(
				typeof(BitrixNotificationsSendErrors),
				nameof(CreateSendCounterpartiesCashlessDebtsNotificationError),
				$"Ошибка при отправке уведомления о долгах по безналу клиентов в Битрикс24: {message}");
	}
}
