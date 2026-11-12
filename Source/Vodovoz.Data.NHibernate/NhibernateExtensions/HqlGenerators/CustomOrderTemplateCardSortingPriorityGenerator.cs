using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Sorting;

namespace Vodovoz.Data.NHibernate.NhibernateExtensions.HqlGenerators
{
	/// <summary>
	/// Транслирует <see cref="OrderTemplateCardSortingPriority.GetByStatus(OnlineOrderTemplateStatus)"/>
	/// в SQL CASE WHEN.
	/// </summary>
	public class CustomOrderTemplateCardSortingPriorityGenerator : BaseHqlGeneratorForMethod
	{
		public CustomOrderTemplateCardSortingPriorityGenerator()
		{
			SupportedMethods = new[] {
				typeof(OrderTemplateCardSortingPriority)
					.GetMethod(nameof(OrderTemplateCardSortingPriority.GetByStatus), new[] { typeof(OnlineOrderTemplateStatus) })
			};
		}

		public override HqlTreeNode BuildHql(
			MethodInfo method,
			Expression targetObject,
			ReadOnlyCollection<Expression> arguments,
			HqlTreeBuilder treeBuilder,
			IHqlExpressionVisitor visitor)
		{
			var statusProperty = visitor.Visit(arguments[0]).AsExpression();

			var whens = OrderTemplateCardSortingPriority.SortingPriority
				.Select(kv => treeBuilder.When(
					treeBuilder.Equality(statusProperty, treeBuilder.Constant(kv.Key.ToString())),
					treeBuilder.Constant(kv.Value)))
				.ToArray();

			return treeBuilder.Case(whens, treeBuilder.Constant(OrderTemplateCardSortingPriority.DefaultPriority));
		}
	}
}
