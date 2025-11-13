using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.Errors
{
	/// <summary>
	/// Ошибки проверки времени DriverApi
	/// </summary>
	public static class DateTimeCheckErrors
	{
		/// <summary>
		/// Время указанное в запросе относится к будующему времени
		/// </summary>
		public static Error TooEarly =>
			new Error(typeof(DateTimeCheckErrors),
				nameof(TooEarly),
				"Нельзя отправлять запросы из будущего! Проверьте настройки системного времени вашего телефона");

		/// <summary>
		/// Время указаннгое в запросе - после времмени истечения возможности подачи текущего запроса
		/// </summary>
		public static Error TooLate =>
			new Error(typeof(DateTimeCheckErrors),
				nameof(TooLate),
				"Таймаут запроса операции");
	}
}
