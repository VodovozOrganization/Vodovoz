using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
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
	}
}
