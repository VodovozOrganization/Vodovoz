using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Data.NHibernate.HibernateMapping.WageCalculation
{
	public class WageParameterItemMap : ClassMap<WageParameterItem>
	{
		public WageParameterItemMap()
		{
			Table("wage_parameter_items");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("wage_parameter_item_type");
			Map(x => x.WageParameterItemType).Column("wage_parameter_item_type").Update().Not.Insert();
		}
	}

	public class FixedWageParameterMap : SubclassMap<FixedWageParameterItem>
	{
		public FixedWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterItemTypes.Fixed));
			Map(x => x.RouteListFixedWage).Column("route_list_fixed_wage");
		}
	}

	public class PercentWageParameterMap : SubclassMap<PercentWageParameterItem>
	{
		public PercentWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterItemTypes.Percent));
			Map(x => x.RouteListPercent).Column("route_list_percent_wage");
			Map(x => x.PercentWageType).Column("percent_wage_type");
		}
	}

	public class SalesPlanWageParameterMap : SubclassMap<SalesPlanWageParameterItem>
	{
		public SalesPlanWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterItemTypes.SalesPlan));
			References(x => x.SalesPlan).Column("sales_plan_id");
		}
	}

	public class RatesLevelWageParameterMap : SubclassMap<RatesLevelWageParameterItem>
	{
		public RatesLevelWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterItemTypes.RatesLevel));
			References(x => x.WageDistrictLevelRates).Column("wage_district_level_rates_id");
		}
	}

	public class OldRatesWageParameterMap : SubclassMap<OldRatesWageParameterItem>
	{
		public OldRatesWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterItemTypes.OldRates));
		}
	}

	public class WithoutWageWageParameterMap : SubclassMap<ManualWageParameterItem>
	{
		public WithoutWageWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterItemTypes.Manual));
		}
	}
}
