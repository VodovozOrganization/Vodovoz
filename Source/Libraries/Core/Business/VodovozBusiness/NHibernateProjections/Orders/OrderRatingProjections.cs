using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using Vodovoz.Domain.Orders;

namespace Vodovoz.NHibernateProjections.Orders
{
	public static class OrderRatingProjections
	{
		/// <summary>
		/// Получение всех причин оценки заказа
		/// </summary>
		/// <returns></returns>
		public static IProjection GetOrderRatingReasons()
		{
			return Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_ORDER_RATING_REASONS(?1)"),
				NHibernateUtil.String,
				Projections.Property<OrderRating>(or => or.Id));
		}
	}
}
