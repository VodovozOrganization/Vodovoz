using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class DeliveryScheduleRestrictionMap : ClassMap<DeliveryScheduleRestriction>
	{
		public DeliveryScheduleRestrictionMap()
		{
			Table("delivery_schedule_restrictions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WeekDay).Column("week_day");

			References(x => x.District).Column("district_id");
			References(x => x.DeliverySchedule).Column("delivery_schedule_id");
			References(x => x.AcceptBefore).Column("accept_before_id");
		}
	}
}
