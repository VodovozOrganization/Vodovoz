using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class DiscountReasonMap : ClassMap<DiscountReason>
	{
		public DiscountReasonMap()
		{
			Table("discount_reasons");

			Id (x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Value).Column("value");
			Map(x => x.IsPremiumDiscount).Column("is_premium_discount");
			
			Map(x => x.ValueType).Column("value_type").CustomType<DiscountUnitTypeStringType>();

			HasManyToMany(x => x.NomenclatureCategories)
				.Table("discount_reasons_nomenclature_categories")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("discount_nomenclature_category_id")
				.LazyLoad();
			
			HasManyToMany(x => x.Nomenclatures)
				.Table("discount_reasons_nomenclatures")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("nomenclature_id")
				.LazyLoad();
			
			HasManyToMany(x => x.ProductGroups)
				.Table("discount_reasons_nomenclature_groups")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("group_id")
				.LazyLoad();
		}
	}
}
