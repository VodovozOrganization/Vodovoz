using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping
{
	public class AtWorkForwarderMap : ClassMap<AtWorkForwarder>
	{
		public AtWorkForwarderMap ()
		{
			Table("at_work_forwader");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");

			References(x => x.Employee).Column("employee_id");
		}
	}
}

