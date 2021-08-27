using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorDeliveryRuleVersionMap: ClassMap<SectorDeliveryRuleVersion>
	{
		public SectorDeliveryRuleVersionMap()
		{
			Table("sector_delivery_rule_version");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.Status).Column("status").CustomType<SectorsSetStatusStringType>();
			
			References(x => x.Sector).Column("sector_id");
			
			HasMany(x => x.CommonDistrictRuleItems).KeyColumn("sector_delivery_rule_version_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}
}