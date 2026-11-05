namespace CustomerOrders.Contracts.V8.Orders.Items
{
	public abstract class OnlineOrderItemWithGiftBaseDto : OnlineOrderItemBaseDto
	{
		/// <summary>
		/// Подарок
		/// </summary>
		public bool GiftItem { get; set; }
	}
}
