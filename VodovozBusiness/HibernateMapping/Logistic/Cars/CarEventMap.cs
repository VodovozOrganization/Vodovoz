﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class CarEventMap : ClassMap<CarEvent>
	{
		public CarEventMap()
		{
			Table("car_events");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.Comment).Column("comment");

			References(x => x.CarEventType).Column("car_event_type_id");
			References(x => x.Author).Column("author_id");
			// References(x => x.Car).Column("car_id");
			References(x => x.Driver).Column("driver_id");
		}
	}
}
