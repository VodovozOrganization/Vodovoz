using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.PromotionalSetsOnlineParameters;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Nodes;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Orders
{
	internal sealed class PromotionalSetRepository : IPromotionalSetRepository
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
		public Dictionary<int, int[]> GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(
			IUnitOfWork uow, VodovozOrder currOrder, bool ignoreCurrentOrder = false)
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

		public bool AddressHasAlreadyBeenUsedForPromoForNewClients(IUnitOfWork uow, DeliveryPoint deliveryPoint)
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
					&& promotionalSetAlias.PromotionalSetForNewClients
					&& ordersAlias.OrderStatus.IsIn(GetAcceptableStatuses())
					&& deliveryPointAlias.Id != deliveryPoint.Id)
				.List<VodovozOrder>();

			return result.Count != 0;
		}

		public IEnumerable<PromotionalSetOnlineParametersNode> GetActivePromotionalSetsOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType)
		{
			PromotionalSet promotionalSetAlias = null;
			PromotionalSetOnlineParametersNode resultAlias = null;

			return uow.Session.QueryOver<PromotionalSetOnlineParameters>()
				.Left.JoinAlias(p => p.PromotionalSet, () => promotionalSetAlias)
				.Where(p => p.Type == parameterType)
				.And(p => p.PromotionalSetOnlineAvailability != null)
				.And(() => !promotionalSetAlias.IsArchive)
				.SelectList(list => list
					.Select(p => p.Id).WithAlias(() => resultAlias.Id)
					.Select(() => promotionalSetAlias.Id).WithAlias(() => resultAlias.PromotionalSetId)
					.Select(() => promotionalSetAlias.OnlineName).WithAlias(() => resultAlias.PromotionalSetOnlineName)
					.Select(() => promotionalSetAlias.PromotionalSetForNewClients).WithAlias(() => resultAlias.PromotionalSetForNewClients)
					.Select(() => promotionalSetAlias.BottlesCountForCalculatingDeliveryPrice)
						.WithAlias(() => resultAlias.BottlesCountForCalculatingDeliveryPrice)
					.Select(p => p.PromotionalSetOnlineAvailability).WithAlias(() => resultAlias.AvailableForSale))
				.TransformUsing(Transformers.AliasToBean<PromotionalSetOnlineParametersNode>())
				.List<PromotionalSetOnlineParametersNode>();
		}

		public IEnumerable<PromotionalSetItemBalanceNode> GetPromotionalSetsItemsWithBalanceForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouses)
		{
			PromotionalSet promotionalSetAlias = null;
			PromotionalSetItem promotionalSetItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclature2Alias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			PromotionalSetItemBalanceNode resultAlias = null;

			var discountProjection = Projections.Conditional(
				Restrictions.Where(() => promotionalSetItemAlias.IsDiscountInMoney),
				Projections.Property(() => promotionalSetItemAlias.DiscountMoney),
				Projections.Property(() => promotionalSetItemAlias.Discount));

			var balanceSubQuery = QueryOver.Of(() => nomenclature2Alias)
				.JoinEntityAlias(
					() => operationAlias,
					() => nomenclature2Alias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin)
				.Where(() => nomenclatureAlias.Id == nomenclature2Alias.Id)
				.AndRestrictionOn(() => operationAlias.Warehouse).IsInG(warehouses)
				.Select(Projections.Sum(() => operationAlias.Amount));

			return uow.Session.QueryOver<PromotionalSetOnlineParameters>()
				.Left.JoinAlias(p => p.PromotionalSet, () => promotionalSetAlias)
				.Left.JoinAlias(() => promotionalSetAlias.PromotionalSetItems, () => promotionalSetItemAlias)
				.Left.JoinAlias(() => promotionalSetItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(p => p.Type == parameterType)
				.And(p => p.PromotionalSetOnlineAvailability != null)
				.And(() => !promotionalSetAlias.IsArchive)
				.SelectList(list => list
					.Select(() => promotionalSetAlias.Id).WithAlias(() => resultAlias.PromotionalSetId)
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => promotionalSetItemAlias.Count).WithAlias(() => resultAlias.Count)
					.Select(discountProjection).WithAlias(() => resultAlias.Discount)
					.Select(() => promotionalSetItemAlias.IsDiscountInMoney).WithAlias(() => resultAlias.IsDiscountMoney)
					.SelectSubQuery(balanceSubQuery).WithAlias(() => resultAlias.Stock)
				)
				.TransformUsing(Transformers.AliasToBean<PromotionalSetItemBalanceNode>())
				.List<PromotionalSetItemBalanceNode>();
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
