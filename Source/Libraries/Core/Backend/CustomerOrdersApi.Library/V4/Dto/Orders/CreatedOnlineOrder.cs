﻿namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Информация по созданному онлайн заказу
	/// </summary>
	public class CreatedOnlineOrder
	{
		public CreatedOnlineOrder() { }
		private CreatedOnlineOrder(int onlineOrderId) => OnlineOrderId = onlineOrderId;
		
		/// <summary>
		/// Id онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }
		
		public static CreatedOnlineOrder Create(int onlineOrderId) => new CreatedOnlineOrder(onlineOrderId);
	}
}
