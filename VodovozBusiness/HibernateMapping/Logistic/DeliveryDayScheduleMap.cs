using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class DeliveryDayScheduleMap : ClassMap<DeliveryDaySchedule>
	{
		public DeliveryDayScheduleMap()
		{
			Table("delivery_day_schedule");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");

			HasManyToMany(x => x.Shifts).Table("delivery_day_schedule_trips")
				.ParentKeyColumn("delivery_day_schedule_id")
				.ChildKeyColumn("delivery_shift_id")
				.LazyLoad();
		}
	}
}
