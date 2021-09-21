using System;
using System.Runtime.Serialization;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Cash
{
	[Serializable]
	public class MissingOrdersWithCashlessPaymentTypeException : InvalidOperationException
	{

		public MissingOrdersWithCashlessPaymentTypeException(RouteList routeList)
			: base($"В МЛ {routeList.Id} отсутствуют заказы с типом оплыта 'Наличная'")
		{
			RouteList = routeList;
		}

		public RouteList RouteList { get; }
	}
}
