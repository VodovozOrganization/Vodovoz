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
            Map(x => x.IsArchive).Column("is_archive");
            Map(x => x.ReasonCategory).Column("category");
        }
    }
}