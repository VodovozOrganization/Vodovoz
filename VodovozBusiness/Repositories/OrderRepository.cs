using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository
{
	public static class OrderRepository
	{
		public static ListStore GetListStoreSumDifferenceReasons (IUnitOfWork uow)
		{
			Vodovoz.Domain.Orders.Order order = null;

			var reasons = uow.Session.QueryOver<VodovozOrder> (() => order)
				.Select (Projections.Distinct (Projections.Property (() => order.SumDifferenceReason)))
				.List<string> ();

			var store = new ListStore (typeof(string));
			foreach (string s in reasons) {
				store.AppendValues (s);
			}
			return store;
		}
			
		public static QueryOver<VodovozOrder> GetOrdersForRLEditingQuery (DateTime date, bool showShipped)
		{
			var query = QueryOver.Of<VodovozOrder>();
			if (!showShipped)
				query.Where(order => order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList);
			else
				query.Where(order => order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.NewOrder && order.OrderStatus != OrderStatus.WaitForPayment);
			return query.Where(order => order.DeliveryDate == date.Date && !order.SelfDelivery);
		}

		public static IList<VodovozOrder> GetAcceptedOrdersForRegion (IUnitOfWork uow, DateTime date, LogisticsArea area)
		{
			DeliveryPoint point = null;
			return uow.Session.QueryOver<VodovozOrder> ()
				.JoinAlias (o => o.DeliveryPoint, () => point)
				.Where (o => o.DeliveryDate == date.Date && point.LogisticsArea.Id == area.Id 
					&& !o.SelfDelivery && o.OrderStatus == Vodovoz.Domain.Orders.OrderStatus.Accepted)
				.List<Vodovoz.Domain.Orders.Order> ();
		}

		public static VodovozOrder GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.OrderBy(() => orderAlias.Id).Desc
				.Take(1).List();
			return queryResult.FirstOrDefault();
		}

		public static IList<VodovozOrder> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.Where(() => orderAlias.DeliveryDate >= DateTime.Today)
				.Where(() => orderAlias.OrderStatus != OrderStatus.Closed 
					&& orderAlias.OrderStatus != OrderStatus.Canceled
					&& orderAlias.OrderStatus != OrderStatus.DeliveryCanceled
					&& orderAlias.OrderStatus != OrderStatus.NotDelivered)
				.List();
		}

		public static IList<VodovozOrder> GetCompleteOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate, PaymentType payment)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.Where(o => o.PaymentType == payment)
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate).List();
		}

		public static IList<VodovozOrder> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate).List();
		}

		public static IList<VodovozOrder> GetOrdersByCode1c (IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<VodovozOrder> ()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<VodovozOrder> ();
		}

		/// <summary>
		/// Список последних заказов для точки.
		/// </summary>
		/// <returns>Список последних заказов для точки.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		public static IList<VodovozOrder> GetLatestOrdersForCounterparty(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int count)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias)
			    .Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.OrderBy(() => orderAlias.Id).Desc
			    .Take(count).List();
			return queryResult;
		}

		public static OrderStatus[] GetOnClosingOrderStatuses()
		{
			return new OrderStatus[] {
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
		}

		public static OrderStatus[] GetNotDeliveredOrderStatuses()
		{
			return new OrderStatus[]{
				OrderStatus.Canceled,
				OrderStatus.Closed,
				OrderStatus.DeliveryCanceled,
				OrderStatus.NotDelivered,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock
			};
		}
	}
}

