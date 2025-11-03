using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Vodovoz.Data.NHibernate.NhibernateExtensions
{
	public class CustomAddDaysMethodGenerator : BaseHqlGeneratorForMethod
	{
		public CustomAddDaysMethodGenerator()
		{
			SupportedMethods = new[]
			{
				typeof(DateTime).GetMethod("AddDays", new[] { typeof(double) })
			};
		}

		public override HqlTreeNode BuildHql(
			MethodInfo method,
			Expression targetObject,
			ReadOnlyCollection<Expression> arguments,
			HqlTreeBuilder treeBuilder,
			IHqlExpressionVisitor visitor)
		{
			var dateExpression = visitor.Visit(targetObject).AsExpression();
			var daysExpression = visitor.Visit(arguments[0]).AsExpression();

			return treeBuilder.MethodCall(
				"TIMESTAMPADD",
				treeBuilder.Ident("DAY"),
				daysExpression,
				dateExpression);
		}
	}
}
