using TaxcomEdo.Contracts.Orders;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Converters
{
	public interface IOrderConverter
	{
		/// <summary>
		/// Конвертация заказа <see cref="Order"/> в информацию о нем для ЭДО <see cref="OrderItemInfoForEdo"/>
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <returns>Информация о заказе для ЭДО</returns>
		OrderInfoForEdo ConvertOrderToOrderInfoForEdo(Order order, DocumentContainerType documentContainerType);
	}
}
