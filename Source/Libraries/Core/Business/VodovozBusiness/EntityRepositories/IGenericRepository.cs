using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Vodovoz.EntityRepositories
{
	public interface IGenericRepository<TEntity> where TEntity : class, IDomainObject
	{
		IEnumerable<TEntity> Get(
			IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0);
	}
}
