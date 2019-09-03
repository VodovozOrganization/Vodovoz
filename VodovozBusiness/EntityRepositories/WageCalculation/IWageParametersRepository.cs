using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.EntityRepositories.WageCalculation
{
	public interface IWageParametersRepository
	{
		IList<WageParameter> WageParameters(IUnitOfWork uow, bool hideArchive = true);
		IList<WageParameter> AvailableWageParametersForEmployeeCategory(IUnitOfWork uow, EmployeeCategory? eCategory = null);
	}
}
