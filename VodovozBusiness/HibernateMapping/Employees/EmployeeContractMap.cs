using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HibernateMapping.Employees
{
	public class EmployeeContractMap : ClassMap<EmployeeContract>
	{
		public EmployeeContractMap()
		{
			Table("employee_contracts");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.FirstDay).Column("first_day");
			Map(x => x.LastDay).Column("last_day");
			References(x => x.Document).Column("document_id");
		}
	}
}
