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

			Map(x => x.FirstDay).Column("first_day");
			Map(x => x.LastDay).Column("last_day");
			Map(x => x.ContractDate).Column("contract_date");
			Map(x => x.Name).Column("name");
			Map(x=>x.TemplateFile).Column("template_file");
			References(x => x.EmployeeContractTemplate).Column("employee_contract_template_id");
			References(x => x.Organization).Column("organization_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Document).Column("document_id");
		}
	}
}
