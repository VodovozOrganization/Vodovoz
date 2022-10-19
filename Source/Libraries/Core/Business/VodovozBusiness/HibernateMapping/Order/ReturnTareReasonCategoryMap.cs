using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
    public class ReturnTareReasonCategoryMap : ClassMap<ReturnTareReasonCategory>
    {
        public ReturnTareReasonCategoryMap()
        {
            Table("return_tare_reason_categories");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Name).Column("name");

			HasManyToMany(x => x.ChildReasons)
				.Table("return_tare_reasons_to_categories")
				.ParentKeyColumn("return_tare_reason_category_id")
				.ChildKeyColumn("return_tare_reason_id")
				.LazyLoad();
		}
    }
}