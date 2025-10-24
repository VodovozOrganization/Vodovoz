using NHibernate;
using NHibernate.Dialect.Function;
using NHibernate.Spatial.Dialect;

namespace Vodovoz.Core.Data.NHibernate.NhibernateExtensions
{
	/// <summary>
	/// Пример использования: Projections.SqlFunction("IS_NULL_OR_WHITESPACE", NHibernateUtil.Boolean, Projection.Property(() => alias))
	/// </summary>
	public class MySQL57SpatialExtendedDialect : MySQL57SpatialDialect
	{
		public MySQL57SpatialExtendedDialect()
		{
			RegisterFunction("DATE", new StandardSQLFunction("DATE", NHibernateUtil.Date));
			/*RegisterFunction(
				"DATE_ADD",
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "DATE_ADD(?1 INTERVAL ?2 ?3)"));*/
			RegisterFunction("TIME", new StandardSQLFunction("TIME", NHibernateUtil.Time));

			RegisterFunction("ROUND", new SQLFunctionTemplate
				(NHibernateUtil.Decimal, "ROUND(?1, ?2)"));

			RegisterFunction("GROUP_CONCAT", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 SEPARATOR ?2)"));
			RegisterFunction("GROUP_CONCAT_DISTINCT", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"));
			RegisterFunction("GROUP_CONCAT_ORDER_BY_ASC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 ORDER BY ?2 ASC SEPARATOR ?3)"));
			RegisterFunction("GROUP_CONCAT_ORDER_BY_DESC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(?1 ORDER BY ?2 DESC SEPARATOR ?3)"));
			RegisterFunction("GROUP_CONCAT_DISTINCT_ORDER_BY_ASC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 ORDER BY ?2 ASC SEPARATOR ?3)"));
			RegisterFunction("GROUP_CONCAT_DISTINCT_ORDER_BY_DESC", new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 ORDER BY ?2 DESC SEPARATOR ?3)"));
			RegisterFunction("CONCAT_WS", new StandardSQLFunction("CONCAT_WS", NHibernateUtil.String));
			RegisterFunction("IS_NULL_OR_WHITESPACE", new SQLFunctionTemplate(NHibernateUtil.Boolean, "IS_NULL_OR_WHITESPACE(?1)"));
		}
	}
}
