﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class UndeliveredOrdersRepository : IUndeliveredOrdersRepository
	{
		public Dictionary<GuiltyTypes, int> GetDictionaryWithUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias);

			if(start != null && end != null)
			{
				query.Left.JoinAlias(u => u.OldOrder, () => orderAlias)
					.Where(() => orderAlias.DeliveryDate >= start)
					.Where(u => orderAlias.DeliveryDate <= end);
			}

			var result = query
				.Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
				.SelectList(list => list
							.SelectGroup(() => guiltyInUndeliveryAlias.GuiltySide)
							.SelectCount(() => undeliveredOrderAlias.Id)
						   )
				.List<object[]>();

			return result.ToDictionary(x => (GuiltyTypes)x[0], x => (int)x[1]);
		}

		public IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrderCountNode resultAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
						   .Left.JoinAlias(u => u.OldOrder, () => orderAlias);

			if(start != null && end != null)
				query.Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query
				.Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
				.SelectList(list => list
							.SelectGroup(() => guiltyInUndeliveryAlias.GuiltySide).WithAlias(() => resultAlias.Type)
							.SelectCount(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Count)
						   )
				.TransformUsing(Transformers.AliasToBean<UndeliveredOrderCountNode>())
				.List<UndeliveredOrderCountNode>();

			return result;
		}

		public IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountOnDptForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrderCountNode resultAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Subdivision subdivisionAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
						   .Where(() => guiltyInUndeliveryAlias.GuiltySide == GuiltyTypes.Department)
						   .Left.JoinAlias(u => u.OldOrder, () => orderAlias)
						   .Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias)
						   .Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias);

			if(start != null && end != null)
				query.Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query.SelectList(list => list
										  .SelectGroup(() => subdivisionAlias.Id).WithAlias(() => resultAlias.SubdivisionId)
										  .Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
										  .SelectCount(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Count)
										 )
							  .TransformUsing(Transformers.AliasToBean<UndeliveredOrderCountNode>())
							  .List<UndeliveredOrderCountNode>();

			return result;
		}

		public IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, int orderId)
		{
			Order order = uow.GetById<Order>(orderId);
			return GetListOfUndeliveriesForOrder(uow, order);
		}

		public IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, Order order)
		{
			var query = uow.Session.QueryOver<UndeliveredOrder>()
						   .Where(u => u.OldOrder == order)
						   .List<UndeliveredOrder>();
			
			return query;
		}

		public IList<int> GetListOfUndeliveryIdsForDriver(IUnitOfWork uow, Employee driver)
		{
			Order orderAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;

			var query = uow.Session.QueryOver<RouteListItem>(() => routeListItemAlias)
						   .Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
						   .Where(() => routeListAlias.Driver == driver)
						   .Left.JoinQueryOver(() => routeListItemAlias.Order, () => orderAlias);
			
			var q = query.List().Select(i => i.Order.Id);
			
			return q.ToList();
		}

		public IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrder undeliveredOrderAlias = null;
			Subdivision subdivisionAlias = null;
			Order orderAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
							  .Left.JoinAlias(u => u.OldOrder, () => orderAlias)
							  .Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
							  .Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias);

			if(start != null && end != null)
			{
				query.Where(() => orderAlias.DeliveryDate >= start)
					.Where(u => orderAlias.DeliveryDate <= end);
			}

			int i = 0;
			var result = query.SelectList(list => list
				  .SelectGroup(u => u.Id)
				  .Select(
					  Projections.SqlFunction(
						  new SQLFunctionTemplate(
							  NHibernateUtil.String,
							  "GROUP_CONCAT(" +
							  "CASE ?1 " +
							  $"WHEN '{nameof(GuiltyTypes.Department)}' THEN IFNULL(CONCAT('Отд: ', ?2), '{GuiltyTypes.Department.GetEnumTitle()}') " +
							  $"WHEN '{nameof(GuiltyTypes.Client)}' THEN '{GuiltyTypes.Client.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.Driver)}' THEN '{GuiltyTypes.Driver.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.ServiceMan)}' THEN '{GuiltyTypes.ServiceMan.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.ForceMajor)}' THEN '{GuiltyTypes.ForceMajor.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.DirectorLO)}' THEN '{GuiltyTypes.DirectorLO.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.DirectorLOCurrentDayDelivery)}' THEN '{GuiltyTypes.DirectorLOCurrentDayDelivery.GetEnumTitle()}' " +
							  $"WHEN '{nameof(GuiltyTypes.None)}' THEN '{GuiltyTypes.None.GetEnumTitle()}' " +
							  "ELSE ?1 " +
							  "END ORDER BY ?1 ASC SEPARATOR '\n')"
							 ),
						  NHibernateUtil.String,
						  Projections.Property(() => guiltyInUndeliveryAlias.GuiltySide),
						  Projections.Property(() => subdivisionAlias.ShortName)
						 )
					 )
				 )
		  .List<object[]>()
		  .GroupBy(x => x[1])
		  .Select(r => new[] { r.Key, r.Count(), i++ })
	      .ToList();

			return result;
		}

		public decimal GetUndelivered19LBottlesQuantity(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			Order orderAlias = null;
			Nomenclature nomenclatureAlias = null;

			var subquery = QueryOver.Of<UndeliveredOrder>()
				.Left.JoinAlias(u => u.OldOrder, () => orderAlias);
			
			if(start != null && end != null)
			{
				subquery.Where(() => orderAlias.DeliveryDate >= start)
					.Where(() => orderAlias.DeliveryDate <= end);
			}

			subquery.Select(u => u.OldOrder.Id);

			var bottles19L = uow.Session.QueryOver<OrderItem>()
				.WithSubquery.WhereProperty(i => i.Order.Id).In(subquery)
				.Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
				.Where(n => n.Category == NomenclatureCategory.water && n.TareVolume == TareVolume.Vol19L)
				.SelectList(list => list.SelectSum(i => i.Count))
				.List<decimal?>()
				.FirstOrDefault();

			return bottles19L ?? 0;
		}

		public Order GetOldOrderFromUndeliveredByNewOrderId(IUnitOfWork uow, int newOrderId)
		{
			Order oldOrderAlias = null;

			return uow.Session.QueryOver<UndeliveredOrder>()
				.JoinAlias(u => u.OldOrder, () => oldOrderAlias)
				.Where(u => u.NewOrder.Id == newOrderId)
				.Select(Projections.Entity(() => oldOrderAlias))
				.SingleOrDefault<Order>();
		}
	}
}
