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
		void CancelOldEdoOffers(IUnitOfWork unitOfWork, Order order);
		void SetNeedToResendEdoDocumentForOrder<T>(T entity, DocumentContainerType type) where T : IDomainObject;
		void ResendEdoOrderDocumentForOrder(Order order, OrderDocumentType type);
		Result ResendEdoDocumentForOrder(OrderEntity order, Guid docflowId);
		Result ValidateEdoContainers(IList<EdoContainer> edoContainers);
		Result ValidateEdoOrderDocument(IUnitOfWork uow, OutgoingEdoDocument orderDocument);
		Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type);
		Result ValidateOrderForDocumentType(OrderEntity order, EdoDocumentType type);
		Result ValidateOrderForOrderDocument(EdoDocFlowStatus status);
		Result ValidateOutgoingDocument(IUnitOfWork uow, EdoDockflowData dockflowData);
	}
}
