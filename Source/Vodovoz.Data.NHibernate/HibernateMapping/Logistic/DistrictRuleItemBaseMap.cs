using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
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
			References(x => x.District).Column("district_id");
		}
	}

	public class CommonDistrictRuleItemMap : SubclassMap<CommonDistrictRuleItem>
	{
		public CommonDistrictRuleItemMap()
		{
			DiscriminatorValue("Common");
		}
	}

	public class WeekDayDistrictRuleItemMap : SubclassMap<WeekDayDistrictRuleItem>
	{
		public WeekDayDistrictRuleItemMap()
		{
			DiscriminatorValue("WeekDay");

			Map(x => x.WeekDay).Column("week_day");
		}
	}
}
