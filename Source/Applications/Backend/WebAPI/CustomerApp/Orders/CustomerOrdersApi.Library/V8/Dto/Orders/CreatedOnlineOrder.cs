namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Информация по созданному онлайн заказу
	/// </summary>
	public class CreatedOnlineOrder
	{
		public CreatedOnlineOrder() { }
		private CreatedOnlineOrder(int onlineOrderId, bool isAutoOrderEnabled)
		{
			OnlineOrderId = onlineOrderId;
			IsAutoOrderEnabled = isAutoOrderEnabled;
		}

		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }

		/// <summary>
		/// Включена настройка автозаказа
		/// </summary>
		public bool IsAutoOrderEnabled { get; set; }

		public static CreatedOnlineOrder Create(CreatedOnlineOrderResult data) =>
			new CreatedOnlineOrder(data.OnlineOrderId, data.IsAutoOrderEnabled);
	}
}
