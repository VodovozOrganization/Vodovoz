using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public interface IWageParameterService
	{
		IRouteListWageCalculationService ActualizeWageParameterAndGetCalculationService(IUnitOfWork uow, Employee employee, IRouteListWageCalculationSource source);
	}
}