using FluentNHibernate.Mapping;
using Vodovoz.Core.Data.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mappings
{
	public class EmployeeWithLoginMap : ClassMap<EmployeeWithLogin>
	{
		public EmployeeWithLoginMap()
		{
			Table("employees");
			
			Id(x => x.Id).Column("id");
			
			Map(x => x.Name).Column("name").ReadOnly();
			Map(x => x.LastName).Column("last_name").ReadOnly();
			Map(x => x.Patronymic).Column("patronymic").ReadOnly();
			
			HasMany(x => x.ExternalApplicationUsers)
				.Cascade.AllDeleteOrphan().LazyLoad().Inverse().KeyColumn("employee_id");
		}
	}
}
