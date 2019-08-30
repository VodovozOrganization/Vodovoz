using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.EntityRepositories.WageCalculation
{
	public class WageParametersRepository : IWageParametersRepository
	{
		public IList<WageParameter> AvailableWageParametersForEmployeeCategory(IUnitOfWork uow, EmployeeCategory? eCategory = null)
		{
			var wageTypes = WageParameter.WageCalculationTypesForEmployeeCategory(eCategory);
			var result = uow.Session.QueryOver<WageParameter>()
							.Where(p => p.WageCalcType.IsIn(wageTypes))
							.List()
							;
			return result;
		}

		public IList<WageParameter> WageParameters(IUnitOfWork uow, bool hideArchive = true)
		{
			var result = uow.Session.QueryOver<WageParameter>()
							.Where(p => !p.IsArchive)
							.List()
							;
			return result;
		}
	}
}