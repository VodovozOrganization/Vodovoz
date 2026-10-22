namespace Vodovoz.Mindbox
{
	/// <summary>
	/// Результат выполнения операции в Mindbox
	/// </summary>
	public sealed class MindboxOperationResult
	{
		private MindboxOperationResult()
		{
		}

		/// <summary>
		/// Признак успешного выполнения
		/// </summary>
		public bool IsSuccess { get; private set; }

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
		/// Ответ Mindbox как есть
		/// </summary>
		public string RawResponse { get; private set; }

		public static MindboxOperationResult Success(int httpStatusCode, string status, string rawResponse) =>
			new MindboxOperationResult
			{
				IsSuccess = true,
				HttpStatusCode = httpStatusCode,
				Status = status,
				RawResponse = rawResponse
			};

		public static MindboxOperationResult Failure(int httpStatusCode, string errorMessage, string rawResponse = null) =>
			new MindboxOperationResult
			{
				IsSuccess = false,
				HttpStatusCode = httpStatusCode,
				ErrorMessage = errorMessage,
				RawResponse = rawResponse
			};
	}
}
