using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
	public interface IProfitCategoryRepository
	{
		IEnumerable<ProfitCategory> GetAllProfitCategories(IUnitOfWork uow);
		ProfitCategory GetProfitCategoryById(IUnitOfWork uow, int profitCategoryId);
	}
}
