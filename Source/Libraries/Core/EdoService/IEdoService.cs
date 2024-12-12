using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Errors;

namespace EdoService.Library
{
	public interface IEdoService
	{
		void CancelOldEdoOffers(IUnitOfWork unitOfWork, Order order);
		void SetNeedToResendEdoDocumentForOrder<T>(T entity, Type type) where T : IDomainObject;
		Result ValidateEdoContainers(IList<EdoContainer> edoContainers);
		Result ValidateOrderForDocument(Order order, Type type);
	}
}
