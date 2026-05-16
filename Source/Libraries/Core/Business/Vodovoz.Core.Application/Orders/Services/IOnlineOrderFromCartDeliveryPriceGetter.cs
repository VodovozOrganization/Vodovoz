using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <summary>
	/// Класс получения цены доставки для заказа из корзины
	/// </summary>
	public interface IOnlineOrderFromCartDeliveryPriceGetter
	{
		/// <summary>
		/// Получение цены доставки
		/// </summary>
		/// <param name="onlineOrder">Данные заказа</param>
		/// <returns>
		/// <see cref="Result{TValue}"/>
		/// в случае успеха, возвращается цена доставки, упакованная в Result
		/// в случае провала - ошибка, упакованная в Result
		/// </returns>
		Result<decimal> GetDeliveryPrice(IOnlineOrderFromCart onlineOrder);
	}
}
