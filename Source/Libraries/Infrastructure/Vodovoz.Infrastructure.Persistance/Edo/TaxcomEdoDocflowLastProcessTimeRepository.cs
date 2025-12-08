using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.EntityRepositories.Edo;

namespace Vodovoz.Infrastructure.Persistance.Edo
{
	public class TaxcomEdoDocflowLastProcessTimeRepository : ITaxcomEdoDocflowLastProcessTimeRepository
	{
		public TaxcomEdoDocflowLastProcessTime GetTaxcomEdoDocflowLastProcessTime(IUnitOfWork uow, string edoAccount)
		{
			return (from lastEventProcessTime in uow.Session.Query<TaxcomEdoDocflowLastProcessTime>()
					join taxcomEdoSettings in uow.Session.Query<TaxcomEdoSettings>()
						on lastEventProcessTime.OrganizationId equals taxcomEdoSettings.OrganizationId
					where taxcomEdoSettings.EdoAccount == edoAccount
					select lastEventProcessTime)
				.SingleOrDefault();
		}
	}
}
