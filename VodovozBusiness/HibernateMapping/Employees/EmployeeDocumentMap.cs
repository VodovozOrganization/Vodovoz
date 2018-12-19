//using System;
//using FluentNHibernate.Mapping;
//using FluentNHibernate.MappingModel;
//using Vodovoz.Domain.Employees;
//using Vodovoz.Domain.Logistic;

//namespace Vodovoz.HibernateMapping.Employees
//{
//	public class EmployeeDocumentMap : ClassMap<EmployeeDocument>
//	{
//		public EmployeeDocumentMap()
//		{
//			Table("passports");
//			Id(x => x.Id).Column("id").GeneratedBy.Native();
//			DiscriminateSubClassesOnColumn("employee_type");

//			Map(x => x.document).Column("document").CustomType<DocumentTypeStringType>();
//			Map(x => x.PassportSeria).Column("passport_seria");
//			Map(x => x.PassportNumber).Column("passport_number");
//			Map(x => x.PassportIssuedOrg).Column("passport_issued_org");
//			Map(x => x.PassportIssuedDate).Column("passport_issued_date");
//		}
//	}
//}
