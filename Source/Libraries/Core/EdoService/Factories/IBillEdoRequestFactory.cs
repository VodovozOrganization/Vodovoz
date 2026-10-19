using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace EdoService.Library.Factories
{
	public interface IBillEdoRequestFactory
	{
		/// <summary>
		/// Создает заявку счета из сохраненного заказа
		/// </summary>
		/// <param name="order">Сохраненный заказ</param>
		/// <param name="type">Обычный или особый счет</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		InformalEdoRequest Create(Order order, OrderDocumentType type, byte[] pdf);

		/// <summary>
		/// Создает заявку сохраненного счета без отгрузки
		/// </summary>
		/// <param name="bill">Счет на долг, предоплату или постоплату</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		InformalEdoRequest Create(OrderWithoutShipmentBase bill, byte[] pdf);
	}
}
