using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.EntityRepositories.BasicHandbooks
{
	public interface ICullingCategoryRepository
	{
		IList<CullingCategory> GetAllCullingCategories(IUnitOfWork uow);
	}
}
