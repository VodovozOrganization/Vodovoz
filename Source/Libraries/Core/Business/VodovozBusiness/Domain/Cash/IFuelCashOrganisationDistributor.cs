using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Cash
{
	public interface IFuelCashOrganisationDistributor
	{
		void DistributeCash(IUnitOfWork uow, FuelDocument fuelDoc);
		void UpdateRecords(IUnitOfWork uow, FuelExpenseCashDistributionDocument document, Expense expense, Employee editor);
	}
}