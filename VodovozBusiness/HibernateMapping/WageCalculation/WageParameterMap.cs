using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class WageParameterMap : ClassMap<WageParameter>
	{
		public WageParameterMap()
		{
			Table("wage_parameters");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("wage_type");
			Map(x => x.WageParameterType).Column("wage_type").CustomType<WageParameterTypesStringType>().Update().Not.Insert();
			References(x => x.Employee).Column("employee_id");
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.WageParameterTarget).Column("wage_parameter_target").CustomType<WageParameterTargetsStringType>();
			Map(x => x.IsStartedWageParameter).Column("is_started_wage_parameter");
		}
	}

	public class FixedWageParameterMap : SubclassMap<FixedWageParameter>
	{
		public FixedWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.Fixed));
			Map(x => x.RouteListFixedWage).Column("route_list_fixed_wage");
		}
	}

	public class PercentWageParameterMap : SubclassMap<PercentWageParameter>
	{
		public PercentWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.Percent));
			Map(x => x.RouteListPercent).Column("route_list_percent_wage");
			Map(x => x.PercentWageType).Column("percent_wage_type").CustomType<PercentWageTypesStringType>();
		}
	}

	public class SalesPlanWageParameterMap : SubclassMap<SalesPlanWageParameter>
	{
		public SalesPlanWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.SalesPlan));
			References(x => x.SalesPlan).Column("sales_plan_id");
		}
	}

	public class RatesLevelWageParameterMap : SubclassMap<RatesLevelWageParameter>
	{
		public RatesLevelWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.RatesLevel));
			References(x => x.WageDistrictLevelRates).Column("wage_district_level_rates_id");
		}
	}

	public class OldRatesWageParameterMap : SubclassMap<OldRatesWageParameter>
	{
		public OldRatesWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.OldRates));
		}
	}

	public class WithoutWageWageParameterMap : SubclassMap<ManualWageParameter>
	{
		public WithoutWageWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.Manual));
		}
	}
}
