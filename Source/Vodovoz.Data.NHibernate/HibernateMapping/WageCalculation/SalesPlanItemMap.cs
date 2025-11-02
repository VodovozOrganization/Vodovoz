using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Data.NHibernate.HibernateMapping.WageCalculation
{
	public class SalesPlanItemMap : ClassMap<SalesPlanItem>
	{
		public SalesPlanItemMap()
		{
			Table("sales_plan_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.PlanDay).Column("plan_day");
			Map(x => x.PlanMonth).Column("plan_month");
			References(x => x.SalesPlan).Column("sales_plan_id");

			DiscriminateSubClassesOnColumn("type");
		}

		public class EquipmentKindSalesPlanMap : SubclassMap<EquipmentKindSalesPlanItem>
		{
			public EquipmentKindSalesPlanMap()
			{
				DiscriminatorValue("EquipmentKind");

				References(x => x.EquipmentKind).Column("equipment_kind_id");
			}
		}

		public class EquipmentTypeSalesPlanMap : SubclassMap<EquipmentTypeSalesPlanItem>
		{
			public EquipmentTypeSalesPlanMap()
			{
				DiscriminatorValue("EquipmentType");

				Map(x => x.EquipmentType).Column("equipment_type");
			}
		}

		public class NomenclatureItemSalesPlanMap : SubclassMap<NomenclatureSalesPlanItem>
		{
			public NomenclatureItemSalesPlanMap()
			{
				DiscriminatorValue("Nomenclature");

				References(x => x.Nomenclature).Column("nomenclature_id");
			}
		}
	}
}
