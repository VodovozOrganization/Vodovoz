using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface IFinancialIncomeCategoriesRepository
	{
		TargetDocument? GetIncomeCategoryTargetDocument(IUnitOfWork unitOfWork, int? incomeCategoryId);
	}
}