using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class AtWorkForwarderMap : ClassMap<AtWorkForwarder>
	{
		public AtWorkForwarderMap ()
		{
			Table("at_work_forwader");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");
			Map(x => x.Trips).Column("trips");

			References(x => x.Employee).Column("employee_id");
			References(x => x.WithDriver).Column("with_driver_id");
		}
	}
}

