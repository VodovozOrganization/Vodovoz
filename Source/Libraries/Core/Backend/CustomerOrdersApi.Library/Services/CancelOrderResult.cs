namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Результат операции отмены заказа
	/// </summary>
	public class CancelOrderResult : OrderOperationResult
	{
		public CancelOrderResult() { }

		public CancelOrderResult(bool success, int httpStatusCode, string title, string message)
			: base(success, httpStatusCode, title, message)
		{
		}
	}
}
