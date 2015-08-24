using FluentNHibernate.Mapping;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain;

namespace Vodovoz
{
	public class ServiceClaimMap: ClassMap<ServiceClaim>
	{
		public ServiceClaimMap ()
		{
			Table ("service_claims");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Reason).Column ("reason");
			Map (x => x.Kit).Column ("kit");
			Map (x => x.Comment).Column ("comment");
			Map (x => x.TotalPrice).Column ("total_price");
			Map (x => x.ServiceStartDate).Column ("service_start_date");
			Map (x => x.RepeatedService).Column ("repeated_service");
			Map (x => x.DiagnosticsResult).Column ("diagnostics_result");
			Map (x => x.Status).Column ("status").CustomType<ServiceClaimStatusStringType> ();
			Map (x => x.Payment).Column ("payment_type").CustomType<PaymentTypeStringType> ();

			References (x => x.Counterparty).Column ("counterparty_id");
			References (x => x.Nomenclature).Column ("nomenclature_id");
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
			References (x => x.Engineer).Column ("engineer_id");
			References (x => x.InitialOrder).Column ("initial_order_id");
			References (x => x.FinalOrder).Column ("final_order_id");

			HasMany (x => x.ServiceClaimItems).Cascade.AllDeleteOrphan ().LazyLoad ().KeyColumn ("service_claim_id");
			HasMany (x => x.ServiceClaimHistory).Cascade.AllDeleteOrphan ().LazyLoad ().KeyColumn ("service_claim_id");
		}
	}
}