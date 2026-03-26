using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using NHibernate.Util;
using System.Linq.Expressions;
using System.Reflection;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.NhibernateExtensions
{
	public class CustomOrderDepositItemActualSumGenerator : BaseHqlGeneratorForProperty
	{
		public CustomOrderDepositItemActualSumGenerator()
		{
			SupportedProperties = new MemberInfo[] { ReflectHelper.GetProperty<OrderDepositItem, decimal>(x => x.ActualSum) };
		}

		public override HqlTreeNode BuildHql(
			MemberInfo member,
			Expression expression,
			HqlTreeBuilder treeBuilder,
			IHqlExpressionVisitor visitor)
		{
			HqlExpression deposit = visitor.Visit(expression).AsExpression();

			HqlExpression actualCount = visitor.Visit(expression).AsExpression();

			HqlExpression count = visitor.Visit(expression).AsExpression();

			return treeBuilder.MethodCall(
				"ROUND",
				treeBuilder.Multiply(
					treeBuilder.Dot(deposit, treeBuilder.Ident("Deposit")),
					treeBuilder.MethodCall("COALESCE",
						treeBuilder.Dot(actualCount, treeBuilder.Ident("ActualCount")),
						treeBuilder.Dot(count, treeBuilder.Ident("Count")))),
				treeBuilder.Constant(2));
		}
	}
}
