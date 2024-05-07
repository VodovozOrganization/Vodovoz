using System;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.Core.Domain.Specifications
{
	public static class ExpressionSpecificationOrExtension
	{
		public static ExpressionSpecification<T> Or<T>(this ExpressionSpecification<T> specificationLeft, ExpressionSpecification<T> specificationRight)
		{
			Expression<Func<T, bool>> resultExpression;

			ParameterExpression param = specificationLeft.Expression.Parameters.First();

			if(ReferenceEquals(param, specificationRight.Expression.Parameters.First()))
			{
				resultExpression = Expression.Lambda<Func<T, bool>>(
					Expression.OrElse(specificationLeft.Expression.Body, specificationRight.Expression.Body), param);
			}
			else
			{
				resultExpression = Expression.Lambda<Func<T, bool>>(
					Expression.OrElse(
						specificationLeft.Expression.Body,
						Expression.Invoke(specificationRight.Expression, param)), param);
			}

			var combinedSpecification = new DynamicExpressionSpecification<T>(resultExpression);
			return combinedSpecification;
		}
	}
}
