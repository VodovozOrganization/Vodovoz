using System;
using Gtk;
using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Transform;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Vodovoz.Repository
{
	public static class OrderRepository
	{
		public static ListStore GetListStoreSumDifferenceReasons (IUnitOfWork uow)
		{
			Vodovoz.Domain.Orders.Order order = null;

			var reasons = uow.Session.QueryOver<Vodovoz.Domain.Orders.Order> (() => order)
				.Select (Projections.Distinct (Projections.Property (() => order.SumDifferenceReason)))
				.List<string> ();

			var store = new ListStore (typeof(string));
			foreach (string s in reasons) {
				store.AppendValues (s);
			}
			return store;
		}

		public static QueryOver<Vodovoz.Domain.Orders.Order> GetAcceptedOrdersForDateQueryOver (DateTime date)
		{
			return QueryOver.Of<Vodovoz.Domain.Orders.Order> ()
				.Where (order => order.OrderStatus == Vodovoz.Domain.Orders.OrderStatus.Accepted
					&& order.DeliveryDate.Date == date.Date);
		}

		public static IList<Vodovoz.Domain.Orders.Order> GetAcceptedOrdersForRegion (IUnitOfWork uow, DateTime date, LogisticsArea area)
		{
			return uow.Session.QueryOver<Vodovoz.Domain.Orders.Order>()
				.Where (o => o.DeliveryDate.Date == date.Date && o.DeliveryPoint.LogisticsArea.Id == area.Id)
				.List<Vodovoz.Domain.Orders.Order>();
		}
	}
}

