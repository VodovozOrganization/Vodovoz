using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
    public class LateArrivalReasonMap : ClassMap<LateArrivalReason>
    {
        public LateArrivalReasonMap()
        {
            Table("late_arrival_reasons");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            Map(x => x.Name).Column("name");
            Map(x => x.CreateDate).Column("create_date").ReadOnly();
            Map(x => x.IsArchive).Column("is_archive");
        }
    }
}