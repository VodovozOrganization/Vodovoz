using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Cash
{
	public interface IRouteListCashOrganisationDistributor
	{
		void DistributeExpenseCash(IUnitOfWork uow, RouteList routeList, Expense expense, decimal amount);
		void DistributeIncomeCash(IUnitOfWork uow, RouteList routeList, Income income, decimal amount);
		void UpdateIncomeCash(IUnitOfWork uow, RouteList routeList, Income income, decimal amount);
	}
}