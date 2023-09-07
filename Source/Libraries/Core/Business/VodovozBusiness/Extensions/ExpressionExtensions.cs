using System.Linq.Expressions;

namespace Vodovoz.Extensions
{
	public static class ExpressionExtensions
	{
		public static Expression<T> CombineWith<T>(this Expression<T> firstExpression, Expression<T> secondExpression)
			=> Combine(firstExpression, secondExpression);

		public static Expression<T> Combine<T>(Expression<T> firstExpression, Expression<T> secondExpression)
		{
			if(firstExpression is null)
			{
				return secondExpression;
			}

			if(secondExpression is null)
			{
				return firstExpression;
			}

			var invokedExpression = Expression.Invoke(
				secondExpression,
				firstExpression.Parameters);

			var combinedExpression = Expression.AndAlso(firstExpression.Body, invokedExpression);

			return Expression.Lambda<T>(combinedExpression, firstExpression.Parameters);
		}
	}
}
