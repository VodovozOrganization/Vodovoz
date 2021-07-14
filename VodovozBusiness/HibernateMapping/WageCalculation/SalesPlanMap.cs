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

			HasMany(x => x.EquipmentTypeItemSalesPlans)
				.Where($"type='EquipmentType'")
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sales_plan_id");

			HasMany(x => x.NomenclatureItemSalesPlans)
				.Where($"type='Nomenclature'")
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("sales_plan_id");

			//HasManyToMany<Nomenclature>(x => x.Nomenclatures)
			//	.Table("nomenclature_sales_plan")
			//	.ParentKeyColumn("sales_plan_id")
			//	.ChildKeyColumn("nomenclature_id")
			//	.LazyLoad();

			//HasManyToMany<NomenclatureItemSalesPlan>(x => x.NomenclatureItemSalesPlans)
			//	.Table("nomenclature_sales_plan")
			//	.ParentKeyColumn("sales_plan_id")
			//	.ChildKeyColumn("nomenclature_id")
			//	.LazyLoad();


			//HasManyToMany<EquipmentType>(x => x.EquipmentTypes)
			//	.Table("nomenclature_sales_plan")
			//	.ParentKeyColumn("sales_plan_id")
			//	.ChildKeyColumn("equipment_type_id")
			//	.LazyLoad();

			//HasManyToMany<EquipmentKind>(x => x.EquipmentKinds)
			//	.Table("nomenclature_sales_plan")
			//	.ParentKeyColumn("sales_plan_id")
			//	.ChildKeyColumn("equipment_kind_id")
			//	.LazyLoad();
		}
	}
}