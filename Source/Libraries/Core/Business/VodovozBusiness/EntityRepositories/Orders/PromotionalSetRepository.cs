using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Goods.PromotionalSets;
using Vodovoz.Domain.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Orders
{
	public class PromotionalSetRepository : IPromotionalSetRepository
	{
		internal static Func<IUnitOfWork, VodovozOrder, bool, Dictionary<int, int[]>> GetPromotionalSetsAndCorrespondingOrdersForDeliveryPointTestGap;
		/// <summary>
		/// Возврат словаря, у которого ключ это <see cref="PromotionalSet.Id"/>,
		/// а значение - массив с <see cref="VodovozOrder.Id"/>, для всех точек доставок
		/// похожих по полям <see cref="DeliveryPoint.City"/>,
		/// <see cref="DeliveryPoint.Street"/>, <see cref="DeliveryPoint.Building"/>,
		/// <see cref="DeliveryPoint.Room"/>
		/// </summary>
		/// <returns>Словарь</returns>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="currOrder">Заказ, из которого берётся точка доставки</param>
		/// <param name="ignoreCurrentOrder">Если <c>true</c>, то в выборке будет
		/// игнорироваться заказ передаваемы в качестве параметра <paramref name="currOrder"/></param>
		public Dictionary<int, int[]> GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(IUnitOfWork uow, VodovozOrder currOrder, bool ignoreCurrentOrder = false)
		{
			if(GetPromotionalSetsAndCorrespondingOrdersForDeliveryPointTestGap != null)
			{
				return GetPromotionalSetsAndCorrespondingOrdersForDeliveryPointTestGap(uow, currOrder, ignoreCurrentOrder);
			}

			VodovozOrder ordersAlias = null;
			PromotionalSet promotionalSetAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var dp = currOrder.DeliveryPoint;
			var oId = !ignoreCurrentOrder ? -1 : currOrder.Id;

			var subQuerySimilarDP = QueryOver.Of(() => deliveryPointAlias)
											   .Where(p => p.City == dp.City)
											   .Where(p => p.Street == dp.Street)
											   .Where(p => p.Building == dp.Building)
											   .Where(p => p.Room == dp.Room)
											   .Select(Projections.Property(() => deliveryPointAlias.Id))
											   ;

			var result = uow.Session.QueryOver(() => promotionalSetAlias)
									.JoinAlias(() => promotionalSetAlias.Orders, () => ordersAlias)
									.JoinAlias(() => ordersAlias.DeliveryPoint, () => deliveryPointAlias)
									.Where(() => ordersAlias.Id != oId)
									.Where(() => ordersAlias.OrderStatus.IsIn(GetAcceptableStatuses()))
									.WithSubquery.WhereProperty(() => deliveryPointAlias.Id).In(subQuerySimilarDP)
									.SelectList(list => list.Select(() => promotionalSetAlias.Id)
															.Select(() => ordersAlias.Id))
									.List<object[]>()
									.GroupBy(x => (int)x[0])
									.ToDictionary(g => g.Key, g => g.Select(x => (int)x[1]).ToArray());
			return result;
		}

		public bool AddressHasAlreadyBeenUsedForPromo(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			string building = GetBuildingNumber(deliveryPoint.Building);

			VodovozOrder ordersAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			PromotionalSet promotionalSetAlias = null;

			var result = uow.Session.QueryOver(() => ordersAlias)
									.JoinAlias(() => ordersAlias.DeliveryPoint, () => deliveryPointAlias)
									.JoinAlias(() => ordersAlias.PromotionalSets, () => promotionalSetAlias)
									.Where(() => deliveryPointAlias.City.IsLike(deliveryPoint.City, MatchMode.Anywhere)
											   && deliveryPointAlias.Street.IsLike(deliveryPoint.Street, MatchMode.Anywhere)
											   && deliveryPointAlias.Building.IsLike(building, MatchMode.Anywhere)
											   && deliveryPointAlias.Room == deliveryPoint.Room
											   && !promotionalSetAlias.CanBeReorderedWithoutRestriction
											   && ordersAlias.OrderStatus.IsIn(GetAcceptableStatuses())
											   && deliveryPointAlias.Id != deliveryPoint.Id)
									.List<VodovozOrder>();
			return result.Count() != 0;
		}
		
		public IEnumerable<PromoSetDuplicateInfoNode> GetPromoSetDuplicateInfoByAddress(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			DeliveryPoint deliveryPointAlias = null;
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			PromoSetDuplicateInfoNode resultAlias = null;

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
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)));

			var deliveryPointsResult = query.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
				).TransformUsing(Transformers.AliasToBean<PromoSetDuplicateInfoNode>())
				.List<PromoSetDuplicateInfoNode>();
			return deliveryPointsResult;
		}
		
		public IEnumerable<PromoSetDuplicateInfoNode> GetPromoSetDuplicateInfoByCounterpartyPhones(
			IUnitOfWork uow, IEnumerable<Phone> phones)
		{
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			PromoSetDuplicateInfoNode resultAlias = null;

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

			var counterpartyConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				counterpartyPhoneProjection,
				Projections.Constant(", ")
			);

			var phoneResult = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.WhereRestrictionOn(() => counterpartyPhoneAlias.DigitsNumber).IsInG(phonesArray)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)))
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
					.Select(counterpartyConcatPhoneProjection).WithAlias(() => resultAlias.Phone)
				).TransformUsing(Transformers.AliasToBean<PromoSetDuplicateInfoNode>())
			.List<PromoSetDuplicateInfoNode>();
			return phoneResult;
		}
		
		public IEnumerable<PromoSetDuplicateInfoNode> GetPromoSetDuplicateInfoByDeliveryPointPhones(
			IUnitOfWork uow, IEnumerable<Phone> phones)
		{
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Phone deliveryPointPhoneAlias = null;
			PromoSetDuplicateInfoNode resultAlias = null;

			var phonesArray = phones.Select(x => x.DigitsNumber).ToArray();
			var nullProjection = Projections.SqlFunction( 
				new SQLFunctionTemplate(NHibernateUtil.String, "NULLIF(1,1)"),
				NHibernateUtil.String
			);

			var deliveryPointPhoneProjection = Projections.Conditional(
				Restrictions.In(Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber), phonesArray),
				Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber),
				nullProjection
			);

			var deliveryPointConcatPhoneProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(DISTINCT ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				deliveryPointPhoneProjection,
				Projections.Constant(", ")
			);

			var phoneResult = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias)
				.WhereRestrictionOn(() => deliveryPointPhoneAlias.DigitsNumber).IsInG(phonesArray)
				.Where(Restrictions.IsNotNull(Projections.Property(() => orderItemAlias.PromoSet)))
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
					.Select(deliveryPointConcatPhoneProjection).WithAlias(() => resultAlias.Phone)
				).TransformUsing(Transformers.AliasToBean<PromoSetDuplicateInfoNode>())
			.List<PromoSetDuplicateInfoNode>();
			return phoneResult;
		}

		private string GetBuildingNumber(string building)
		{
			string buildingNumber = string.Empty;

			foreach(var ch in building)
			{
				if(char.IsDigit(ch))
				{
					buildingNumber += ch;
				}
				else
				{
					if(buildingNumber != string.Empty)
					{
						break;
					}
				}
			}

			return buildingNumber;
		}

		private static OrderStatus[] GetAcceptableStatuses()
		{
			return new[]
			{
				OrderStatus.Accepted,
				OrderStatus.InTravelList,
				OrderStatus.WaitForPayment,
				OrderStatus.OnLoading,
				OrderStatus.OnTheWay,
				OrderStatus.Shipped,
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
		}
	}
}
