namespace CustomerOrdersApi.Library.V8.Dto.Orders
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

		/// <summary>
		/// Списано бонусов
		/// </summary>
		public int BonusesWrittenOff { get; set; }

		/// <summary>
		/// Бонусов к начислению
		/// </summary>
		public int? BonusesToEarn { get; set; }

		public static CreatedOnlineOrder Create(CreatedOnlineOrderResult data) => new CreatedOnlineOrder(data.OnlineOrderId)
		{
			BonusesWrittenOff = data.BonusesWrittenOff,
			BonusesToEarn = data.BonusesToEarn
		};
	}
}
