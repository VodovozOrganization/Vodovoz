using QS.DomainModel.Entity;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Errors;

namespace EdoService.Library
{
	public interface IEdoService
	{
		void SetNeedToResendEdoDocumentForOrder<T>(T entity, Type type) where T : IDomainObject;
		Result ValidateEdoContainers(IList<EdoContainer> edoContainers);
		Result ValidateOrderForUpd(Order order);
	}
}
