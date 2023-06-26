using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.EntityRepositories.Cash
{
	public class FinancialExpenseCategoriesRepository : IFinancialExpenseCategoriesRepository
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

			return (from incomeCategory in unitOfWork.Session.Query<FinancialIncomeCategory>()
					where incomeCategory.Id == expenseCategoryId
					select incomeCategory.TargetDocument)
				   .FirstOrDefault();
		}
	}
}
