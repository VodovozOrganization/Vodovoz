using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace EdoService.Library
{
	public interface IEdoService
	{
		void CancelOldEdoOffers(IUnitOfWork unitOfWork, Order order);
		void SetNeedToResendEdoDocumentForOrder<T>(T entity, DocumentContainerType type) where T : IDomainObject;
		void ResendEdoOrderDocumentForOrder(Order order, OrderDocumentType type);
		Result ValidateEdoContainers(IList<EdoContainer> edoContainers);
		Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type);
		Result ValidateOrderForOrderDocument(EdoDocFlowStatus status);
	}
}
