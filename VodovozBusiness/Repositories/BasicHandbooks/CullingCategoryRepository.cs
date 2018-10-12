using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace Vodovoz.Repository
{
	public static class CullingCategoryRepository
	{
		public static IList<CullingCategory> All (IUnitOfWork uow)
		{
			return uow.Session.CreateCriteria<CullingCategory> ().List<CullingCategory> ();
		}
	}
}

