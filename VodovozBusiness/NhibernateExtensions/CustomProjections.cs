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

        #region GROUP_CONCAT

        public static IProjection GroupConcat(
            Expression<Func<object>> expression,
            bool useDistinct = false,
            Expression<Func<object>> orderByExpression = null,
            OrderByDirection orderByDirection = OrderByDirection.Asc,
            string separator = ",")
        {
            if(expression == null) {
                throw new ArgumentNullException(nameof(expression));
            }

            return GroupConcat(
                Projections.Property(expression),
                useDistinct,
                orderByExpression == null ? null : Projections.Property(orderByExpression),
                orderByDirection,
                separator
            );
        }

        public static IProjection GroupConcat(
            IProjection projection,
            bool useDistinct = false,
            IProjection orderByProjection = null,
            OrderByDirection orderByDirection = OrderByDirection.Asc,
            string separator = ",")
        {
            if(projection == null) {
                throw new ArgumentNullException(nameof(projection));
            }
            var separatorProjection = Projections.Constant(separator);

            //DISTINCT + ORDER BY
            if(useDistinct && orderByProjection != null) {
                switch(orderByDirection) {
                    case OrderByDirection.Asc:
                        return Projections.SqlFunction(
                            "GROUP_CONCAT_DISTINCT_ORDER_BY_ASC",
                            NHibernateUtil.String, projection, orderByProjection, separatorProjection);
                    case OrderByDirection.Desc:
                        return Projections.SqlFunction("GROUP_CONCAT_DISTINCT_ORDER_BY_DESC",
                            NHibernateUtil.String, projection, orderByProjection, separatorProjection);
                    default:
                        throw new NotSupportedException($"{nameof(OrderByDirection)}{orderByDirection} is not supported");
                }
            }
            //DISTINCT
            if(useDistinct) {
                return Projections.SqlFunction("GROUP_CONCAT_DISTINCT", NHibernateUtil.String, projection, separatorProjection);
            }
            //ORDER BY
            if(orderByProjection != null) {
                switch(orderByDirection) {
                    case OrderByDirection.Asc:
                        return Projections.SqlFunction("GROUP_CONCAT_ORDER_BY_ASC",
                            NHibernateUtil.String, projection, orderByProjection, separatorProjection);
                    case OrderByDirection.Desc:
                        return Projections.SqlFunction("GROUP_CONCAT_ORDER_BY_DESC",
                            NHibernateUtil.String, projection, orderByProjection, separatorProjection);
                    default:
                        throw new NotSupportedException($"{nameof(OrderByDirection)}{orderByDirection} is not supported");
                }
            }

            return Projections.SqlFunction("GROUP_CONCAT", NHibernateUtil.String, projection, separatorProjection);
        }

        #endregion

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

    public enum OrderByDirection
    {
        Asc,
        Desc
    }
}
