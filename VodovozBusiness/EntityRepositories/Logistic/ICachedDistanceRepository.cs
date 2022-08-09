using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface ICachedDistanceRepository
	{
		IList<CachedDistance> GetCache(IUnitOfWork uow, long[] hash);
		IList<CachedDistance> GetCache(IUnitOfWork uow, WayHash[] hashes);
		CachedDistance GetFirstCacheByCreateDate(IUnitOfWork uow);
	}
}
