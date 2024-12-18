﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
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
			DeliveryPoint deliveryPoint)
		{
			DeliveryPoint deliveryPointAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias, () => !counterpartyPhoneAlias.IsArchive)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias, () => !deliveryPointPhoneAlias.IsArchive)
				.Where(
					Restrictions.And(
						Restrictions.Where(() =>
							deliveryPointAlias.City == deliveryPoint.City
							&& deliveryPointAlias.Street == deliveryPoint.Street
							&& deliveryPointAlias.Building == deliveryPoint.Building
							&& deliveryPointAlias.Room == deliveryPoint.Room
						),
						Restrictions.Ge(Projections.Property(() => deliveryPointAlias.Room), 1)
					)
				)
				.And(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)))
				.And(() => orderAlias.Id != orderId);

			var deliveryPointsResult = query.SelectList(list => list
				.SelectGroup(() => orderAlias.Id)
				.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
				.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
				.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
			).TransformUsing(Transformers.AliasToBean<FreeLoaderInfoNode>())
			.List<FreeLoaderInfoNode>();
			return deliveryPointsResult;
		}

		public IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByCounterpartyPhones(
			IUnitOfWork uow,
			int orderId,
			IEnumerable<Phone> phones)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var phonesArray = phones.Select(x => x.DigitsNumber).ToArray();
			var nullProjection = Projections.SqlFunction( 
				new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
				NHibernateUtil.String
			);

			var counterpartyPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => counterpartyPhoneAlias.DigitsNumber), phonesArray),
				Projections.Property(() => counterpartyPhoneAlias.DigitsNumber),
				nullProjection
			);

			var deliveryPointPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phonesArray),
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
				.WhereRestrictionOn(() => counterpartyPhoneAlias.DigitsNumber).IsInG(phonesArray)
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
			IEnumerable<Phone> phones)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Vodovoz.Domain.Orders.OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			FreeLoaderInfoNode resultAlias = null;

			var phonesArray = phones.Select(x => x.DigitsNumber).ToArray();
			
			var nullProjection = Projections.SqlFunction( 
				new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
				NHibernateUtil.String
			);

			var counterpartyPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => counterpartyPhoneAlias.DigitsNumber), phonesArray),
				Projections.Property(() => counterpartyPhoneAlias.DigitsNumber),
				nullProjection
			);

			var deliveryPointPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phonesArray),
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
				.WhereRestrictionOn(() => deliveryPointPhoneAlias.DigitsNumber).IsInG(phonesArray)
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
				.Where(
					Restrictions.And(
						Restrictions.Where(() =>
							deliveryPointAlias.BuildingFiasGuid == buildingFiasGuid
							&& deliveryPointAlias.Room == room
						),
						Restrictions.Ge(Projections.Property(() => deliveryPointAlias.Room), 1)
					)
				)
				.And(() => promoSetAlias.PromotionalSetForNewClients == promoSetForNewClients)
				.And(() => orderAlias.Id != orderId);
					
			var deliveryPointsResult = query.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
				).TransformUsing(Transformers.AliasToBean<FreeLoaderInfoNode>())
				.List<FreeLoaderInfoNode>();
			return deliveryPointsResult;
		}
	}
}
