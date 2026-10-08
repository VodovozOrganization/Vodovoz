using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Specifications.TrueMark
{
	/// <summary>
	/// Спецификации отбора кодов продукта адреса маршрутного листа
	/// </summary>
	public class RouteListItemTrueMarkProductCodeSpecification : ExpressionSpecification<RouteListItemTrueMarkProductCode>
	{
		private RouteListItemTrueMarkProductCodeSpecification(Expression<Func<RouteListItemTrueMarkProductCode, bool>> expression)
			: base(expression)
		{
		}

		/// <summary>
		/// Создает спецификацию для отбора кодов продукта адреса маршрутного листа
		/// </summary>
		/// <param name="routeListItemId">Идентификатор адреса маршрутного листа</param>
		public static RouteListItemTrueMarkProductCodeSpecification CreateForRouteListItemId(int routeListItemId)
			=> new RouteListItemTrueMarkProductCodeSpecification(x => x.RouteListItem.Id == routeListItemId);
	}
}
