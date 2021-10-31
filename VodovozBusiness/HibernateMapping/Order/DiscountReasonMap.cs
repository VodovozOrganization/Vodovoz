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

			Map(x => x.ValueType).Column("value_type")
				.CustomType<DiscountValueTypeStringType>();

			HasMany(x => x.ProductGroups).KeyColumn("discount_reason_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}

	public class DiscountNomenclatureGroupMap : ClassMap<DiscountNomenclatureGroup>
	{
		public DiscountNomenclatureGroupMap()
		{
			Table("discount_nomenclature_group");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ProductGroup).Column("group_id");
			References(x => x.DiscountReason).Column("discount_reason_id");

		}
	}
}
