using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class FinancialResponsibilityCenterMap : ClassMap<FinancialResponsibilityCenter>
	{
		public FinancialResponsibilityCenterMap()
		{
			Table("financial_responsibility_centers");
			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.ResponsibleEmployeeId).Column("responsible_employee_id");
			Map(x => x.ViceResponsibleEmployeeId).Column("vice_responsible_employee_id");

			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.RequestApprovalDenied).Column("request_approval_denied");
		}
	}
}
