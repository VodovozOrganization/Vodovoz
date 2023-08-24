using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.EntityRepositories
{
	public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IDomainObject
	{
		public IEnumerable<TEntity> Get(
			IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0)
		{
			if(limit != 0)
			{
				return GetQueriable(unitOfWork, predicate).Take(limit).ToList();
			}

			return GetQueriable(unitOfWork, predicate).ToList();
		}

		private IQueryable<TEntity> GetQueriable(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate = null)
		{
			if(predicate is null)
			{
				return unitOfWork.Session
					.Query<TEntity>();
			}

			return unitOfWork.Session.Query<TEntity>()
				.Where(predicate);
		}
	}
}
