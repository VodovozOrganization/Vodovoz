using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Edo;

namespace Vodovoz.Infrastructure.Persistance.Edo
{
	public class TaxcomEdoDocflowLastProcessTimeRepository : ITaxcomEdoDocflowLastProcessTimeRepository
	{
		public TaxcomEdoDocflowLastProcessTime GetTaxcomEdoDocflowLastProcessTime(IUnitOfWork uow, string edoAccount)
		{
			return (from lastEventProcessTime in uow.Session.Query<TaxcomEdoDocflowLastProcessTime>()
					join organization in uow.Session.Query<Organization>()
						on lastEventProcessTime.OrganizationId equals organization.Id
					where organization.TaxcomEdoAccountId == edoAccount
					select lastEventProcessTime)
				.SingleOrDefault();
		}
	}
}
