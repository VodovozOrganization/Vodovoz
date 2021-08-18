using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Sale
{
	public class DistrictRuleItemBaseMap : ClassMap<DistrictRuleItemBase>
	{
		public DistrictRuleItemBaseMap()
		{
			Table("district_rule_items");

			DiscriminateSubClassesOnColumn("rule_type").AlwaysSelectWithValue();
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Price).Column("price");

			References(x => x.DeliveryPriceRule).Column("delivery_price_rule_id");
		}
	}
	
	public class CommonDistrictRuleItemMap : SubclassMap<CommonDistrictRuleItem>
	{
		public CommonDistrictRuleItemMap()
		{
			DiscriminatorValue("Common");
			
			References(x => x.SectorDeliveryRuleVersion).Column("sector_delivery_rule_version_id");
		}
	}
	
	public class WeekDayDistrictRuleItemMap : SubclassMap<WeekDayDistrictRuleItem>
	{
		public WeekDayDistrictRuleItemMap()
		{
			DiscriminatorValue("WeekDay");

			Map(x => x.WeekDay).Column("week_day").CustomType<WeekDayNameStringType>();
			
			References(x => x.SectorWeekDayDeliveryRuleVersion).Column("sector_week_day_delivery_rule_version_id");
		}
	}
}
