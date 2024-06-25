using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.BasicHandbooks
{
	public class CullingCategoryRepository : ICullingCategoryRepository
	{
		public IList<CullingCategory> GetAllCullingCategories(IUnitOfWork uow) => uow.GetAll<CullingCategory>().ToList();
	}
}

