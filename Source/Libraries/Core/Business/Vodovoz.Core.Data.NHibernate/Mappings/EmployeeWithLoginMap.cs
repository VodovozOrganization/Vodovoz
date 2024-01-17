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
			Map(x => x.UserLogin).Column("android_login").ReadOnly();
		}
	}
}
