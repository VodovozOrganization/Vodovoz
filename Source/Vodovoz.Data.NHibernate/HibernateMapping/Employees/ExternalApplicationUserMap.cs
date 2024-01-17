using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class ExternalApplicationUserMap : ClassMap<ExternalApplicationUser>
	{
		public ExternalApplicationUserMap()
		{
			Table("external_applications_users");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Login).Column("login");
			Map(x => x.Password).Column("password");
			Map(x => x.SessionKey).Column("session_key");
			Map(x => x.Token).Column("token");
			Map(x => x.ExternalApplicationType).Column("external_application_type");

			References(x => x.Employee).Column("employee_id");
		}
	}
}
