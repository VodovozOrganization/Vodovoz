using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.BasicHandbooks;

namespace Vodovoz.Infrastructure.Persistance.BasicHandbooks
{
	internal sealed class CullingCategoryRepository : ICullingCategoryRepository
	{
		public IList<CullingCategory> GetAllCullingCategories(IUnitOfWork uow) => uow.GetAll<CullingCategory>().ToList();
	}
}

