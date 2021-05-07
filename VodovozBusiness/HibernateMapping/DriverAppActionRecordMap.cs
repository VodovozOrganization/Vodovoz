using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HibernateMapping
{
    public class DriverAppActionRecordMap : ClassMap<DriverAppActionRecord>
    {
        public DriverAppActionRecordMap()
        {
            Table("driver_mobile_app_action_records");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.ActionType).Column("type");
            Map(x => x.ActionDateTime).Column("datetime");
        }
    }
}
