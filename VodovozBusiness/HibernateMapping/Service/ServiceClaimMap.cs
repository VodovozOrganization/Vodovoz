using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Service;

namespace Vodovoz.HMap
{
	public class ServiceClaimMap: ClassMap<ServiceClaim>
	{
		public ServiceClaimMap ()
		{
			Table ("service_claims");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Reason)				.Column ("reason");
			Map (x => x.Kit)				.Column ("kit");
			Map (x => x.TotalPrice)			.Column ("total_price");
			Map (x => x.ServiceStartDate)	.Column ("service_start_date");
			Map (x => x.RepeatedService)	.Column ("repeated_service");
			Map (x => x.DiagnosticsResult)	.Column ("diagnostics_result");
			Map (x => x.Status)				.Column ("status")							   .CustomType<ServiceClaimStatusStringType> ();
			Map (x => x.Payment)			.Column ("payment_type")					   .CustomType<PaymentTypeStringType> ();
			Map (x => x.ServiceClaimType)	.Column ("service_claim_type")				   .CustomType<ServiceClaimTypeStringType> ();
			Map (x => x.WithSerial)			.Column ("service_claim_equipment_serial_type").CustomType<ServiceClaimEquipmentSerialStringType>();

			References (x => x.Counterparty)		.Column ("counterparty_id");
			References (x => x.Nomenclature)		.Column ("nomenclature_id");
			References (x => x.Equipment)			.Column ("equipment_id");
			References (x => x.ReplacementEquipment).Column ("replacement_equipment_id");
			References (x => x.DeliveryPoint)		.Column ("delivery_point_id");
			References (x => x.Engineer)			.Column ("engineer_id");
			References (x => x.InitialOrder)		.Column ("initial_order_id");
			References (x => x.FinalOrder)			.Column ("final_order_id");

			HasMany (x => x.ServiceClaimItems)	.Inverse().Cascade.AllDeleteOrphan ().LazyLoad ().KeyColumn ("service_claim_id");
			HasMany (x => x.ServiceClaimHistory).Inverse().Cascade.AllDeleteOrphan ().LazyLoad ().KeyColumn ("service_claim_id");
		}
	}
}