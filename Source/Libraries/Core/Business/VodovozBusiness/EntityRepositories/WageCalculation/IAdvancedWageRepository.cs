using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.EntityRepositories.WageCalculation
{
	public interface IAdvancedWageRepository
	{
		IEnumerable<AdvancedWageParameter> GetChildParameters(IUnitOfWork uow, AdvancedWageParameter parentParameter);
		IEnumerable<AdvancedWageParameter> GetRootParameter(IUnitOfWork uow, WageRate wageRate);
	}
}