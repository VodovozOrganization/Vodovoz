using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
	public interface IProfitCategoryRepository
	{
		IEnumerable<ProfitCategory> GetAllProfitCategories(IUnitOfWork uow);
		ProfitCategory GetProfitCategory(IUnitOfWork uow, int profitCategoryId);
	}
}
