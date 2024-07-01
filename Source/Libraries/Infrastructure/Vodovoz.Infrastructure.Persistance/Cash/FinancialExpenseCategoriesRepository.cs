using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class FinancialExpenseCategoriesRepository : IFinancialExpenseCategoriesRepository
	{
		public IEnumerable<FinancialExpenseCategory> Get(IUnitOfWork unitOfWork, Expression<Func<FinancialExpenseCategory, bool>> predicate)
		{
			if(predicate is null)
			{
				return unitOfWork.Session.Query<FinancialExpenseCategory>().ToList();
			}

			return unitOfWork.Session.Query<FinancialExpenseCategory>()
				.Where(predicate)
				.ToList();
		}

		public TargetDocument? GetExpenseCategoryTargetDocument(IUnitOfWork unitOfWork, int? expenseCategoryId)
		{
			if(expenseCategoryId is null)
			{
				return null;
			}

			return (from expenseCategory in unitOfWork.Session.Query<FinancialExpenseCategory>()
					where expenseCategory.Id == expenseCategoryId
					select expenseCategory.TargetDocument)
				   .FirstOrDefault();
		}
	}
}
