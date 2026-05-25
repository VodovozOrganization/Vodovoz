using System.Threading.Tasks;
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
		Task<CheckUsersBasketResponse> CheckAsync(CheckUsersBasketRequest request);
		/// <summary>
		/// Получение форм оплат и доп условий заказа из корзины
		/// </summary>
		/// <param name="request">Данные запроса <see cref="OrderConditionsRequest"/></param>
		/// <returns>Данные для ответа <see cref="OrderConditionsResponse"/></returns>
		Task<OrderConditionsResponse> GetOrderConditionsAsync(OrderConditionsRequest request);
	}
}
