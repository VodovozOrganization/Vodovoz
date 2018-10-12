using System;
using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class DeliveryShiftMap : ClassMap<DeliveryShift>
	{
		public DeliveryShiftMap ()
		{
			Table("delivery_shift");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
			Map(x => x.StartTime).Column("start_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.EndTime).Column("end_time").CustomType<TimeAsTimeSpanType>();
		}
	}
}

