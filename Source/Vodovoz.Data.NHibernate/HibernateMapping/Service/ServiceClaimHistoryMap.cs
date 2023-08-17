using FluentNHibernate.Mapping;
using Vodovoz.Domain.Service;

namespace Vodovoz.HibernateMapping
{
	public class ServiceClaimHistoryMap : ClassMap<ServiceClaimHistory>
	{
		public ServiceClaimHistoryMap ()
		{
			Table ("service_claim_history");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Date).Column ("date_and_time");
			Map (x => x.Comment).Column ("comment");
			Map (x => x.Status).Column ("status").CustomType<ServiceClaimStatusStringType> ();

			References (x => x.Employee).Column ("employee_id");
			References (x => x.ServiceClaim).Column ("service_claim_id");
		}
	}
}

