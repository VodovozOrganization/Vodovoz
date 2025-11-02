
using Vodovoz.Core.Domain.Specifications;

namespace Vodovoz.Core.Domain.Cash
{
	public static class FinancialResponsibilityCenterSpecifications
	{
		public static ExpressionSpecification<FinancialResponsibilityCenter> ForId(int? id)
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.Id == id);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForResponsibleEmployeeId(int responsibleEmployeeId)
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.ResponsibleEmployeeId == responsibleEmployeeId);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForViceResponsibleEmployeeId(int viceResponsibleEmployeeId)
			=> new	ExpressionSpecification<FinancialResponsibilityCenter>(s => s.ViceResponsibleEmployeeId == viceResponsibleEmployeeId);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForIsArchive()
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.IsArchive == true);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForIsNotArchive()
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.IsArchive == false);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForRequestApprovalIsDenied()
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.RequestApprovalDenied == true);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForRequestApprovalIsAllowed()
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.RequestApprovalDenied == false);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForName(string name)
			=> new ExpressionSpecification<FinancialResponsibilityCenter>(s => s.Name == name);

		public static ExpressionSpecification<FinancialResponsibilityCenter> ForEmployeeIdIsResponsible(int employeeId)
			=> ForResponsibleEmployeeId(employeeId)
				.Or(ForViceResponsibleEmployeeId(employeeId));
	}
}
