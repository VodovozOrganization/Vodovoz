namespace Vodovoz.Core.Domain.Orders.OnlineOrders
{
	/// <summary>
	/// Версия онлайн заказа
	/// </summary>
	public enum OnlineOrderVersion
	{
		/// <summary>
		/// Первая версия
		/// </summary>
		V1,
		/// <summary>
		/// Вторая версия(после консолидации позиций, когда промики передаются одной строкой, вместо позиций)
		/// </summary>
		V2
	}
}
