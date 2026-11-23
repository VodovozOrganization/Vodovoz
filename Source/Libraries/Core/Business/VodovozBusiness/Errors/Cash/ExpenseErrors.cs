using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Cash
{
	/// <summary>
	/// Ошибки расходного ордера
	/// </summary>
	public static class ExpenseErrors
	{
		/// <summary>
		/// Ошибка при создании расходного ордера
		/// </summary>
		public static Error CreationFailed =>
			new Error(
				typeof(ExpenseErrors),
				nameof(CreationFailed),
				"При создании расходного ордера возникла ошибка");

		/// <summary>
		/// Создаёт ошибку создания расходного ордера с указанием причины
		/// </summary>
		/// <param name="message">Причина ошибки</param>
		/// <returns>Ошибка с кодом <see cref="CreationFailed"/></returns>
		public static Error CreateCreationFailed(string message) =>
			string.IsNullOrWhiteSpace(message) ? CreationFailed : new Error(
				typeof(ExpenseErrors),
				nameof(CreationFailed),
				$"При создании расходного ордера возникла ошибка: {message}");
	}
}
