namespace Vodovoz.Mindbox
{
	/// <summary>
	/// Результат выполнения операции в Mindbox
	/// </summary>
	public sealed class MindboxOperationResult
	{
		/// <summary>
		/// Результат отправки
		/// </summary>
		public MindboxSendOutcome Outcome { get; private set; }

		/// <summary>
		/// Признак успешного выполнения
		/// </summary>
		public bool IsSuccess => Outcome == MindboxSendOutcome.Success;

		/// <summary>
		/// Признак временной ошибки, при которой нужен повтор
		/// </summary>
		public bool IsRetryable => Outcome == MindboxSendOutcome.TransientError;

		/// <summary>
		/// Признак глобальной ошибки, при которой нужно остановить всю очередь
		/// </summary>
		public bool ShouldPauseQueue =>
			Outcome == MindboxSendOutcome.Unauthorized
			|| Outcome == MindboxSendOutcome.NotFound
			|| Outcome == MindboxSendOutcome.RateLimited;

		/// <summary>
		/// HTTP-код ответа
		/// </summary>
		public int HttpStatusCode { get; private set; }

		/// <summary>
		/// Статус операции из ответа Mindbox
		/// </summary>
		public string Status { get; private set; }

		/// <summary>
		/// Текст ошибки
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// Идентификатор ошибки Mindbox
		/// </summary>
		public string ErrorId { get; private set; }

		/// <summary>
		/// Длительность запроса, мс
		/// </summary>
		public long DurationMs { get; private set; }

		/// <summary>
		/// Ответ Mindbox как есть
		/// </summary>
		public string RawResponse { get; private set; }

		public static MindboxOperationResult Create(
			MindboxSendOutcome outcome,
			int httpStatusCode,
			string status,
			string errorMessage,
			string errorId,
			long durationMs,
			string rawResponse)
		{
			return new MindboxOperationResult
			{
				Outcome = outcome,
				HttpStatusCode = httpStatusCode,
				Status = status,
				ErrorMessage = errorMessage,
				ErrorId = errorId,
				DurationMs = durationMs,
				RawResponse = rawResponse
			};
		}

		public static MindboxOperationResult Success(
			int httpStatusCode,
			string status,
			long durationMs,
			string rawResponse) =>
			Create(MindboxSendOutcome.Success, httpStatusCode, status, null, null, durationMs, rawResponse);

		public static MindboxOperationResult Failure(
			MindboxSendOutcome outcome,
			int httpStatusCode,
			string status,
			string errorMessage,
			string errorId,
			long durationMs,
			string rawResponse) =>
			Create(outcome, httpStatusCode, status, errorMessage, errorId, durationMs, rawResponse);

		public static MindboxOperationResult Transient(
			int httpStatusCode,
			string status,
			string errorMessage,
			long durationMs,
			string rawResponse = null) =>
			Create(
				MindboxSendOutcome.TransientError,
				httpStatusCode,
				status,
				errorMessage,
				null,
				durationMs,
				rawResponse);
	}
}
