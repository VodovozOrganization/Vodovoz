namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	/// <summary>
	/// Данные строки сводного уведомления об ошибках отправки в Такском.
	/// </summary>
	public class TaxcomSendProblemNotificationData
	{
		/// <summary>
		/// Описание заказа или задачи трансфера.
		/// </summary>
		public string TaskDescription { get; }

		/// <summary>
		/// Идентификатор документа для уведомления.
		/// </summary>
		public string MainDocumentId { get; }

		/// <summary>
		/// Сообщение об ошибке отправки.
		/// </summary>
		public string ErrorMessage { get; }

		/// <summary>
		/// Идентификатор проблемы ЭДО.
		/// </summary>
		public int ProblemId { get; }

		/// <summary>
		/// Количество опубликованных повторных попыток.
		/// </summary>
		public int RetryCount { get; }

		/// <summary>
		/// Создаёт данные строки уведомления.
		/// </summary>
		/// <param name="taskDescription">Описание заказа или задачи трансфера.</param>
		/// <param name="mainDocumentId">Идентификатор документа.</param>
		/// <param name="errorMessage">Сообщение об ошибке.</param>
		/// <param name="problemId">Идентификатор проблемы ЭДО.</param>
		/// <param name="retryCount">Количество повторных попыток.</param>
		public TaxcomSendProblemNotificationData(
			string taskDescription,
			string mainDocumentId,
			string errorMessage,
			int problemId,
			int retryCount)
		{
			TaskDescription = taskDescription;
			MainDocumentId = mainDocumentId;
			ErrorMessage = errorMessage;
			ProblemId = problemId;
			RetryCount = retryCount;
		}
	}
}
