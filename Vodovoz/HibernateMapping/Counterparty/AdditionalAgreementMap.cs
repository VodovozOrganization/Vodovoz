using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HMap
{
	public class AdditionalAgreementMap : ClassMap<AdditionalAgreement>
	{
		public AdditionalAgreementMap ()
		{
			Table ("additional_agreements");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			DiscriminateSubClassesOnColumn ("type");
			Map(x => x.Type).Column("type");
			Map(x => x.AgreementNumber).Column("number");
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.IssueDate).Column("issue_date");
			Map(x => x.IsCancelled).Column("is_cancelled");

			References (x => x.Contract).Column ("counterparty_contract_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
		}
	}

	public class NonfreeRentAgreementMap : SubclassMap<NonfreeRentAgreement>
	{
		public NonfreeRentAgreementMap ()
		{
			DiscriminatorValue ("NonfreeRent");
			HasMany (x => x.Equipment).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("additional_agreement_id");
		}
	}

	public class DailyRentAgreementMap : SubclassMap<DailyRentAgreement>
	{
		public DailyRentAgreementMap ()
		{
			DiscriminatorValue ("DailyRent");
			Map(x => x.RentDays).Column("rent_days");
			HasMany (x => x.Equipment).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("additional_agreement_id");
		}
	}

	public class FreeRentAgreementMap : SubclassMap<FreeRentAgreement>
	{
		public FreeRentAgreementMap()
		{
			DiscriminatorValue ("FreeRent");
			HasMany (x => x.Equipment).Cascade.AllDeleteOrphan ().LazyLoad ()
				.KeyColumn ("additional_agreement_id");
		}
	}

	public class WaterSalesAgreementMap : SubclassMap<WaterSalesAgreement>
	{
		public WaterSalesAgreementMap()
		{
			DiscriminatorValue ("WaterSales");
			Map(x => x.IsFixedPrice).Column("water_is_fixed_price");
			Map(x => x.FixedPrice).Column("water_fixed_price");
		}
	}

	public class RepairAgreementMap : SubclassMap<RepairAgreement>
	{
		public RepairAgreementMap()
		{
			DiscriminatorValue ("Repair");
		}
	}
}