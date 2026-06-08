using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Validation
{
	public interface IFastDeliveryValidator
	{
		/// <summary>
		/// Валидация заказа на возможность доставки в рамках экспресс-доставки
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <param name="isSkipOrderDeliveryDateCheck">Пропустить проверку даты доставки заказа</param>
		/// <returns>Результат проверки</returns>
		Result ValidateOrder(Order order, bool isSkipOrderDeliveryDateCheck = false);
	}
}
