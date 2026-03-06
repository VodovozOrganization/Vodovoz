namespace CustomerOrdersApi.Library.Services
{
	public abstract class OrderOperationResult
	{
		/// <summary>
		/// Признак успешного выполнения операции
		/// </summary>
		public bool IsSuccess { get; set; }

		/// <summary>
		/// HTTP статус код
		/// </summary>
		public int StatusCode { get; set; }

		/// <summary>
		/// Заголовок сообщения
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Подробное сообщение об ошибке или успехе
		/// </summary>
		public string DetailMessage { get; set; }

		protected OrderOperationResult() { }

		protected OrderOperationResult(bool isSuccess, int statusCode, string title, string detailMessage)
		{
			IsSuccess = isSuccess;
			StatusCode = statusCode;
			Title = title;
			DetailMessage = detailMessage;
		}
	}
}
