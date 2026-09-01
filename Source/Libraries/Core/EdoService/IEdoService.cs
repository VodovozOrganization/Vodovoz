using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using VodovozBusiness.Nodes;

namespace EdoService.Library
{
	public interface IEdoService
	{
		/// <summary>
		/// Аннулирует старые офферы ЭДО для заказа
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="order"></param>
		void CancelOldEdoOffers(IUnitOfWork unitOfWork, Order order);

		/// <summary>
		/// Устанавливает флаг необходимости повторной отправки документа ЭДО для заказа
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity"></param>
		/// <param name="type"></param>
		void SetNeedToResendEdoDocumentForOrder<T>(T entity, DocumentContainerType type) where T : IDomainObject;

		/// <summary>
		/// Переотправка документа заказа по ЭДО
		/// </summary>
		/// <param name="order"></param>
		/// <param name="type"></param>
		void ResendEdoOrderDocumentForOrder(Order order, OrderDocumentType type);

		/// <summary>
		/// Переотправка документа по ЭДО
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		Result ResendEdoDocumentForOrder(OrderEntity order);

		/// <summary>
		/// Проверяет возможность отправки документов ЭДО для контейнеров
		/// </summary>
		/// <param name="edoContainers">Список контейнеров ЭДО</param>
		/// <returns>Результат проверки</returns>
		Result ValidateEdoContainers(IList<EdoContainer> edoContainers);

		/// <summary>
		/// Проверяет возможность отправки документа ЭДО заказа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderDocument"></param>
		/// <returns>Результат проверки</returns>
		Result ValidateEdoOrderDocument(IUnitOfWork uow, OrderEdoDocument orderDocument);

		/// <summary>
		/// Проверяет возможность отправки документа ЭДО заказа определенного типа
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <param name="type">Тип документа</param>
		/// <returns>Результат проверки</returns>
		Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type);

		/// <summary>
		/// Проверяет возможность отправки документа ЭДО заказа определенного типа
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <param name="type">Тип документа</param>
		/// <returns>Результат проверки</returns>
		Result ValidateOrderForDocumentType(OrderEntity order, EdoDocumentType type);

		/// <summary>
		/// Проверяет возможность отправки документа ЭДО заказа по статусу документооборота
		/// </summary>
		/// <param name="status">Статус документооборота</param>
		/// <returns>Результат проверки</returns>
		Result ValidateOrderForOrderDocument(EdoDocFlowStatus status);

		/// <summary>
		/// Проверка исходящего документа ЭДО
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="dockflowData"></param>
		/// <returns></returns>
		Result ValidateOutgoingDocument(IUnitOfWork uow, EdoDockflowData dockflowData);
		
		/// <summary>
		/// Публикует ивент в rabbit о создании ЭДО задачи
		/// </summary>
		/// <param name="edoTask"></param>
		/// <returns></returns>
		Result SendDocumentTaskCreatedEvent(EdoTask edoTask);

		/// <summary>
		/// Запускает переобработку задачи на отправку чека, 
		/// которая попала в проблему в статусе New
		/// </summary>
		/// <param name="receiptEdoTaskId">Идентификатор задачи чека</param>
		/// <returns>Результат переобработки</returns>
		Result RehandleNewReceiptDocumentWithProblem(int receiptEdoTaskId);

		/// <summary>
		/// Можно ли переотправить документ
		/// </summary>
		/// <param name="status">Статус документа</param>
		/// <returns>Да - если можно переотправить, Нет - если нельзя</returns>
		bool CanResendEdoDocument(EdoDocumentStatus? status);

		/// <summary>
		/// Переотправка документа по ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Результат переотправки документа</returns>
		Result<string> ResendEdoDocumentForOrder(int taskId);

		/// <summary>
		/// Проверяет, отличается ли текущий основной аккаунт ЭДО клиента от аккаунта,
		/// использованного при отправке документа.
		/// </summary>
		/// <param name="taskId">Идентификатор задачи документа.</param>
		/// <returns>Результат проверки с признаком изменения аккаунта.</returns>
		Result<bool> IsRecipientEdoAccountChanged(int taskId);

		/// <summary>
		/// Переотправляет УПД на изменившийся основной аккаунт ЭДО клиента.
		/// </summary>
		/// <param name="taskId">Идентификатор задачи документа.</param>
		/// <returns>Результат запуска переотправки.</returns>
		Result<string> ResendEdoDocumentToChangedAccount(int taskId);

		/// <summary>
		/// Ставит документ в очередь на переотправку после отмены вывода кодов из оборота в ЧЗ.
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Результат постановки в очередь</returns>
		Result<string> ScheduleResendEdoDocumentAfterTrueMarkCancellation(int taskId);

		/// <summary>
		/// Переотправляет документ ЭДО с подбором новых кодов ЧЗ из пула.
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Результат переотправки документа</returns>
		Result<string> ResendEdoDocumentForOrderWithCodesFromPool(int taskId);

		/// <summary>
		/// Переотправка чека по ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="receiptEdoTaskId">Идентификатор задачи чека</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат переотправки чека</returns>
		Task<Result> ResendReceiptDocument(
			int receiptEdoTaskId,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Переотправка документа по ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="orderEdoTaskId">Идентификатор задачи документа</param>
		/// <returns>Результат переотправки документа</returns>
		Result<string> TryResendUpdDocument(int orderEdoTaskId);

		/// <summary>
		/// Переотправка чека по ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="orderEdoTaskId">Идентификатор задачи чека</param>
		/// <returns>Результат переотправки чека</returns>
		Result<string> TryResendReceiptDocument(int orderEdoTaskId);

		/// <summary>
		/// Проверяет наличие отмененного документооборота по задаче ЭДО
		/// </summary>
		/// <param name="edoTaskId">Идентификатор задачи ЭДО</param>
		/// <returns>True - если отмененный документооборот есть, False - если нет</returns>
		bool HasCancelledDocflow(int edoTaskId);

		/// <summary>
		/// Отменяет документооборот по задаче ЭДО
		/// </summary>
		/// <param name="edoTaskId">Идентификатор задачи ЭДО</param>
		/// <returns>Результат отмены документооборота с сообщением</returns>
		Result<string> CancelDocflow(int edoTaskId);

		/// <summary>
		/// Проверяет наличие документооборота по задаче ЭДО
		/// </summary>
		/// <param name="edoTaskId">Идентификатор задачи ЭДО</param>
		/// <returns>True - если документооборот есть, False - если нет</returns>
		bool HasDocflow(int edoTaskId);
		Result RehandleNewUpdDocumentWithProblem(int updEdoTaskId);

		/// <summary>
		/// Обновить статус документооборота Такском по ЭДО задаче
		/// </summary>
		/// <param name="taskId">Идентификатор задачи ЭДО</param>
		/// <param name="docflowId">Идентификатор документооборота</param>
		/// <returns>Результат обновления статуса</returns>
		Result<string> UpdateDocflowStatus(int taskId, Guid? docflowId);

		/// <summary>
		/// Обновляет статус документооборота из Taxcom по ID документооборота
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="docflowId">ID документооборота в Taxcom</param>
		/// <param name="organizationId">ID организации, отправившей документ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат с информацией о статусе</returns>
		Task<Result<string>> UpdateDocflowStatusAsync(
			IUnitOfWork uow,
			Guid? docflowId,
			int organizationId,
			CancellationToken cancellationToken = default);
	}
}
