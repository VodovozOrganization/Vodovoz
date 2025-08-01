using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Common
{
	/// <summary>
	/// Ошибки апи клиентского приложения
	/// </summary>
	public static class CustomerAppsApiClient
	{
		/// <summary>
		/// Неизвестный клиент
		/// </summary>
		public static Error UnknownCounterparty =>
			new Error(
				typeof(CustomerAppsApiClient),
				nameof(UnknownCounterparty),
				"Неизвестный клиент");

		/// <summary>
		/// Неизвестный источник запроса
		/// </summary>
		public static Error UnsupportedSource =>
			new Error(
				typeof(CustomerAppsApiClient),
				nameof(UnsupportedSource),
				"Неизвестный источник запроса");

		/// <summary>
		/// Неизвестный пользователь
		/// </summary>
		public static Error UnknownUser =>
			new Error(
				typeof(CustomerAppsApiClient),
				nameof(UnknownUser),
				"Неизвестный пользователь");
	}
}
