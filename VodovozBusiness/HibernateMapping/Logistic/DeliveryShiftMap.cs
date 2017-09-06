using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;
using DataAccess.NhibernateFixes;

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
			Map(x => x.StartTime).Column("start_time").CustomType<TimeAsTimeSpanTypeClone>();
			Map(x => x.EndTime).Column("end_time").CustomType<TimeAsTimeSpanTypeClone>();
		}
	}
}

