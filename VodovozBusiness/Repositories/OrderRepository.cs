using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
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

		public static QueryOver<VodovozOrder> GetAcceptedOrdersForDateQueryOver (DateTime date)
		{
			return QueryOver.Of<VodovozOrder> ()
				.Where (order => order.OrderStatus == Vodovoz.Domain.Orders.OrderStatus.Accepted
			&& order.DeliveryDate.Date == date.Date
			&& !order.SelfDelivery);
		}

		public static IList<VodovozOrder> GetAcceptedOrdersForRegion (IUnitOfWork uow, DateTime date, LogisticsArea area)
		{
			DeliveryPoint point = null;
			return uow.Session.QueryOver<VodovozOrder> ()
				.JoinAlias (o => o.DeliveryPoint, () => point)
				.Where (o => o.DeliveryDate.Date == date.Date && point.LogisticsArea.Id == area.Id 
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

		public static IList<VodovozOrder> GetCompleteOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate).List();
		}
	}
}

