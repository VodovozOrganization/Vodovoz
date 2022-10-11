using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
    public class AcceptBeforeMap : ClassMap<AcceptBefore>
    {
        public AcceptBeforeMap()
        {
            Table("accept_before");

            Id(x => x.Id).Column("id").GeneratedBy.Native();
            
            Map(x => x.Name).Column("name");
            Map(x => x.Time).Column("time").CustomType<TimeAsTimeSpanType>();
        }
    }
}