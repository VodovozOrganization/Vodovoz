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
		public string BlockReason { get; }

		private ReceiptReprocessingEligibility(bool isEligible, string blockReason = null)
		{
			IsEligible = isEligible;
			BlockReason = blockReason;
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
