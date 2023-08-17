using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintResultOfEmployeesMap : SubclassMap<ComplaintResultOfEmployees>
	{
		public ComplaintResultOfEmployeesMap()
		{
			DiscriminatorValue("Employees");
		}
	}
}
