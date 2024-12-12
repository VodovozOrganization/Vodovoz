using Core.Infrastructure.Specifications;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Vodovoz.Core.Domain.Repositories
{
	public interface IGenericRepository<TEntity> where TEntity : class, IDomainObject
	{
		IEnumerable<TEntity> Get(
			IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0);

		IEnumerable<TEntity> Get(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification, int limit = 0);
		IEnumerable<TType> GetValue<TType>(IUnitOfWork unitOfWork, Expression<Func<TEntity, TType>> selector, Expression<Func<TEntity, bool>> predicate = null, int limit = 0);
	}
}
