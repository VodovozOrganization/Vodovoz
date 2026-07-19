using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System.Collections.Generic;
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

		Result ValidateEdoContainers(IList<EdoContainer> edoContainers);

		/// <summary>
		/// Проверяет возможность отправки документа ЭДО заказа
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="orderDocument"></param>
		/// <returns></returns>
		Result ValidateEdoOrderDocument(IUnitOfWork uow, OrderEdoDocument orderDocument);

		Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type);

		/// <summary>
		/// Проверяет возможность отправки документа ЭДО заказа определенного типа
		/// </summary>
		/// <param name="order"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		Result ValidateOrderForDocumentType(OrderEntity order, EdoDocumentType type);

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
		/// Запускает переобработку задачи на отправку УПД, 
		/// которая попала в проблему в статусе New
		/// </summary>
		void RehandleNewUpdDocumentWithProblem(int updEdoTaskId);

		/// <summary>
		/// Запускает переобработку задачи на отправку чека, 
		/// которая попала в проблему в статусе New
		/// </summary>
		void RehandleNewReceiptDocumentWithProblem(int receiptEdoTaskId);

		/// <summary>
		/// Можно ли переотправить документ
		/// </summary>
		/// <param name="status">Статус документа</param>
		/// <returns>Да - если можно переотправить, Нет - если нельзя</returns>
		bool CanResend(EdoDocumentStatus? status);

		/// <summary>
		/// Переотправка документа по ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="taskId">Идентификатор задачи</param>
		/// <returns>Результат переотправки документа</returns>
		Result ResendEdoDocumentForOrder(int taskId);

		/// <summary>
		/// Переотправка чека по ЭДО из сохраненных в пул
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="orderTaskId">Идентификатор задачи</param>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>Результат переотправки чека</returns>
		Task<Result> ResendReceiptFromSavedToPool(IUnitOfWork uow, int? orderTaskId, int orderId);

		/// <summary>
		/// Переотправка чека по ЭДО по идентификатору задачи
		/// </summary>
		/// <param name="receiptEdoTaskId">Идентификатор задачи чека</param>
		/// <returns>Результат переотправки чека</returns>
		Task<Result> ResendReceiptDocument(int receiptEdoTaskId);
	}
}
