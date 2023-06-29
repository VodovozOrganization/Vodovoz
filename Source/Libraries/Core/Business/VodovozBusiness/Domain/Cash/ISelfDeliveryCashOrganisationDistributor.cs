using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Cash
{
	public interface ISelfDeliveryCashOrganisationDistributor
	{
		void DistributeExpenseCash(IUnitOfWork uow, Order selfDeliveryOrder, Expense expense);
		void DistributeIncomeCash(IUnitOfWork uow, Order selfDeliveryOrder, Income income);
		void UpdateRecords(IUnitOfWork uow, Order selfDeliveryOrder, Expense expense, Employee editor);
		void UpdateRecords(IUnitOfWork uow, Order selfDeliveryOrder, Income income, Employee editor);
	}
}