using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Errors
{
	/// <summary>
	/// Ошибки проверки номера телефона DriverApi
	/// </summary>
	public static class PhoneNumberErrors
	{
		/// <summary>
		/// Номер телефона не соответствует формату
		/// </summary>
		public static Error InvalidFormat =>
			new Error(typeof(PhoneNumberErrors),
				nameof(InvalidFormat),
				"Номер телефона не соответствует формату");

		/// <summary>
		/// Создает ошибку, что номер телефона не соответствует формату
		/// </summary>
		/// <param name="phoneNumber">Номер телефона</param>
		/// <param name="formatMessage">Сообщение о формате</param>
		/// <returns>Ошибка</returns>
		public static Error CreateInvalidFormat(string phoneNumber, string formatMessage) =>
			new Error(typeof(PhoneNumberErrors),
				nameof(InvalidFormat),
				$"Номер телефона {phoneNumber} должен соответствовать формату: {formatMessage}");
	}
}
