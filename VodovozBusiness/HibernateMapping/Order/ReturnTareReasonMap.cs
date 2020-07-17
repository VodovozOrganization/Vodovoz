using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
    public class ReturnTareReasonMap : ClassMap<ReturnTareReason>
    {
        public ReturnTareReasonMap()
        {
            Table("return_tare_reasons");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Name).Column("name");
            //Map(x => x.CreateDate).Column("create_date").ReadOnly();
            Map(x => x.IsArchive).Column("is_archive");
            Map(x => x.ReasonCategory).Column("category");
            HasManyToMany(x => x.Orders).Table("return_tare_reasons_to_orders")
                .ParentKeyColumn("return_tare_reason_id")
                .ChildKeyColumn("order_id")
                .LazyLoad();
        }
    }
}