using System;
using System.Linq.Expressions;

namespace Vodovoz.Core.Domain.Specifications
{
	public static class ExpressionSpecificationNotExtension
	{
		public static ExpressionSpecification<T> Not<T>(this ExpressionSpecification<T> specification)
		{
			Expression<Func<T, bool>> resultExpression;

			resultExpression = Expression.Lambda<Func<T, bool>>(
				Expression.Not(specification.Expression));

			return new DynamicExpressionSpecification<T>(resultExpression);
		}
	}
}
