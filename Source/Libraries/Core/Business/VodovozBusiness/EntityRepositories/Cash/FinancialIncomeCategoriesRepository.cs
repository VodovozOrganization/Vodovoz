using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.EntityRepositories.Cash
{
	public class FinancialIncomeCategoriesRepository : IFinancialIncomeCategoriesRepository
	{
		public TargetDocument? GetIncomeCategoryTargetDocument(IUnitOfWork unitOfWork, int? incomeCategoryId)
		{
			if(incomeCategoryId is null)
			{
				return null;
			}

			return (from incomeCategory in unitOfWork.GetAll<FinancialIncomeCategory>()
					where incomeCategory.Id == incomeCategoryId
					select incomeCategory.TargetDocument)
				   .FirstOrDefault();
		}
	}
}
