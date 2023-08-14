using QS.DomainModel.Entity;
using System.Collections.Generic;

namespace Vodovoz.CachingRepositories.Common
{
	public interface IDomainEntityNodeInMemoryCacheRepository<TEntity>
		where TEntity : IDomainObject
	{
		void InvalidateById(int id);
		void WarmUpCacheWithIds(IEnumerable<int> ids);
		string GetTitleById(int id);
	}
}
