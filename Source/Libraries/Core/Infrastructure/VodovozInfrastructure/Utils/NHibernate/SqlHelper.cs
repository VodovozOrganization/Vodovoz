using System;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;

namespace VodovozInfrastructure.Utils.NHibernate
{
    public static class SqlHelper
    {
        public static IQueryOver<E, F> WhereStringIsNotNullOrEmpty<E, F>(this IQueryOver<E, F> query, Expression<Func<E, object>> propExpression)
        {
            var prop = Projections.Property(propExpression);
            var criteria = Restrictions.Or(Restrictions.IsNull(prop), Restrictions.Eq(Projections.SqlFunction("trim", NHibernateUtil.String, prop), ""));
            return query.Where(Restrictions.Not(criteria));
        }
    }
}