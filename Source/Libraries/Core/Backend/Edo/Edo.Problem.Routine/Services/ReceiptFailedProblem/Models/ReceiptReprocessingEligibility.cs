namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Результат проверки допустимости автоматической повторной фискализации чека
	/// </summary>
	public class ReceiptReprocessingEligibility
	{
		/// <summary>
		/// Допустима ли автоматическая переотправка чека
		/// </summary>
		public bool IsEligible { get; }

		/// <summary>
		/// Причина блокировки автоматической переотправки (если заблокирована)
		/// </summary>
		public string Reason { get; }

		private ReceiptReprocessingEligibility(bool isEligible, string reason = null)
		{
			IsEligible = isEligible;
			Reason = reason;
		}

		/// <summary>
		/// Создает результат, разрешающий переотправку
		/// </summary>
		public static ReceiptReprocessingEligibility Allowed() =>
			new ReceiptReprocessingEligibility(true);

		/// <summary>
		/// Создает результат, блокирующий переотправку
		/// </summary>
		public static ReceiptReprocessingEligibility Blocked(string reason) =>
			new ReceiptReprocessingEligibility(false, reason);
	}
}
