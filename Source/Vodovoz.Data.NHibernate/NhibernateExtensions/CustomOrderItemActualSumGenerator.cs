using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using NHibernate.Util;
using System.Linq.Expressions;
using System.Reflection;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.NhibernateExtensions
{
	public class CustomOrderItemActualSumGenerator : BaseHqlGeneratorForProperty
	{
		public CustomOrderItemActualSumGenerator()
		{
			SupportedProperties = new MemberInfo[] { ReflectHelper.GetProperty<OrderItem, decimal>(x => x.ActualSum) };
		}

		public override HqlTreeNode BuildHql(
			MemberInfo member,
			Expression expression,
			HqlTreeBuilder treeBuilder,
			IHqlExpressionVisitor visitor)
		{
			HqlExpression orderItemPrice = visitor.Visit(expression).AsExpression();

			HqlExpression orderItemActualCount = visitor.Visit(expression).AsExpression();

			HqlExpression orderItemCount = visitor.Visit(expression).AsExpression();

			HqlExpression orderItemDiscountMoney = visitor.Visit(expression).AsExpression();

			return treeBuilder.MethodCall(
				"ROUND",
				treeBuilder.Subtract(
					treeBuilder.Multiply(
						treeBuilder.Dot(orderItemPrice, treeBuilder.Ident("Price")),
						treeBuilder.MethodCall("COALESCE",
							treeBuilder.Dot(orderItemActualCount, treeBuilder.Ident("ActualCount")),
							treeBuilder.Dot(orderItemCount, treeBuilder.Ident("Count")))),
					treeBuilder.Dot(orderItemDiscountMoney, treeBuilder.Ident("DiscountMoney"))),
				treeBuilder.Constant(2));
		}
	}
}
