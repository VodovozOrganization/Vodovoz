using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class EmployeeFileInformationMap : ClassMap<EmployeeFileInformation>
	{
		public EmployeeFileInformationMap()
		{
			Table("employee_file_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.EmployeeId).Column("employee_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
