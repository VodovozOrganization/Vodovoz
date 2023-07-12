using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface ICategoryRepository
	{
		IList<ExpenseCategory> ExpenseSelfDeliveryCategories(IUnitOfWork uow);
	}
}
