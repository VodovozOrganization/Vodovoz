using System;
using System.Linq.Expressions;
using System.Reflection;
using SolrNet;

namespace SolrSearch
{
	public static class Utils
	{
		public static string GetPropertyName<T>(Expression<Func<T, object>> propertySelector)
			where T : class
		{
			if(propertySelector == null) {
				throw new ArgumentNullException(nameof(propertySelector));
			}

			MemberExpression memberExpr = propertySelector.Body as MemberExpression;
			if(memberExpr == null) {
				UnaryExpression unaryExpr = propertySelector.Body as UnaryExpression;
				if(unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}

			if(memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
				return memberExpr.Member.Name;

			throw new InvalidOperationException("Должно быть выбрано свойство");
		}

		public static void Register<TSolrEntity>(string solrCoreAddress)
		{
			Startup.Init<TSolrEntity>(solrCoreAddress);
		}
	}
}
