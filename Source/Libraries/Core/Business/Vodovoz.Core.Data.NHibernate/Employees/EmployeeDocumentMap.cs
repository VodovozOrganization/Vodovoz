using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class EmployeeDocumentMap : ClassMap<EmployeeDocumentEntity>
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
