using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface IFinancialIncomeCategoriesRepository
	{
		IEnumerable<FinancialIncomeCategory> Get(IUnitOfWork unitOfWork, Expression<Func<FinancialIncomeCategory, bool>> predicate);
		TargetDocument? GetIncomeCategoryTargetDocument(IUnitOfWork unitOfWork, int? incomeCategoryId);
	}
}