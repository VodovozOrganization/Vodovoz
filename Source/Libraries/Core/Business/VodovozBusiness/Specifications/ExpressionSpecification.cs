using System;
using System.Linq.Expressions;

namespace Vodovoz.Specifications
{
	public class ExpressionSpecification<T> : ISpecification<T>
	{
		public Expression<Func<T, bool>> Expression { get; }

		private Func<T, bool> _expressionFunc;
		private Func<T, bool> ExpressionFunc => _expressionFunc ?? (_expressionFunc = Expression.Compile());

		public ExpressionSpecification(Expression<Func<T, bool>> expression)
		{
			Expression = expression;
		}

		public bool IsSatisfiedBy(T entity) => ExpressionFunc(entity);
	}
}
