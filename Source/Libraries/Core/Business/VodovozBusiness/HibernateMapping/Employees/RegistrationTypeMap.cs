using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
	public class RegistrationTypeMap : ClassMap<RegistrationType>
	{
		public RegistrationTypeMap()
		{
			Table("registrations_types");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
