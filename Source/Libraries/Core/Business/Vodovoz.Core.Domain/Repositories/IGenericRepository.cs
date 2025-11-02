using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Specifications;

namespace Vodovoz.Core.Domain.Repositories
{
	/// <summary>
	/// Репозиторий для работы с сущностями.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface IGenericRepository<TEntity> where TEntity : class, IDomainObject
	{
		/// <summary>
		/// Получение сущностей из базы данных.
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="predicate"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		IEnumerable<TEntity> Get(
			IUnitOfWork unitOfWork,
			Expression<Func<TEntity, bool>> predicate = null,
			int limit = 0);

		/// <summary>
		/// Получение сущностей из базы данных с использованием спецификации.
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="expressionSpecification"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		IEnumerable<TEntity> Get(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification, int limit = 0);

		/// <summary>
		/// Получение сущностей из базы данных.
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="predicate"></param>
		/// <param name="limit"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<Result<IEnumerable<TEntity>>> GetAsync(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate = null, int limit = 0, CancellationToken cancellationToken = default);

		/// <summary>
		/// Получение сущностей из базы данных и маппинг в другой тип.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="unitOfWork"></param>
		/// <param name="map"></param>
		/// <param name="predicate"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		Task<Result<IEnumerable<TResult>>> GetAsync<TResult>(IUnitOfWork unitOfWork, Func<TEntity, TResult> map, Expression<Func<TEntity, bool>> predicate = null, int limit = 0);

		/// <summary>
		/// Получение 1го элемента из базы данных.
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TEntity GetFirstOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate);
		TEntity GetFirstOrDefault(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification);

		/// <summary>
		/// Получение последнего элемента из базы данных.
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TEntity GetLastOrDefault(IUnitOfWork unitOfWork, Expression<Func<TEntity, bool>> predicate);
		TEntity GetLastOrDefault(IUnitOfWork unitOfWork, ExpressionSpecification<TEntity> expressionSpecification);

		/// <summary>
		/// Получение значения из базы данных.
		/// </summary>
		/// <typeparam name="TType"></typeparam>
		/// <param name="unitOfWork"></param>
		/// <param name="selector"></param>
		/// <param name="predicate"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		IEnumerable<TType> GetValue<TType>(IUnitOfWork unitOfWork, Expression<Func<TEntity, TType>> selector, Expression<Func<TEntity, bool>> predicate = null, int limit = 0);
	}
}
