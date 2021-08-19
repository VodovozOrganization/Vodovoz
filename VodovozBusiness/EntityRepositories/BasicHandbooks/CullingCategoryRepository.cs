using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.BasicHandbooks
{
	public class CullingCategoryRepository : ICullingCategoryRepository
	{
		public IList<CullingCategory> All(IUnitOfWork uow)
		{
			return uow.Session.CreateCriteria<CullingCategory>().List<CullingCategory> ();
		}
	}
}

