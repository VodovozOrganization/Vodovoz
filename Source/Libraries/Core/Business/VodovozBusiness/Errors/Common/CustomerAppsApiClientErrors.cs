using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Common
{
	/// <summary>
	/// Ошибки апи клиентского приложения
	/// </summary>
	public static class CustomerAppsApiClientErrors
	{
		/// <summary>
		/// Неизвестный клиент
		/// </summary>
		public static Error UnknownCounterparty =>
			new Error(
				typeof(CustomerAppsApiClientErrors),
				nameof(UnknownCounterparty),
				"Неизвестный клиент");

		/// <summary>
		/// Неизвестный источник запроса
		/// </summary>
		public static Error UnsupportedSource =>
			new Error(
				typeof(CustomerAppsApiClientErrors),
				nameof(UnsupportedSource),
				"Неизвестный источник запроса");

		/// <summary>
		/// Неизвестный пользователь
		/// </summary>
		public static Error UnknownUser =>
			new Error(
				typeof(CustomerAppsApiClientErrors),
				nameof(UnknownUser),
				"Неизвестный пользователь");
	}
}
