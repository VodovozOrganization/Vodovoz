using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Specifications;

namespace Vodovoz.Infrastructure.Persistance
{
	/// <summary>
	/// Реализация репозитория для работы с сущностями.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IDomainObject
	{
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public IEnumerable<TType> GetValue<TType>(
			IUnitOfWork unitOfWork,
			Expression<Func<TEntity, TType>> selector,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0)
		{
			if(limit != 0)
			{
				return GetQueriable(unitOfWork, predicate).Take(limit).Select(selector).ToList();
			}

			return GetQueriable(unitOfWork, predicate).Select(selector).ToList();
		}

		/// <inheritdoc/>
		public IEnumerable<TEntity> Get(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification, int limit = 0)
		{
			return Get(unitOfWork, expressionSpecification.Expression).ToList();
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public TEntity GetFirstOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate)
		{
			return GetQueriable(unitOfWork, predicate).FirstOrDefault();
		}

		/// <inheritdoc/>
		public TEntity GetLastOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate)
		{
			return GetQueriable(unitOfWork, predicate).LastOrDefault();
		}

		/// <inheritdoc/>
		public int GetCount(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate)
		{
			return GetQueriable(unitOfWork, predicate).Count();
		}

		/// <inheritdoc/>
		public async Task<Result<IEnumerable<TEntity>>> GetAsync(
			IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0,
			CancellationToken cancellationToken = default)
		{
			if(limit != 0)
			{
				return GetQueriable(unitOfWork, predicate).Take(limit).ToList();
			}

			return await GetQueriable(unitOfWork, predicate)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<Result<IEnumerable<TResult>>> GetAsync<TResult>(
			IUnitOfWork unitOfWork,
			Func<TEntity, TResult> map,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0)
		{
			return await GetAsync(unitOfWork, predicate, limit)
				.MapAsync(entities => entities.Select(x => map(x)));
		}

		/// <inheritdoc/>
		public async Task<Result<IEnumerable<TEntity>>> GetAsync(
			IUnitOfWork unitOfWork,
			ExpressionSpecification<TEntity> expressionSpecification,
			int limit = 0,
			CancellationToken cancellationToken = default)
		{
			if(limit != 0)
			{
				return GetQueriable(unitOfWork, expressionSpecification.Expression).Take(limit).ToList();
			}

			return await GetQueriable(unitOfWork, expressionSpecification.Expression)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public TEntity GetFirstOrDefault(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return GetQueriable(unitOfWork, expressionSpecification).FirstOrDefault();
		}

		/// <inheritdoc/>
		public TEntity GetLastOrDefault(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return GetQueriable(unitOfWork, expressionSpecification).OrderByDescending(x => x.Id).FirstOrDefault();
		}

		/// <inheritdoc/>
		public int GetCount(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return GetQueriable(unitOfWork, expressionSpecification).Count();
		}

		/// <inheritdoc/>
		private IQueryable<TEntity> GetQueriable(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification)
		{
			return unitOfWork.Session.Query<TEntity>()
				.Where(expressionSpecification.Expression);
		}
	}
}
