using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Nodes;

namespace Vodovoz.Tools.Orders
{
	public class FastDeliveryHistoryConverter
	{
		public IList<FastDeliveryVerificationDetailsNode> ConvertAvailabilityHistoryItemsToVerificationDetailsNodes(
			IEnumerable<FastDeliveryAvailabilityHistoryItem> items)
		{
			var nodes = new List<FastDeliveryVerificationDetailsNode>();

			foreach(var item in items)
			{
				DateTime dateOfRouteListFastDeliveryMaxDistance, dateOfRouteListMaxFastDeliveryOrders;
				dateOfRouteListFastDeliveryMaxDistance = dateOfRouteListMaxFastDeliveryOrders =
					item.FastDeliveryAvailabilityHistory.VerificationDate > DateTime.MinValue 
					? item.FastDeliveryAvailabilityHistory.VerificationDate
					: DateTime.Now;

				double routeListFastDeliveryRadius = (double)item.RouteList.GetFastDeliveryMaxDistanceValue(dateOfRouteListFastDeliveryMaxDistance);
				var routeListMaxFastDeliveryOrders = item.RouteList.GetMaxFastDeliveryOrdersValue(dateOfRouteListMaxFastDeliveryOrders);

				var node = new FastDeliveryVerificationDetailsNode
				{
					RouteList = item.RouteList,
					IsValidRLToFastDelivery = item.IsValidToFastDelivery,

					RemainingTimeForShipmentNewOrder = new FastDeliveryVerificationParameter<TimeSpan>
					{
						IsValidParameter = item.IsValidRemainingTimeForShipmentNewOrder,
						ParameterValue = item.RemainingTimeForShipmentNewOrder
					},
					DistanceByLineToClient = new FastDeliveryVerificationParameter<decimal>
					{
						IsValidParameter = item.IsValidDistanceByLineToClient,
						ParameterValue = item.DistanceByLineToClient
					},
					DistanceByRoadToClient = new FastDeliveryVerificationParameter<decimal>
					{
						IsValidParameter = item.IsValidDistanceByRoadToClient,
						ParameterValue = item.DistanceByRoadToClient
					},
					IsGoodsEnough = new FastDeliveryVerificationParameter<bool>
					{
						IsValidParameter = item.IsValidIsGoodsEnough,
						ParameterValue = item.IsGoodsEnough
					},
					LastCoordinateTime = new FastDeliveryVerificationParameter<TimeSpan>
					{
						IsValidParameter = item.IsValidLastCoordinateTime,
						ParameterValue = item.LastCoordinateTimeElapsed
					},
					UnClosedFastDeliveries = new FastDeliveryVerificationParameter<int>
					{
						IsValidParameter = item.IsValidUnclosedFastDeliveries,
						ParameterValue = item.UnclosedFastDeliveries
					},
					RouteListFastDeliveryRadius = routeListFastDeliveryRadius,
					RouteListMaxFastDeliveryOrders = routeListMaxFastDeliveryOrders
			};

				nodes.Add(node);
			}

			return nodes;
		}

		public IList<FastDeliveryAvailabilityHistoryItem> ConvertVerificationDetailsNodesToAvailabilityHistoryItems(
			IEnumerable<FastDeliveryVerificationDetailsNode> nodes, FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory)
		{
			var items = new List<FastDeliveryAvailabilityHistoryItem>();

			foreach(var node in nodes)
			{
				var item = new FastDeliveryAvailabilityHistoryItem
				{
					DistanceByLineToClient = node.DistanceByLineToClient.ParameterValue,
					IsValidDistanceByLineToClient = node.DistanceByLineToClient.IsValidParameter,
					DistanceByRoadToClient = node.DistanceByRoadToClient.ParameterValue,
					IsValidDistanceByRoadToClient = node.DistanceByRoadToClient.IsValidParameter,
					Driver = node.RouteList.Driver,
					IsGoodsEnough = node.IsGoodsEnough.ParameterValue,
					IsValidIsGoodsEnough = node.IsGoodsEnough.IsValidParameter,
					LastCoordinateTimeElapsed =  node.LastCoordinateTime.ParameterValue,
					IsValidLastCoordinateTime = node.LastCoordinateTime.IsValidParameter,
					RemainingTimeForShipmentNewOrder = node.RemainingTimeForShipmentNewOrder.ParameterValue,
					IsValidRemainingTimeForShipmentNewOrder = node.RemainingTimeForShipmentNewOrder.IsValidParameter,
					RouteList = node.RouteList,
					UnclosedFastDeliveries = node.UnClosedFastDeliveries.ParameterValue,
					IsValidUnclosedFastDeliveries = node.UnClosedFastDeliveries.IsValidParameter,
					IsValidToFastDelivery = node.IsValidRLToFastDelivery,
					FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory
				};

				items.Add(item);
			}

			return items;
		}

		public IList<FastDeliveryOrderItemHistory> ConvertNomenclatureAmountNodesToOrderItemsHistory(
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes, 
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory
			)
		{
			return nomenclatureNodes
				.Select(x => new FastDeliveryOrderItemHistory
				{
					Nomenclature = x.Nomenclature ?? new Nomenclature { Id = x.NomenclatureId },
					Count = x.Amount,
					FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory
				})
				.ToList();
		}

		public IList<FastDeliveryNomenclatureDistributionHistory> ConvertNomenclatureDistributionToDistributionHistory(
			IEnumerable<AdditionalLoadingNomenclatureDistribution> distributions, FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistory)
		{
			return distributions.Select(x =>
					new FastDeliveryNomenclatureDistributionHistory
					{
						Nomenclature = x.Nomenclature,
						Percent = x.Percent,
						FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory
					})
				.ToList();
		}
	}
}
