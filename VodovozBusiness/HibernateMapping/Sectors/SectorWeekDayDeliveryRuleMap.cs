using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.HibernateMapping.Sectors
{
	public class SectorWeekDaysDeliveryRuleMap: ClassMap<SectorWeekDayDeliveryRule>
	{
		public SectorWeekDaysDeliveryRuleMap()
		{
			Table("sector_week_days_delivery_rules");
			Not.LazyLoad();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DeliveryWeekDay).Column("delivery_week_day");
			Map(x => x.Price).Column("price");
			
			References(x => x.DeliveryPriceRule).Column("delivery_price_rule_id");
			References(x => x.SectorWeekDayDeliveryRuleVersion).Column("sector_week_day_rules_version");
		}
	}
}