using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using System.Threading;
using System.Threading.Tasks;

namespace EdoService.Library.Factories
{
	/// <summary>
	/// Создает заявки со снимком реквизитов счета и сохраняет PDF в файловом хранилище.
	/// </summary>
	public interface IBillEdoRequestFactory
	{
		/// <summary>
		/// Создает заявку счета из сохраненного заказа
		/// </summary>
		/// <param name="order">Сохраненный заказ</param>
		/// <param name="type">Обычный или особый счет</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		Task<InformalEdoRequest> CreateAsync(Order order, OrderDocumentType type, byte[] pdf, CancellationToken cancellationToken = default);

		/// <summary>
		/// Создает заявку сохраненного счета без отгрузки
		/// </summary>
		/// <param name="bill">Счет на долг, предоплату или постоплату</param>
		/// <param name="pdf">PDF сохраненного счета без подписи и печати</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Новая заявка со снимком отправляемого счета</returns>
		Task<InformalEdoRequest> CreateAsync(OrderWithoutShipmentBase bill, byte[] pdf, CancellationToken cancellationToken = default);
	}
}
