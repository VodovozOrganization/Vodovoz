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
		IEnumerable<OrderEdoTask> GetEdoTaskByOrder(IUnitOfWork uow, int orderId);
		IEnumerable<OrderEdoTaskNode> GetEdoTasksForOrder(IUnitOfWork uow, int orderId);
		IEnumerable<EdoDocflowForOrderNode> GetEdoDocflowsForOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает задачи по ЭДО, которые связаны с клиентами, подключенными к системе TrueMark, и по которым истекло время ожидания ответа от клиента
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="timeoutDays">Таймаут</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Заказ с задачами</returns>
		Task<IList<TimedOutOrderDocumentTaskNode>> GetTimedOutOrderDocumentTasks(
			IUnitOfWork uow,
			int timeoutDays,
			CancellationToken cancellationToken
		);

		/// <summary>
		/// Возвращает номера заказов, по которым уже созданы заявки на вывод кодов из оборота
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderIds">Номера заказов для проверки</param>
		/// <returns>Номера заказов, по которым существуют заявки на вывод кодов из оборота</returns>
		Task<IList<int>> GetExistingWithdrawalEdoRequestOrders(
			IUnitOfWork uow,
			IEnumerable<int> orderIds,
			CancellationToken cancellationToken
		);

		/// <summary>
		/// Возвращает список задач ЭДО с указанной проблемой
		/// </summary>
		/// <typeparam name="T">Тип задачи ЭДО</typeparam>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="problemSourceName">Имя источника проблемы</param>	
		/// <param name="minCreationTime">Минимальное время создания задачи</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <param name="maxCreationTime">Максимальное время создания задачи</param>
		/// <returns>Список задач ЭДО с указанной проблемой</returns>
		Task<IList<T>> GetProblemEdoTasks<T>(
			IUnitOfWork uow,
			string problemSourceName,
			DateTime minCreationTime,
			CancellationToken cancellationToken,
			DateTime? maxCreationTime = null
		) where T : OrderEdoTask;

		/// <summary>
		/// Возвращает идентификаторы задач ЭДО с ошибкой отправки, которые связаны с документами, созданными после указанного времени
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="minFiscalDocumentCreationTime">Минимальное время создания фискального документа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список идентификаторов задач ЭДО с ошибкой отправки</returns>
		Task<IList<int>> GetSendErrorFiscalDocumentsEdoTasksIds(
			IUnitOfWork uow,
			DateTime minFiscalDocumentCreationTime,
			CancellationToken cancellationToken
		);

		/// <summary>
		/// Возвращает список активных задач ЭДО с указанной проблемой
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="problemSourceName">Имя источника проблемы</param>
		/// <param name="minCreationTime">Минимальное время создания задачи</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task<IList<OrderEdoTask>> GetProblemEdoTasks(
			IUnitOfWork unitOfWork,
			string problemSourceName,
			DateTime minCreationTime,
			CancellationToken cancellationToken
		);

		IEnumerable<EdoInOrderDocumentNode> GetEdoInOrderDocuments(
			IUnitOfWork uow,
			int orderId
		);
		IEnumerable<EdoInOrderProblemNode> GetEdoProblemsForOrder(IUnitOfWork uow, int orderId);
		IEnumerable<EdoInOrderTransferNode> GetTransferEdoTasksForOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает сгруппированные данные ЭДО, по которым истекло время подтверждения УПД от клиента
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="timeoutDays">Количество дней до истечения принятия УПД</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<IList<TimedOutDocFlowGrouppedNode>> GetTimedOutDocFlows(IUnitOfWork unitOfWork, int timeoutDays, CancellationToken cancellationToken);
		IEnumerable<EdoInOrderReceiptNode> GetReceiptsForOrder(IUnitOfWork uow, int orderId);
	}
}
