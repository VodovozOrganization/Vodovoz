using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Specifications;

namespace Vodovoz.Core.Domain.Subdivisions
{
	public static class SubdivisionSpecifications
	{
		public static DynamicExpressionSpecification<SubdivisionEntity> ForEmployeeIsChief(int employeeId)
			=> new DynamicExpressionSpecification<SubdivisionEntity>(s => s.ChiefId == employeeId);

		public static DynamicExpressionSpecification<SubdivisionEntity> ForFinancialResponsibilityCenter(int? financialResponsibilityCenterId)
			=> new DynamicExpressionSpecification<SubdivisionEntity>(s => s.FinancialResponsibilityCenterId == financialResponsibilityCenterId);

		public static DynamicExpressionSpecification<SubdivisionEntity> ForFinancialResponsibilityCenters(IEnumerable<int> financialResponcibilitiesCentersIds)
			=> new DynamicExpressionSpecification<SubdivisionEntity>(s => financialResponcibilitiesCentersIds.Contains(s.FinancialResponsibilityCenterId.Value));
	}
}
