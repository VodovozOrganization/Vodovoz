using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IEdoRepository
	{
		/// <summary>
		/// Получить список организаций
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список организаций</returns>
		Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Есть ли сегодня чек на указанную сумму
		/// </summary>
		/// <param name="sum">Сумма чека</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат проверки</returns>
		Task<bool> HasReceiptOnSumToday(decimal sum, CancellationToken cancellationToken);

		/// <summary>
		/// Получить задачу ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача ЭДО</returns>
		Task<OrderEdoTask> GetOrderEdoTaskById(
			IUnitOfWork uow,
			int taskId,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получает ЭДО документы заказа по идентификатору заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список документов ЭДО</returns>
		IEnumerable<OrderEdoDocument> GetOrderEdoDocumentsByOrderId(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает задачи ЭДО по идентификатору заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список задач ЭДО</returns>
		IEnumerable<OrderEdoTask> GetEdoTaskByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получает задачи ЭДО для указанного заказа в виде узлов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов задач ЭДО</returns>
		IEnumerable<OrderEdoTaskNode> GetEdoTasksForOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получает задачи ЭДО для указанного заказа в виде узлов с информацией о документообороте
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов документооборота ЭДО</returns>
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
		/// Возвращает данные для обработки активных проблем контакта при отправке чека
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="problemSourceNames">Имена источников проблем</param>
		/// <param name="minCreationTime">Минимальное время создания задачи</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные задач, проблем и состояния их обработки</returns>
		Task<IList<ReceiptContactProblemNode>> GetReceiptContactProblemNodes(
			IUnitOfWork uow,
			IEnumerable<string> problemSourceNames,
			DateTime minCreationTime,
			CancellationToken cancellationToken
		);

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

		/// <summary>
		/// Получает список документов ЭДО для указанного заказа в виде узлов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов документов ЭДО</returns>
		IEnumerable<EdoInOrderDocumentNode> GetEdoInOrderDocuments(
			IUnitOfWork uow,
			int orderId
		);

		/// <summary>
		/// Получает список проблем ЭДО для указанного заказа в виде узлов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов проблем ЭДО</returns>
		IEnumerable<EdoInOrderProblemNode> GetEdoProblemsForOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получит список задач ЭДО по передаче документов для указанного заказа в виде узлов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов задач ЭДО</returns>
		IEnumerable<EdoInOrderTransferNode> GetTransferEdoTasksForOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает сгруппированные данные ЭДО, по которым истекло время подтверждения УПД от клиента
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="timeoutDays">Количество дней до истечения принятия УПД</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<IList<TimedOutDocFlowGrouppedNode>> GetTimedOutDocFlows(
			IUnitOfWork unitOfWork,
			int timeoutDays,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получить список чеков для указанного заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов чеков для указанного заказа</returns>
		IEnumerable<EdoInOrderReceiptNode> GetReceiptsForOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получить список документооборотов по налоговой для указанного заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Список узлов документооборота для указанного заказа</returns>
		IEnumerable<EdoInOrderTaxcomDocflowNode> GetEdoInOrderDocflows(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Получить GTIN по номеру GTIN
		/// </summary>
		/// <param name="gtinNumber">Номер GTIN</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Сущность GTIN</returns>
		Task<GtinEntity> GetGtinByGtinNumberAsync(
			string gtinNumber,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить список узлов проблем с отсутствием кодов в пуле
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="problemSourceName">Имя источника проблемы</param>
		/// <param name="batchSize">Размер партии (опционально)</param>
		/// <param name="retryIntervalHours">Интервал повторной попытки в часах</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Коллекция узлов проблем с отсутствием кодов в пуле</returns>
		Task<IList<CodePoolMissingProblemNode>> GetCodePoolMissingProblemNodes(
			IUnitOfWork uow,
			string problemSourceName,
			int? batchSize,
			int retryIntervalHours,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает задачи с активными проблемами для возобновления
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="problemSourceName">Имя источника проблемы</param>
		/// <param name="minCreationTime">Минимальное время создания задачи</param>
		/// <param name="reasonForLeaving">Причина выбытия из документооборота</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task<IList<EdoTaskProblemRoutineNode>> GetProblemEdoTasksForResume(
			IUnitOfWork unitOfWork,
			string problemSourceName,
			DateTime minCreationTime,
			ReasonForLeaving? reasonForLeaving = null,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить документ ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Документ ЭДО</returns>
		OrderEdoDocument GetOrderEdoDocumentByTaskId(IUnitOfWork uow, int taskId);

		/// <summary>
		/// Получить документооборот Такском по идентификатору задачи
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Документооборот Такском</returns>
		TaxcomDocflow GetTaxcomDocflowByTaskId(IUnitOfWork uow, int taskId);

		/// <summary>
		/// Получить список статусов ЭДО, которые считаются полученными
		/// </summary>
		/// <returns>Массив статусов ЭДО</returns>
		EdoDocFlowStatus[] GetRecievedEdoDocFlowStatuses();

		// <summary>
		/// Получить список статусов ЭДО документов, которые считаются полученными
		/// </summary>
		/// <returns>Массив статусов ЭДО документов</returns>
		EdoDocumentStatus[] GetInProgressEdoDocumentStatuses();
	}
}
