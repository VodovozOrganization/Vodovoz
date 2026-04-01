namespace CloudPaymentsApi.Library.Responses
{
	/// <summary>
	/// Базовый ответ от CloudPayments API
	/// </summary>
	public class CloudPaymentsResponse<T>
	{
		/// <summary>
		/// Успешен ли
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Сообщение об ошибке, если запрос не был успешным
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Код ошибки
		/// </summary>
		public string ErrorCode { get; set; }

		/// <summary>
		/// Модель данных, возвращаемая при успешном запросе
		/// </summary>
		public T Model { get; set; }
	}
}
