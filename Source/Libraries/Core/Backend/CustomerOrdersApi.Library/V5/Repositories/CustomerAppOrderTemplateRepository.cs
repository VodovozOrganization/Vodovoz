using System.Collections.Generic;
using System.Linq;
using CustomerApps.Contracts.V5;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace CustomerOrdersApi.Library.V5.Repositories
{
	public class CustomerAppOrderTemplateRepository
	{
		public IEnumerable<OrderTemplateData> GetOrderTemplates(IUnitOfWork uow, int counterpartyId)
		{
			var templates = (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					where template.CounterpartyId == counterpartyId
					select OrderTemplateData.Create(template)
				)
				.ToList();
			
			return templates;
		}
	}
}
