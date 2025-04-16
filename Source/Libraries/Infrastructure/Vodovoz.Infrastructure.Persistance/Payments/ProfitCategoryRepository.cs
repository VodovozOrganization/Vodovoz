using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.Infrastructure.Persistance.Payments
{
	internal sealed class ProfitCategoryRepository : IProfitCategoryRepository
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
