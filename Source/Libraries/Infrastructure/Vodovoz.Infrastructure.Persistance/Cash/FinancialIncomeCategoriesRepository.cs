using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class FinancialIncomeCategoriesRepository : IFinancialIncomeCategoriesRepository
	{
		public IEnumerable<FinancialIncomeCategory> Get(IUnitOfWork unitOfWork, Expression<Func<FinancialIncomeCategory, bool>> predicate)
		{
			if(predicate is null)
			{
				return unitOfWork.Session.Query<FinancialIncomeCategory>().ToList();
			}

			return unitOfWork.Session.Query<FinancialIncomeCategory>()
				.Where(predicate)
				.ToList();
		}

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
