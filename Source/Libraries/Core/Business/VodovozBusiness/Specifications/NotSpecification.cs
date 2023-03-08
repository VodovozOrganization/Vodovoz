using System;
using System.Linq.Expressions;

namespace Vodovoz.Specifications
{
	internal class NotSpecification<TEntity> : ISpecification<TEntity>
	{
		private readonly ISpecification<TEntity> _wrapped;

		protected ISpecification<TEntity> Wrapped => _wrapped;

		internal NotSpecification(ISpecification<TEntity> spec)
		{
			_wrapped = spec ?? throw new ArgumentNullException(nameof(spec));
		}

		public Expression<Func<TEntity, bool>> IsSatisfiedBy()
		{
			var expression = _wrapped.IsSatisfiedBy();
			return Expression.Lambda<Func<TEntity, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
		}
	}
}
