using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class EmployeeRegistrationVersionMap : ClassMap<EmployeeRegistrationVersion>
	{
		public EmployeeRegistrationVersionMap()
		{
			Table("employees_registrations_versions");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");

			References(x => x.Employee).Column("employee_id");
			References(x => x.EmployeeRegistration)
				.Column("employee_registration_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
