using TaxcomEdo.Contracts.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Converters
{
	public interface IOrderItemConverter
	{
		/// <summary>
		/// Конвертация строки заказа <see cref="OrderItem"/> в информацию о ней для ЭДО <see cref="OrderItemInfoForEdo"/>
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>Информация о строке заказа для ЭДО</returns>
		OrderItemInfoForEdo ConvertOrderItemToOrderItemInfoForEdo(OrderItem orderItem);
	}
}
