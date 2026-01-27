namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Информация по созданному онлайн заказу
	/// </summary>
	public class CreatedOnlineOrder
	{
		public CreatedOnlineOrder() { }
		private CreatedOnlineOrder((int OnlineOrderId, int Code) data)
		{
			OnlineOrderId = data.OnlineOrderId;
			Code = data.Code;
		}
		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }
		/// <summary>
		/// Http код
		/// </summary>
		public int Code { get; set; } = 500;
		
		public static CreatedOnlineOrder Create((int OnlineOrderId, int Code) data) => new CreatedOnlineOrder(data);
	}
}
