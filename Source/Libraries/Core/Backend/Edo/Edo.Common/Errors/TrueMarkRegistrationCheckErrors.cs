using Vodovoz.Core.Domain.Results;

namespace Edo.Common.Errors
{
	public static class TrueMarkRegistrationCheckErrors
	{
		/// <summary>
		/// Ошибка при запросе статуса регистрации клиента в Честном Знаке
		/// </summary>
		public static Error ClientTrueMarkRegistrationCheckRequestError =>
			new Error(
				typeof(TrueMarkRegistrationCheckErrors),
				nameof(ClientTrueMarkRegistrationCheckRequestError),
				"Ошибка при запросе статуса регистрации в Честном Знаке");

		/// <summary>
		/// Ошибка при запросе статуса регистрации клиента в Честном Знаке
		/// </summary>
		public static Error CreateClientTrueMarkRegistrationCheckRequestError(string inn, string errorMessage) =>
			new Error(
				typeof(TrueMarkRegistrationCheckErrors),
				nameof(ClientTrueMarkRegistrationCheckRequestError),
				$"Ошибка при запросе статуса регистрации в Честном Знаке для ИНН: {inn}. Сообщение: {errorMessage}");

		/// <summary>
		/// Неизвестное значение статуса регистрации в Честном Знаке
		/// </summary>
		public static Error UnknownRegistrationStatusInTrueMarkError =>
			new Error(
				typeof(TrueMarkRegistrationCheckErrors),
				nameof(UnknownRegistrationStatusInTrueMarkError),
				"Неизвестное значение статуса регистрации в Честном Знаке");

		/// <summary>
		/// Неизвестное значение статуса регистрации в Честном Знаке
		/// </summary>
		public static Error CreateUnknownRegistrationStatusInTrueMarkError(string inn, string status) =>
			new Error(
				typeof(TrueMarkRegistrationCheckErrors),
				nameof(UnknownRegistrationStatusInTrueMarkError),
				$"Неизвестное значение статуса регистрации в Честном Знаке для ИНН: {inn}. Статус: {status}");

		/// <summary>
		/// Неизвестная ошибка при проверке статуса регистрации в Честном Знаке
		/// </summary>
		public static Error ClientTrueMarkRegistrationCheckUnhandledError =>
			new Error(
				typeof(TrueMarkRegistrationCheckErrors),
				nameof(ClientTrueMarkRegistrationCheckUnhandledError),
				"Неизвестная ошибка при проверке статуса регистрации в Честном Знаке");

		/// <summary>
		/// Неизвестная ошибка при проверке статуса регистрации в Честном Знаке
		/// </summary>
		public static Error CreateClientTrueMarkRegistrationCheckUnhandledError(string inn, string errorMessage) =>
			new Error(
				typeof(TrueMarkRegistrationCheckErrors),
				nameof(ClientTrueMarkRegistrationCheckUnhandledError),
				$"Неизвестная ошибка при проверке статуса регистрации в Честном Знаке для ИНН: {inn}. Сообщение: {errorMessage}");
	}
}
