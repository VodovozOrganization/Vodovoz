using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface IFinancialExpenseCategoriesRepository
	{
		IEnumerable<FinancialExpenseCategory> Get(IUnitOfWork unitOfWork, Expression<Func<FinancialExpenseCategory, bool>> predicate);
		TargetDocument? GetExpenseCategoryTargetDocument(IUnitOfWork unitOfWor, int? expenseCategoryId);
	}
}
