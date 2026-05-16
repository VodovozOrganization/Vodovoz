using CustomerOrders.Contracts.V5.Orders.Discounts;

namespace CustomerOrders.Contracts.V5.Orders.PromoCodes
{
	/// <summary>
	/// Результат проверки доступности скидки на первый заказ для клиента
	/// </summary>
	public class FirstOrderDiscountConditionsDto
	{
		/// <summary>
		/// Указывает доступна ли скидка на первый заказ для клиента
		/// </summary>
		public bool DiscountIsAvailable { get; set; }

		/// <summary>
		/// Информация о скидке
		/// </summary>
		public DiscountDto Discount { get; set; }
	}
}
