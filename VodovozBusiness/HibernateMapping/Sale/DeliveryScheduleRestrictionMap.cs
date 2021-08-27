using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Sale
{
    public class DeliveryScheduleRestrictionMap : ClassMap<DeliveryScheduleRestriction>
    {
        public DeliveryScheduleRestrictionMap()
        {
            Table("sector_delivery_schedule_restrictions");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.WeekDay).Column("week_day").CustomType<WeekDayNameStringType>();
            
            References(x => x.DeliverySchedule).Column("delivery_schedule_id");
            References(x => x.AcceptBefore).Column("accept_before_id");
            References(x => x.SectorWeekDayScheduleVersion).Column("sector_week_day_schedules");
        }
    }
}