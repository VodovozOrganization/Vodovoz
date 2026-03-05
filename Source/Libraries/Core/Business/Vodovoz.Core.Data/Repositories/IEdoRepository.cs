using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IEdoRepository
	{
		Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken);
		Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken);
		Task<IEnumerable<GroupGtinEntity>> GetGroupGtinsAsync(CancellationToken cancellationToken);
		Task<bool> HasReceiptOnSumToday(decimal sum, CancellationToken cancellationToken);

		/// <summary>
		/// Получает ЭДО документы заказа по идентификатору заказа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderId"></param>
		/// <returns></returns>
		IEnumerable<OrderEdoDocument> GetOrderEdoDocumentsByOrderId(IUnitOfWork uow, int orderId);
		IEnumerable<OrderEdoTask> GetEdoTaskByOrderAsync(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает задачи по ЭДО, которые связаны с клиентами, подключенными к системе TrueMark, и по которым истекло время ожидания ответа от клиента
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="timeoutDays">Таймаут</param>
		/// <param name="searchMode">Режим поиска</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Заказ с задачами</returns>
		Task<IList<TimedOutOrderDocumentTaskNode>> GetTrueMarkConnectedClientsTimedOutOrderDocumentTasks(IUnitOfWork uow, int timeoutDays, TimedOutDocumentTasksSearchMode searchMode, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает номера заказов, по которым уже созданы заявки на вывод кодов из оборота
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderIds">Номера заказов для проверки</param>
		/// <returns>Номера заказов, по которым существуют заявки на вывод кодов из оборота</returns>
		Task<IList<int>> GetExistingWithdrawalEdoRequestOrders(IUnitOfWork uow, IEnumerable<int> orderIds, CancellationToken cancellationToken);
	}
}
