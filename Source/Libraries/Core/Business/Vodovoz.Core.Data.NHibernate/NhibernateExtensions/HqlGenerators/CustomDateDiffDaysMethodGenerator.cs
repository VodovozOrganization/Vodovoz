using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;

namespace Vodovoz.Core.Data.NHibernate.NhibernateExtensions.HqlGenerators
{
	public class CustomDateDiffDaysMethodGenerator : BaseHqlGeneratorForMethod
	{
		public CustomDateDiffDaysMethodGenerator()
		{
			SupportedMethods = new[]
			{
				ReflectionHelper.GetMethodDefinition(
					() => SqlDateFunctions.DateDiffDays(DateTime.MinValue, DateTime.MinValue))
			};
		}

		public override HqlTreeNode BuildHql(
			MethodInfo method,
			Expression targetObject,
			ReadOnlyCollection<Expression> arguments,
			HqlTreeBuilder treeBuilder,
			IHqlExpressionVisitor visitor)
		{
			var start = visitor.Visit(arguments[0]).AsExpression();
			var end = visitor.Visit(arguments[1]).AsExpression();

			return treeBuilder.MethodCall("DATEDIFF", start, end);
		}
	}
	
	public static class SqlDateFunctions
	{
		[Obsolete("Использовать только внутри LINQ-запросов: транслируется в SQL")]
		public static int DateDiffDays(DateTime startDate, DateTime endDate) =>
			throw new NotSupportedException("Метод не предназначен для прямого вызова");
	}
}
