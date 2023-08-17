using FluentNHibernate.Mapping;
using Vodovoz.Domain.Service;

namespace Vodovoz.HibernateMapping
{
	public class ServiceClaimItemMap : ClassMap<ServiceClaimItem>
	{
		public ServiceClaimItemMap ()
		{
			Table ("service_claim_items");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Count).Column ("count");
			Map (x => x.Price).Column ("price");

			References (x => x.ServiceClaim).Column ("service_claim_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
		}
	}
}

