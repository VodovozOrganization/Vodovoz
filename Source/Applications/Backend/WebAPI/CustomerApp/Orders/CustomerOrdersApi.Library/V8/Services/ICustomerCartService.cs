using System.Threading.Tasks;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Сервис работы с корзиной
	/// </summary>
	public interface ICustomerCartService
	{
		/// <summary>
		/// Получение форм оплат и доп условий заказа из корзины
		/// </summary>
		/// <param name="request">Данные запроса <see cref="OrderConditionsRequest"/></param>
		/// <returns>Данные для ответа <see cref="OrderConditionsResponse"/></returns>
		Task<OrderConditionsResponse> GetOrderConditionsAsync(OrderConditionsRequest request);
	}
}
