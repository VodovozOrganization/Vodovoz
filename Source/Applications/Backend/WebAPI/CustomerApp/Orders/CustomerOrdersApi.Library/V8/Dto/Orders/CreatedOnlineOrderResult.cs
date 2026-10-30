namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Результат создания онлайн заказа
	/// </summary>
	public class CreatedOnlineOrderResult
	{
		public CreatedOnlineOrderResult() { }

		private CreatedOnlineOrderResult(int onlineOrderId, int code, bool isAutoOrderEnabled)
		{
			OnlineOrderId = onlineOrderId;
			Code = code;
			IsAutoOrderEnabled = isAutoOrderEnabled;
		}

		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }

		/// <summary>
		/// Http код
		/// </summary>
		public int Code { get; set; } = 500;

		/// <summary>
		/// Включена настройка автозаказа
		/// </summary>
		public bool IsAutoOrderEnabled { get; set; }

		public static CreatedOnlineOrderResult Create(int onlineOrderId, int code, bool isAutoOrderEnabled = false) =>
			new CreatedOnlineOrderResult(onlineOrderId, code, isAutoOrderEnabled);
	}
}
