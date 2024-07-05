﻿using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Payments;

namespace Vodovoz.EntityRepositories.Payments
{
	public class ProfitCategoryRepository : IProfitCategoryRepository
	{
		public IEnumerable<ProfitCategory> GetAllProfitCategories(IUnitOfWork uow)
		{
			return uow.GetAll<ProfitCategory>();
		}

		public ProfitCategory GetProfitCategoryById(IUnitOfWork uow, int profitCategoryId)
		{
			return uow.Session.QueryOver<ProfitCategory>()
				.Where(pc => pc.Id == profitCategoryId)
				.SingleOrDefault();
		}
	}
}
