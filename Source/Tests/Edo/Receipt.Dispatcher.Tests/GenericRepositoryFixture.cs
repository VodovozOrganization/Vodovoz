using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Specifications;

namespace Receipt.Dispatcher.Tests
{
	public class GenericRepositoryFixture<TEntity> : IGenericRepository<TEntity>
		where TEntity : class, IDomainObject
	{
		public GenericRepositoryFixture()
		{
			Data = new ObservableList<TEntity>();
		}

		public IObservableList<TEntity> Data { get; private set; }

		public IEnumerable<TEntity> Get(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate = null, int limit = 0)
		{
			if(limit != 0)
			{
				return Data.Where(predicate.Compile()).Take(limit);
			}

			return Data.Where(predicate.Compile());
		}

		public IEnumerable<TEntity> Get(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification, int limit = 0)
		{
			if(limit != 0)
			{
				return Data.Where(expressionSpecification.IsSatisfiedBy).Take(limit);
			}

			return Data.Where(expressionSpecification.IsSatisfiedBy);
		}

		public IEnumerable<TType> GetValue<TType>(IUnitOfWork unitOfWork, Expression<Func<TEntity, TType>> selector, Expression<Func<TEntity, bool>> predicate = null, int limit = 0)
		{
			if(limit != 0)
			{
				return Data
					.Where(predicate.Compile())
					.Take(limit)
					.Select(selector.Compile());
			}

			return Data
				.Where(predicate.Compile())
				.Select(selector.Compile());
		}
	}
}
