using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class PremiumBaseMap : ClassMap<PremiumBase>
	{
		public PremiumBaseMap()
		{
			Table("premiums");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.TotalMoney).Column("total_money");
			Map(x => x.PremiumReasonString).Column("premium_reason_string");

			References(x => x.Author).Column("author_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("premium_id");

			DiscriminateSubClassesOnColumn("type");
		}

		public class PremiumMap : SubclassMap<Premium>
		{
			public PremiumMap()
			{
				DiscriminatorValue("Premium");
			}
		}

		public class PremiumRaskatGAZelleMap : SubclassMap<PremiumRaskatGAZelle>
		{
			public PremiumRaskatGAZelleMap()
			{
				DiscriminatorValue("RaskatGAZelle");
				References(x => x.RouteList).Column("route_list_id");
			}
		}
	}
}
