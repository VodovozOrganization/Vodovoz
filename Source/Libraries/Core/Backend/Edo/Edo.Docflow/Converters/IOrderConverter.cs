using TaxcomEdo.Contracts.Orders;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Docflow.Converters
{
	public interface IOrderConverter
	{
		/// <summary>
		/// Конвертация заказа <see cref="OrderEntity"/> в информацию о нем для ЭДО <see cref="OrderItemInfoForEdo"/>
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <returns>Информация о заказе для ЭДО</returns>
		OrderInfoForEdo ConvertOrderToOrderInfoForEdo(OrderEntity order);
	}
}
