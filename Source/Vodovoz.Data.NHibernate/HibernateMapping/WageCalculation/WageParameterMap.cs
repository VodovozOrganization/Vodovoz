using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Data.NHibernate.HibernateMapping.WageCalculation
{
	public class WageParameterMap : ClassMap<WageParameter>
	{
		public WageParameterMap()
		{
			Table("wage_parameters");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("wage_parameter_type");
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.IsStartedWageParameter).Column("is_started_wage_parameter");
		}
	}

	public class EmployeeWageParameterMap : SubclassMap<EmployeeWageParameter>
	{
		public EmployeeWageParameterMap()
		{
			DiscriminatorValue(nameof(WageParameterTypes.ForEmployee));
			References(x => x.Employee).Column("employee_id");
			References(x => x.WageParameterItem).Column("wage_parameter_item_id").Cascade.All();
			References(x => x.WageParameterItemForOurCars).Column("driver_with_our_cars_wage_parameter_item_id").Cascade.All();
			References(x => x.WageParameterItemForRaskatCars).Column("raskat_cars_wage_parameter_item_id").Cascade.All();
		}
	}
}
