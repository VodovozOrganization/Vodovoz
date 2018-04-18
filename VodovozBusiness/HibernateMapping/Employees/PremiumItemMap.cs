using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping
{
	public class PremiumItemMap : ClassMap<PremiumItem>
	{
		public PremiumItemMap()
		{
			Table("premium_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Money).Column("money");

			References(x => x.Premium).Column("premium_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.WageOperation).Column("wages_movement_operations_id");
		}
	}
}
