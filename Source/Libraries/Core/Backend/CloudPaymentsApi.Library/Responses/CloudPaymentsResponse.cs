namespace CloudPaymentsApi.Library.Responses
{
	/// <summary>
	/// Базовый ответ от CloudPayments API
	/// </summary>
	public class CloudPaymentsResponse<T>
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public string ErrorCode { get; set; }
		public T Model { get; set; }
	}
}
