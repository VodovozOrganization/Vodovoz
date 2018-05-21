using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping
{
	public class PremiumMap : ClassMap<Premium>
	{
		public PremiumMap()
		{
			Table("premiums");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.TotalMoney).Column("total_money");
			Map(x => x.PremiumReasonString).Column("premium_reason_string");

			References(x => x.Author).Column("author_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("premium_id");
		}
	}
}
