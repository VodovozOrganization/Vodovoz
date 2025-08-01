using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.Nodes;
using VodovozBusiness.EntityRepositories.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	public class FreeLoaderRepository : IFreeLoaderRepository
	{
		public IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersByAddress(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint,
			bool? promoSetForNewClients = null)
		{
			DeliveryPoint deliveryPointAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			PromotionalSet promoSetAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias, () => !counterpartyPhoneAlias.IsArchive)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias, () => !deliveryPointPhoneAlias.IsArchive)
				.JoinAlias(() => orderAlias.PromotionalSets, () => promoSetAlias)
				.Where(() => deliveryPointAlias.City == deliveryPoint.City)
				.And(() => deliveryPointAlias.Street == deliveryPoint.Street)
				.And(() => deliveryPointAlias.Building == deliveryPoint.Building)
				.And(() => orderAlias.Id != orderId)
				.AndRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(OrderRepository.GetUndeliveryAndNewStatuses());

			if(int.TryParse(deliveryPoint.Room, out var roomNumber))
			{
				query.And(() => deliveryPointAlias.Room == deliveryPoint.Room);
			}
			else
			{
				query.And(CustomRestrictions.Rlike(Projections.Property(() => deliveryPointAlias.Room), "[^\\s\\d]"));
			}

			if(promoSetForNewClients.HasValue)
			{
				query.And(() => promoSetAlias.PromotionalSetForNewClients == promoSetForNewClients.Value);
			}

			var deliveryPointsResult = query.SelectList(list => list
				.SelectGroup(() => orderAlias.Id)
				.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
				.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
			).TransformUsing(Transformers.AliasToBean<FreeLoaderInfoNode>())
			.Take(1)
			.List<FreeLoaderInfoNode>();
			
			return deliveryPointsResult;
		}

		public IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByCounterpartyPhones(
			IUnitOfWork uow,
			int orderId,
			IEnumerable<string> phoneNumbers)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var nullProjection = Projections.SqlFunction( 
				new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
				NHibernateUtil.String
			);

			var counterpartyPhoneProjection = Projections.Conditional(
				Restrictions.InG(Projections.Property(() => counterpartyPhoneAlias.DigitsNumber), phoneNumbers),
				Projections.Property(() => counterpartyPhoneAlias.DigitsNumber),
				nullProjection
			);

			var deliveryPointPhoneProjection = Projections.Conditional(
				Restrictions.InG(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phoneNumbers),
				Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber),
				nullProjection
			);

			var counterpartyConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				counterpartyPhoneProjection,
				Projections.Constant(", ")
			);

			var deliveryPointConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				deliveryPointPhoneProjection,
				Projections.Constant(", ")
			);

			var concatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(', ', ?1, ?2)"),
				NHibernateUtil.String,
				counterpartyConcatPhoneProjection,
				deliveryPointConcatPhoneProjection
			);

			var phoneResult = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.WhereRestrictionOn(() => counterpartyPhoneAlias.DigitsNumber).IsInG(phoneNumbers)
				.And(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)))
				.And(() => orderAlias.Id != orderId)
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
					.Select(concatPhoneProjection).WithAlias(() => resultAlias.Phone)
				).TransformUsing(Transformers.AliasToBean<FreeLoaderInfoNode>())
			.List<FreeLoaderInfoNode>();
			return phoneResult;
		}

		public IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByDeliveryPointPhones(
			IUnitOfWork uow,
			IEnumerable<int> excludeOrderIds,
			IEnumerable<string> phoneNumbers)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Vodovoz.Domain.Orders.OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var nullProjection = Projections.SqlFunction( 
				new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
				NHibernateUtil.String
			);

			var counterpartyPhoneProjection = Projections.Conditional(
				Restrictions.InG(Projections.Property(() => counterpartyPhoneAlias.DigitsNumber), phoneNumbers),
				Projections.Property(() => counterpartyPhoneAlias.DigitsNumber),
				nullProjection
			);

			var deliveryPointPhoneProjection = Projections.Conditional(
				Restrictions.InG(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phoneNumbers),
				Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber),
				nullProjection
			);

			var counterpartyConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				counterpartyPhoneProjection,
				Projections.Constant(", ")
			);

			var deliveryPointConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				deliveryPointPhoneProjection,
				Projections.Constant(", ")
			);

			var concatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(', ', ?1, ?2)"),
				NHibernateUtil.String,
				counterpartyConcatPhoneProjection,
				deliveryPointConcatPhoneProjection
			);

			var phoneResult = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.WhereRestrictionOn(() => deliveryPointPhoneAlias.DigitsNumber).IsInG(phoneNumbers)
				.AndRestrictionOn(() => orderAlias.Id).Not.IsInG(excludeOrderIds)
				.And(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)))
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
					.Select(concatPhoneProjection).WithAlias(() => resultAlias.Phone)
				).TransformUsing(Transformers.AliasToBean<FreeLoaderInfoNode>())
			.List<FreeLoaderInfoNode>();
			return phoneResult;
		}

		public IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByBuildingFiasGuid(
			IUnitOfWork uow,
			Guid buildingFiasGuid,
			string room,
			int orderId,
			bool promoSetForNewClients = true)
		{
			DeliveryPoint deliveryPointAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			PromotionalSet promoSetAlias = null;
			Counterparty counterpartyAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.PromotionalSets, () => promoSetAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Where(() => deliveryPointAlias.BuildingFiasGuid == buildingFiasGuid)
				.And(() => promoSetAlias.PromotionalSetForNewClients == promoSetForNewClients)
				.And(() => orderAlias.Id != orderId)
				.AndRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(OrderRepository.GetUndeliveryAndNewStatuses());

			if(int.TryParse(room, out var roomNumber))
			{
				query.And(() => deliveryPointAlias.Room == room);
			}
			else
			{
				query.And(CustomRestrictions.Rlike(Projections.Property(() => deliveryPointAlias.Room), "[^\\s\\d]"));
			}

			var deliveryPointsResult = query.SelectList(list => list
					.Select(
						Projections.Distinct(
							Projections.Property(() => orderAlias.Id))).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
				).TransformUsing(Transformers.AliasToBean<FreeLoaderInfoNode>())
				.Take(1)
				.List<FreeLoaderInfoNode>();
			return deliveryPointsResult;
		}

		public bool HasOnlineOrderWithPromoSetForNewClients(IUnitOfWork uow, int deliveryPointId)
		{
			var query = from onlineOrderItem in uow.Session.Query<OnlineOrderItem>()
				join onlineOrder in uow.Session.Query<OnlineOrder>()
					on onlineOrderItem.OnlineOrder.Id equals onlineOrder.Id
				join promoSet in uow.Session.Query<PromotionalSet>()
					on onlineOrderItem.PromoSet.Id equals promoSet.Id
				where onlineOrder.DeliveryPoint.Id == deliveryPointId
					&& onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled
					&& promoSet.PromotionalSetForNewClients
				select	onlineOrder;

			return query
				.Take(1)
				.Any();
		}
	}
}
