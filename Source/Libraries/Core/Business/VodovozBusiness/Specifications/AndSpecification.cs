using System;
using System.Linq.Expressions;

namespace Vodovoz.Specifications
{
	internal class AndSpecification<TEntity> : ISpecification<TEntity>
	{
		private readonly ISpecification<TEntity> _spec1;
		private readonly ISpecification<TEntity> _spec2;

		protected ISpecification<TEntity> Spec1 => _spec1;

		protected ISpecification<TEntity> Spec2 => _spec2;

		internal AndSpecification(ISpecification<TEntity> spec1, ISpecification<TEntity> spec2)
		{
			_spec1 = spec1 ?? throw new ArgumentNullException(nameof(spec1));
			_spec2 = spec2 ?? throw new ArgumentNullException(nameof(spec2));
		}

		public Expression<Func<TEntity, bool>> IsSatisfiedBy()
		{
			var expression1 = _spec1.IsSatisfiedBy();
			var expression2 = _spec2.IsSatisfiedBy();

			var combinedBody = Expression.AndAlso(expression1.Body, expression2.Body);
			return Expression.Lambda<Func<TEntity, bool>>(combinedBody, expression1.Parameters[0]);
		}
	}
}
