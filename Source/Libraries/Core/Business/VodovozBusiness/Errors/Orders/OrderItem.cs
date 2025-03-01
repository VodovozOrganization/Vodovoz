namespace Vodovoz.Errors.Orders
{
	public static class OrderItem
	{
		public static Error NotFound =>
			new Error(
				typeof(OrderItem),
				nameof(NotFound),
				"Строка заказа не найдена");
	}
}
