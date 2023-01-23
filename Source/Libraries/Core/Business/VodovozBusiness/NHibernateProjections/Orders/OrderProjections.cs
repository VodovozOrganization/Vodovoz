using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using Vodovoz.Domain.Orders;

namespace Vodovoz.NHibernateProjections.Orders
{
	public static class OrderProjections
	{
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется OrderItem orderItemAlias
		/// </summary>
		/// <returns></returns>
		public static IProjection GetOrderSumProjection()
		{
			OrderItem orderItemAlias = null;

			return Projections.Sum(
			Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3) - ?4, 2)"),
				NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.Price),
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count),
					Projections.Property(() => orderItemAlias.DiscountMoney)));
		}

		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется OrderItem orderItemAlias
		/// </summary>
		/// <returns></returns>
		public static IProjection GetOrderItemSumProjection()
		{
			OrderItem orderItemAlias = null;

			return Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3) - ?4, 2)"),
				NHibernateUtil.Decimal,
					Projections.Property(() => orderItemAlias.Price),
					Projections.Property(() => orderItemAlias.ActualCount),
					Projections.Property(() => orderItemAlias.Count),
					Projections.Property(() => orderItemAlias.DiscountMoney));
		}
	}
}
