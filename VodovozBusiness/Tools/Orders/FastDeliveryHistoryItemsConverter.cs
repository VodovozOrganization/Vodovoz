using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.EntityRepositories.Delivery;

namespace Vodovoz.Tools.Orders
{
	public class FastDeliveryHistoryItemsConverter
	{
		public IEnumerable<FastDeliveryVerificationDetailsNode> ConvertAvailabilityHistoryItemsToVerificationDetailsNodes(IEnumerable<FastDeliveryAvailabilityHistoryItem> items)
		{
			var nodes = new List<FastDeliveryVerificationDetailsNode>();

			foreach(var item in items)
			{
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
						ParameterValue = item.LastCoordinateTime
					},
					UnClosedFastDeliveries = new FastDeliveryVerificationParameter<int>
					{
						IsValidParameter = item.IsValidUnclosedFastDeliveries,
						ParameterValue = item.UnclosedFastDeliveries
					}
				};

				nodes.Add(node);
			}

			return nodes;
		}

		public FastDeliveryAvailabilityHistoryItem ConvertVerificationDetailsNodeToAvailabilityHistoryItem(FastDeliveryVerificationDetailsNode node)
		{
			return new FastDeliveryAvailabilityHistoryItem
			{
				DistanceByLineToClient = node.DistanceByLineToClient.ParameterValue,
				IsValidDistanceByLineToClient = node.DistanceByLineToClient.IsValidParameter,
				DistanceByRoadToClient = node.DistanceByRoadToClient.ParameterValue,
				IsValidDistanceByRoadToClient = node.DistanceByRoadToClient.IsValidParameter,
				Driver = node.RouteList.Driver,
				IsGoodsEnough = node.IsGoodsEnough.ParameterValue,
				IsValidIsGoodsEnough = node.IsGoodsEnough.IsValidParameter,
				LastCoordinateTime = node.LastCoordinateTime.ParameterValue,
				IsValidLastCoordinateTime = node.LastCoordinateTime.IsValidParameter,
				RemainingTimeForShipmentNewOrder = node.RemainingTimeForShipmentNewOrder.ParameterValue,
				IsValidRemainingTimeForShipmentNewOrder = node.RemainingTimeForShipmentNewOrder.IsValidParameter,
				RouteList = node.RouteList,
				UnclosedFastDeliveries = node.UnClosedFastDeliveries.ParameterValue,
				IsValidUnclosedFastDeliveries = node.UnClosedFastDeliveries.IsValidParameter,
				IsValidToFastDelivery = node.IsValidRLToFastDelivery
			};

		}
	}
}
