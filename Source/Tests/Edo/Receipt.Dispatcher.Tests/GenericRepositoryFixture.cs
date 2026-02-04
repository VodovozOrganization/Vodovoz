using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
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

		public async Task<Result<IEnumerable<TEntity>>> GetAsync(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate = null, int limit = 0, CancellationToken cancellationToken = default)
		{
			return await Task.FromResult(Result.Success(Get(unitOfWork, predicate, limit)));
		}

		public async Task<Result<IEnumerable<TResult>>> GetAsync<TResult>(IUnitOfWork unitOfWork, Func<TEntity, TResult> map, Expression<Func<TEntity, bool>> predicate = null, int limit = 0)
		{
			return await GetAsync(unitOfWork, predicate, limit)
				.MapAsync(x => x.Select(map));
		}

		public async Task<Result<IEnumerable<TEntity>>> GetAsync(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification, int limit = 0, CancellationToken cancellationToken = default)
		{
			return await GetAsync(unitOfWork, expressionSpecification.Expression, limit, cancellationToken);
		}

		public int GetCount(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate)
		{
			return Data.Count(predicate.Compile());
		}

		public int GetCount(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return Data.Where(expressionSpecification.Expression.Compile()).Count();
		}

		public TEntity GetFirstOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate)
		{
			return Data.FirstOrDefault(predicate.Compile());
		}

		public TEntity GetFirstOrDefault(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return Data.Where(expressionSpecification.Expression.Compile()).FirstOrDefault();
		}

		public TEntity GetLastOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate)
		{
			return Data.LastOrDefault(predicate.Compile());
		}

		public TEntity GetLastOrDefault(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return Data.Where(expressionSpecification.Expression.Compile()).OrderByDescending(x => x.Id).FirstOrDefault();
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
