namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Итоговый результат выполнения пакета переобработки ошибочных чеков
	/// </summary>
	public class ReprocessingExecutionResult
	{
		/// <summary>
		/// Всего найдено чеков со статусом Failed
		/// </summary>
		public int TotalFound { get; set; }

		/// <summary>
		/// Успешно отправлено или подтверждено фискализаций
		/// </summary>
		public int SuccessCount { get; set; }

		/// <summary>
		/// Ошибок при обработке
		/// </summary>
		public int ErrorCount { get; set; }

		/// <summary>
		/// Пропущено документов (в процессе обработки кассой или временный сбой)
		/// </summary>
		public int SkippedCount { get; set; }

		/// <summary>
		/// Количество замененных кодов маркировки
		/// </summary>
		public int ReplacedCodesCount { get; set; }
	}
}
