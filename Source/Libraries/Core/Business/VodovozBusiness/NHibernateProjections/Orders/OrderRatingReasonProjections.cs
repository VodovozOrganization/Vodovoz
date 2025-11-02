using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using Vodovoz.Domain.Orders;

namespace Vodovoz.NHibernateProjections.Orders
{
	public static class OrderRatingReasonProjections
	{
		/// <summary>
		/// Получение оценок для причины оценки заказа
		/// </summary>
		/// <returns></returns>
		public static IProjection GetOrderRatingsForReason()
		{
			return Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_ORDER_RATINGS_FOR_REASON(?1)"),
				NHibernateUtil.String,
				Projections.Property<OrderRatingReason>(r => r.Id));
		}
	}
}
