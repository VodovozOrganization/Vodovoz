using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.CashReceipts
{
	public static class CashReceiptErrors
	{
		/// <summary>
		/// Должен быть указан валидный код чека
		/// </summary>
		public static Error CashReceiptIdNotValid =>
			new Error(
				typeof(CashReceiptErrors),
				nameof(CashReceiptIdNotValid),
				"Должен быть указан валидный код чека");

		/// <summary>
		/// Служба кассовых чеков вернула результат с ошибкой
		/// </summary>
		public static Error CashReceiptApiRequestProcessingError =>
			new Error(
				typeof(CashReceiptErrors),
				nameof(CashReceiptApiRequestProcessingError),
				"Служба кассовых чеков вернула результат с ошибкой");

		/// <summary>
		/// Служба кассовых чеков вернула результат с ошибкой
		/// </summary>
		/// <param name="errorMessage">Детали ошибки</param>
		/// <returns></returns>
		public static Error CreateCashReceiptApiRequestProcessingError(string errorMessage) =>
			new Error(
				typeof(CashReceiptErrors),
				nameof(CashReceiptApiRequestProcessingError),
				$"Служба кассовых чеков вернула результат с ошибкой:\n{errorMessage}");

		/// <summary>
		/// Сервис обработки кассовых чеков недоступен
		/// </summary>
		public static Error CashReceiptApiServiceUnavailableError =>
			new Error(
				typeof(CashReceiptErrors),
				nameof(CashReceiptApiRequestProcessingError),
				"Сервис обработки кассовых чеков недоступен");

		/// <summary>
		/// Ошибка аутентификации при обращении к сервису кассовых чеков
		/// </summary>
		public static Error CashReceiptApiUnauthenticatedError =>
			new Error(
				typeof(CashReceiptErrors),
				nameof(CashReceiptApiUnauthenticatedError),
				"Ошибка аутентификации при обращении к сервису кассовых чеков");
	}
}
