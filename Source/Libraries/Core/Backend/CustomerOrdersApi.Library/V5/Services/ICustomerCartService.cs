using CustomerOrders.Contracts.V5.Carts;

namespace CustomerOrdersApi.Library.V5.Services
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
	}
}
