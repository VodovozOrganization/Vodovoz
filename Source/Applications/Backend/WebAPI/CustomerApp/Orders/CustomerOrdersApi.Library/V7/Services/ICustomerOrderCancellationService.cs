using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Services
{
	public interface ICustomerOrderCancellationService
	{
		/// <summary>
		/// Проверяет, можно ли отменить заказ в текущем статусе
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="order">Заказ для проверки</param>
		/// <param name="onlineOrder">Онлайн заказ, связанный с заказом</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Результат проверки:
		/// Success - отмена возможна;
		/// Failure с соответствующей ошибкой - отмена невозможна
		/// </returns>
		Task<Result> CanCancel(IUnitOfWork uow, Order order, OnlineOrder onlineOrder, CancellationToken cancellationToken);

		/// <summary>
		/// Применяет отмену заказа по его внешнему идентификатору
		/// </summary>
		/// <param name="externalOrderId">Внешний идентификатор заказа</param>
		/// <param name="source">Источник запроса</param>
		/// <param name="transactionId">Идентификатор транзакции в платежной системе</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Результат проверки:
		/// Success - отмена возможна;
		/// Failure с соответствующей ошибкой - отмена невозможна
		/// </returns>
		Task<Result<string>> ApplyCancellationAsync(
			Source source,
			int counterpartyId,
			int? orderId,
			int? onlineOrderId,
			CancellationToken cancellationToken);
	}
}
