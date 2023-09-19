namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права заказы
	/// </summary>
	public static partial class Order
	{
		public static string CanActivateClientsSecondOrderDiscount => "can_activate_clients_second_order_discount";

		/// <summary>
		/// Пользователь может формировать заказ для ликвидированного контрагента
		/// </summary>
		public static string CanFormOrderWithLiquidatedCounterparty =>
			"can_form_order_with_liquidated_counterparty";
	}
}
