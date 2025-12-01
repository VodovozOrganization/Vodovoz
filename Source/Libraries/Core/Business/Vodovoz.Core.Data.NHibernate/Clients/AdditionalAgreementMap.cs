using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class AdditionalAgreementMap : ClassMap<AdditionalAgreementEntity>
	{
		public AdditionalAgreementMap()
		{
			Table("additional_agreements");

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.AgreementNumber)
				.Column("number");

			Map(x => x.StartDate)
				.Column("start_date");

			Map(x => x.IssueDate)
				.Column("issue_date");

			Map(x => x.IsCancelled)
				.Column("is_cancelled");

			Map(x => x.ChangedTemplateFile)
				.Column("template_file")
				.LazyLoad();

			References(x => x.Contract)
				.Column("counterparty_contract_id");

			References(x => x.DeliveryPoint)
				.Column("delivery_point_id");

			References(x => x.DocumentTemplate)
				.Column("template_id");
		}
	}

	public class NonfreeRentAgreementMap : SubclassMap<NonfreeRentAgreementEntity>
	{
		public NonfreeRentAgreementMap()
		{
			DiscriminatorValue("NonfreeRent");

			Map(x => x.RentMonths)
				.Column("rent_months");

			HasMany(x => x.PaidRentEquipments)
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.KeyColumn("additional_agreement_id");
		}
	}

	public class DailyRentAgreementMap : SubclassMap<DailyRentAgreementEntity>
	{
		public DailyRentAgreementMap()
		{
			DiscriminatorValue("DailyRent");

			Map(x => x.RentDays)
				.Column("rent_days");

			HasMany(x => x.Equipment)
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.KeyColumn("additional_agreement_id");
		}
	}

	public class FreeRentAgreementMap : SubclassMap<FreeRentAgreementEntity>
	{
		public FreeRentAgreementMap()
		{
			DiscriminatorValue("FreeRent");

			HasMany(x => x.Equipment)
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.KeyColumn("additional_agreement_id");
		}
	}

	public class WaterSalesAgreementMap : SubclassMap<WaterSalesAgreementEntity>
	{
		public WaterSalesAgreementMap()
		{
			DiscriminatorValue("WaterSales");
		}
	}

	public class EquipmentSalesAgreementMap : SubclassMap<SalesEquipmentAgreementEntity>
	{
		public EquipmentSalesAgreementMap()
		{
			DiscriminatorValue("EquipmentSales");

			HasMany(x => x.SalesEqipments)
				.KeyColumn("additional_agreement_id")
				.Inverse()
				.Cascade.AllDeleteOrphan()
				.LazyLoad();
		}
	}

	public class RepairAgreementMap : SubclassMap<RepairAgreementEntity>
	{
		public RepairAgreementMap()
		{
			DiscriminatorValue("Repair");
		}
	}
}
