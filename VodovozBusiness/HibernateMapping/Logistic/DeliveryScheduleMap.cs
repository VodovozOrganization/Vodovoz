using System;
using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class DeliveryScheduleMap : ClassMap<DeliverySchedule>
	{
		public DeliveryScheduleMap ()
		{
			Table("delivery_schedule");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
			Map(x => x.From).Column ("from_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.To).Column ("to_time").CustomType<TimeAsTimeSpanType>();
		}
	}
}

