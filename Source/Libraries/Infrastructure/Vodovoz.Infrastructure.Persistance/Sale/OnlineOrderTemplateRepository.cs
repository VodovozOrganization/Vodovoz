using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.EntityRepositories.Sale;

namespace Vodovoz.Infrastructure.Persistance.Sale
{
	public class OnlineOrderTemplateRepository : IOnlineOrderTemplateRepository
	{
		public int GetActiveOnlineOrderTemplatesCount(IUnitOfWork uow, int counterpartyId)
		{
			return (
				from template in uow.Session.Query<OnlineOrderTemplate>()
				where template.CounterpartyId == counterpartyId
					&& template.Status != OnlineOrderTemplateStatus.Archived
				select template.Id
				)
				.Count();
		}
	}
}
