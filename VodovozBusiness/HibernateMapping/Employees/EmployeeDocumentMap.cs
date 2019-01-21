using System;
using FluentNHibernate.Mapping;
using FluentNHibernate.MappingModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Employees
{
	public class EmployeeDocumentMap : ClassMap<EmployeeDocument>
	{
		public EmployeeDocumentMap()
		{
			Table("employee_documents");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PassportSeria).Column("passport_seria");
			Map(x => x.PassportNumber).Column("passport_number");
			Map(x => x.PassportIssuedOrg).Column("passport_issued_org");
			Map(x => x.PassportIssuedDate).Column("passport_issued_date");
			Map(x => x.Document).Column("document_type");
			Map(x => x.MainDocument).Column("main_document");
			Map(x => x.Name).Column("name");
		}
	}
}
