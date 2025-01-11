namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Состояние заказа(структура товаров)
	/// </summary>
	public enum OrderItemsAddRemoveState
	{
		/// <summary>
		/// Не изменилось
		/// </summary>
		None,
		/// <summary>
		/// Добавлен товар
		/// </summary>
		AddedItem,
		/// <summary>
		/// Удален товар
		/// </summary>
		RemovedItem
	}
}
