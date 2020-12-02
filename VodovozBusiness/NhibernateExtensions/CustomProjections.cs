using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;

namespace Vodovoz.NhibernateExtensions
{
    public static class CustomProjections
    {
        public static IProjection Date(params Expression<Func<object>>[] properties)
        {
            return Date(properties.Select(Projections.Property).ToArray());
        }

        public static IProjection Date(params IProjection[] projections)
        {
            return Projections.SqlFunction("DATE", NHibernateUtil.Date, projections);
        }
    }
}