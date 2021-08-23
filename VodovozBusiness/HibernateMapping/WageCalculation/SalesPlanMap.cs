using FluentNHibernate.Mapping;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class SalesPlanMap : ClassMap<SalesPlan>
	{
		public SalesPlanMap()
		{
			Table("sales_plans");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.EmptyBottlesToTake).Column("empty_bottles_to_take_salesplan_wage");
			Map(x => x.FullBottleToSell).Column("full_bottles_to_sell_salesplan_wage");
			Map(x => x.ProceedsDay).Column("proceeds_day");
			Map(x => x.ProceedsMonth).Column("proceeds_month");

			HasMany(x => x.EquipmentTypeItemSalesPlans)
				.Where($"type='EquipmentType'")
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sales_plan_id");

			HasMany(x => x.NomenclatureItemSalesPlans)
				.Where($"type='Nomenclature'")
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sales_plan_id");

			HasMany(x => x.EquipmentKindItemSalesPlans)
				.Where($"type='EquipmentKind'")
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sales_plan_id");
		}
	}
}