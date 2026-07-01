using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Cash
{
	[Serializable]
	public class MissingOrdersWithCashPaymentTypeException : InvalidOperationException
	{

		public MissingOrdersWithCashPaymentTypeException(RouteList routeList)
			: base($"В МЛ {routeList.Id} отсутствуют заказы с типом оплаты 'Наличная'")
		{
			RouteList = routeList;
		}

		public RouteList RouteList { get; }
	}
}
