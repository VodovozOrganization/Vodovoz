using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Sale
{
	public class ScheduleRestrictedDistrictRuleItemMap : ClassMap<ScheduleRestrictedDistrictRuleItem>
	{
		public ScheduleRestrictedDistrictRuleItemMap()
		{
			Table("schedule_restricted_district_rule_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DeliveryPrice).Column("price");

			References(x => x.DeliveryPriceRule).Column("rule_id");
			References(x => x.ScheduleRestrictedDistrict).Column("district_id");
		}
	}
}
