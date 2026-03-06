namespace CustomerOrdersApi.Library.Services
{
	/// <summary>
	/// Результат операции переноса заказа
	/// </summary>
	public class TransferOrderResult : OrderOperationResult
	{
		public TransferOrderResult() { }

		public TransferOrderResult(bool success, int httpStatusCode, string title, string message)
			: base(success, httpStatusCode, title, message)
		{
		}
	}
}
