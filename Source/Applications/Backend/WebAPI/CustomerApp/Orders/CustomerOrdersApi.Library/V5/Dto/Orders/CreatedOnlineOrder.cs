namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Информация по созданному онлайн заказу
	/// </summary>
	public class CreatedOnlineOrder
	{
		public CreatedOnlineOrder() { }
		private CreatedOnlineOrder(int onlineOrderId)
		{
			OnlineOrderId = onlineOrderId;
		}
		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }
		public static CreatedOnlineOrder Create(CreatedOnlineOrderResult data) => new CreatedOnlineOrder(data.OnlineOrderId);
	}
}
