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

			HasManyToMany(x => x.ProductGroups)
				.Table("discount_reasons_nomenclature_groups")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("group_id")
				.LazyLoad();
		}
	}
}
