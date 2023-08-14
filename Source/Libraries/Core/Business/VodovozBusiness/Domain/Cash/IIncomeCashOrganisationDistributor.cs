using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Cash
{
	public interface IIncomeCashOrganisationDistributor
	{
		void DistributeCashForIncome(IUnitOfWork uow, Income income, Organization organisation = null);
		void UpdateRecords(IUnitOfWork uow, IncomeCashDistributionDocument document, Income income, Employee editor);
	}
}