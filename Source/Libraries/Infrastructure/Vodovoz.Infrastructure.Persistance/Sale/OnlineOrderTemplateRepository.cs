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
		
		public bool NotArchiveTemplateExists(IUnitOfWork uow, int templateId)
		{
			return (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					where template.Id == templateId
						&& template.Status != OnlineOrderTemplateStatus.Archived
					select template.Id
				)
				.Any();
		}

		public bool OnlineOrderFromCounterparty(IUnitOfWork uow, int templateId, int? counterpartyId)
		{
			return (
					from template in uow.Session.Query<OnlineOrderTemplate>()
					where template.Id == templateId
						&& template.CounterpartyId == counterpartyId
					select template.Id
				)
				.Any();
		}
	}
}
