namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Результат выполнения операции переобработки чеков с ошибками фискализации
	/// </summary>
	public class ReprocessingExecutionResult
	{
		/// <summary>
		/// Общее количество найденных фискальных документов со статусом Failed для обработки
		/// </summary>
		public int TotalFound { get; set; }

		/// <summary>
		/// Количество успешно обработанных и отправленных фискальных документов
		/// </summary>
		public int SuccessCount { get; set; }

		/// <summary>
		/// Общее количество замененных невалидных кодов маркировки
		/// </summary>
		public int ReplacedCodesCount { get; set; }

		/// <summary>
		/// Количество пропущенных документов из-за закрытого бухгалтерского периода
		/// </summary>
		public int SkippedClosedPeriodCount { get; set; }

		/// <summary>
		/// Количество документов, при обработке которых произошли ошибки
		/// </summary>
		public int ErrorCount { get; set; }

		/// <summary>
		/// Информационное сообщение о результате выполнения операции
		/// </summary>
		public string Message { get; set; }
	}
}
