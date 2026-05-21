using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V6.Services
{
	/// <summary>
	/// Сервис работы с корзиной
	/// </summary>
	public interface ICustomerCartService
	{
		/// <summary>
		/// Проверка корзины
		/// </summary>
		/// <param name="request">Данные заказа из корзины</param>
		/// <returns></returns>
		CheckUsersBasketResponse Check(CheckUsersBasketRequest request);
		/// <summary>
		/// Получение форм оплат и доп условий заказа из корзины
		/// </summary>
		/// <param name="request">Данные запроса <see cref="OrderConditionsRequest"/></param>
		/// <returns>Данные для ответа <see cref="OrderConditionsResponse"/></returns>
		OrderConditionsResponse GetOrderConditions(OrderConditionsRequest request);
	}
}
