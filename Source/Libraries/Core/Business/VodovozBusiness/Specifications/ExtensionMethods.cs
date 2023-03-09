using QS.DomainModel.Entity;
using System.Linq;

namespace Vodovoz.Specifications
{
	public static class ExtensionMethods
	{
		public static ISpecification<TEntity> And<TEntity>(this ISpecification<TEntity> spec1, ISpecification<TEntity> spec2)
		{
			return new AndSpecification<TEntity>(spec1, spec2);
		}

		public static ISpecification<TEntity> Or<TEntity>(this ISpecification<TEntity> spec1, ISpecification<TEntity> spec2)
		{
			return new OrSpecification<TEntity>(spec1, spec2);
		}

		public static ISpecification<TEntity> Not<TEntity>(this ISpecification<TEntity> spec)
		{
			return new NotSpecification<TEntity>(spec);
		}

		public static IQueryable<TEntity> Specification<TEntity>(
			this IQueryable<TEntity> query,
			ISpecification<TEntity> specification)
			where TEntity : IDomainObject
		{
			return query.Where(specification.IsSatisfiedBy());
		}
	}
}
