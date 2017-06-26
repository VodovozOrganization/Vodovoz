using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class AtWorkDriverMap : ClassMap<AtWorkDriver>
	{
		public AtWorkDriverMap ()
		{
			Table("at_work_drivers");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");
			Map(x => x.Trips).Column("trips");

			References(x => x.Employee).Column("employee_id");
			References(x => x.Car).Column("car_id");
		}
	}
}

