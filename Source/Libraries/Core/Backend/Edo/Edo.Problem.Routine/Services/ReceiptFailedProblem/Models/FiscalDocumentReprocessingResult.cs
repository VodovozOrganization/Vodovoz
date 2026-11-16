namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Результат переобработки отдельного фискального документа
	/// </summary>
	public class FiscalDocumentReprocessingResult
	{
		/// <summary>
		/// Успешно завершена фискализация документа
		/// </summary>
		public bool IsSuccess { get; set; }

		/// <summary>
		/// Произошла ошибка при переобработке документа
		/// </summary>
		public bool IsError { get; set; }

		/// <summary>
		/// Количество замененных кодов маркировки
		/// </summary>
		public int ReplacedCodesCount { get; set; }

		/// <summary>
		/// Документ пропущен (в процессе обработки кассой или при сетевых сбоях)
		/// </summary>
		public bool IsSkipped => !IsSuccess && !IsError;

		/// <summary>
		/// Создает результат успешного завершения обработки
		/// </summary>
		/// <param name="replacedCodes">Количество замененных кодов маркировки</param>
		/// <returns>Результат успешного завершения</returns>
		public static FiscalDocumentReprocessingResult Success(int replacedCodes = 0) =>
			new FiscalDocumentReprocessingResult { IsSuccess = true, ReplacedCodesCount = replacedCodes };

		/// <summary>
		/// Создает результат ошибки при обработке документа
		/// </summary>
		/// <returns>Результат ошибки</returns>
		public static FiscalDocumentReprocessingResult Error() =>
			new FiscalDocumentReprocessingResult { IsError = true };

		/// <summary>
		/// Создает результат пропуска (документ в обработке кассой или временный сетевой сбой)
		/// </summary>
		/// <returns>Результат нейтрального пропуска</returns>
		public static FiscalDocumentReprocessingResult Skipped() =>
			new FiscalDocumentReprocessingResult();
	}
}
