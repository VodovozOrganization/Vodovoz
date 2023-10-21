using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class EmployeeMap : SubclassMap<Employee>
	{
		public EmployeeMap()
		{
			Map(x => x.DriverOfCarTypeOfUse).Column("driver_of_car_type_of_use");
			Map(x => x.DriverOfCarOwnType).Column("driver_of_car_own_type");

			References(x => x.Nationality).Column("nationality_id");
			References(x => x.Citizenship).Column("citizenship_id");
			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.User).Column("user_id");
			References(x => x.DefaultForwarder).Column("default_forwarder_id");
			References(x => x.OrganisationForSalary).Column("organisation_for_salary_id");
			References(x => x.Post).Column("employees_posts_id");

			HasMany(x => x.Accounts).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Phones).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Documents).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Attachments).Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("employee_id");
			HasMany(x => x.Contracts).Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
			HasMany(x => x.WageParameters).Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
			HasMany(x => x.EmployeeRegistrationVersions)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id")
				.OrderBy("start_date DESC");

			HasMany(x => x.DriverWorkScheduleSets)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("driver_id")
				.OrderBy("date_activated DESC");

			HasMany(x => x.DriverDistrictPrioritySets)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("driver_id")
				.OrderBy("date_created DESC");
		}
	}
}
