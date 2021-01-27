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
        
        public static IProjection Abs(params Expression<Func<object>>[] properties)
        {
            return Abs(properties.Select(Projections.Property).ToArray());
        }

        public static IProjection Abs(params IProjection[] projections)
        {
            var firstProjection = projections.FirstOrDefault();
            if(firstProjection == null) {
                throw new ArgumentException(@"В SQL функцию ABS не было передано ни одного параметра", nameof(projections));
            }
            
            var returnType = firstProjection.GetTypes(null, null).FirstOrDefault();
            if(returnType == null) {
                throw new InvalidOperationException("Не удалось получить возвращаемый тип проекции");
            }
            
            return Projections.SqlFunction("ABS", returnType, projections);
        }
    }
}
