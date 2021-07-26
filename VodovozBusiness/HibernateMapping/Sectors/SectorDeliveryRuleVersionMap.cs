using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorDeliveryRuleVersionMap: ClassMap<SectorDeliveryRuleVersion>
	{
		public SectorDeliveryRuleVersionMap()
		{
			Table("sector_week_days_delivery_rules");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			
			References(x => x.Sector).Column("sector_id");
			HasMany(x => x.CommonDistrictRuleItems).Cascade.AllDeleteOrphan().Inverse().KeyColumn("sector_week_days_delivery_rules_id");
		}
	}
}