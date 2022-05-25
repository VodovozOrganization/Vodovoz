using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;

namespace Vodovoz.Models
{
	public class FastDeliveryAvailabilityHistoryModel
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public FastDeliveryAvailabilityHistoryModel(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public void SaveFastDeliveryAvailabilityHistory(FastDeliveryVerification fastDeliveryVerification, double fastDeliveryMaxDistanceKm, Order order = null)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("FastDeliveryAvailabilityHistoryModel"))
			{

				#region Save FastDeliveryAvailabilityHistory

				var fastDeliveryAvailabilityHistory = new FastDeliveryAvailabilityHistory
				{
					IsValid = fastDeliveryVerification.FastDeliveryVerificationDetailsNodes.Any(x => x.IsValidRLToFastDelivery),
					FastDeliveryMaxDistanceKm = fastDeliveryMaxDistanceKm,
					IsGetClosestByRoute = fastDeliveryVerification.IsGetClosestByRoute
				};

				if(order != null)
				{
					fastDeliveryAvailabilityHistory.Order = order.Id == 0 ? null : order;
					fastDeliveryAvailabilityHistory.Author = order.Author;
					fastDeliveryAvailabilityHistory.DeliveryPoint = order.DeliveryPoint;
					fastDeliveryAvailabilityHistory.District = order.DeliveryPoint.District;
					fastDeliveryAvailabilityHistory.Counterparty = order.Client;
				}

				uow.Save(fastDeliveryAvailabilityHistory);

				#endregion

				#region Save FastDeliveryAvailabilityHistoryItem
				foreach(var node in fastDeliveryVerification.FastDeliveryVerificationDetailsNodes)
				{
					var fastDeliveryAvailabilityHistoryItem = new FastDeliveryAvailabilityHistoryItem
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
						FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory
					};

					uow.Save(fastDeliveryAvailabilityHistoryItem);
				}

				#endregion

				#region Save FastDeliveryOrderItemsHistory

				if(order != null)
				{
					foreach(var orderItem in order.OrderItems)
					{
						var fastDeliveryOrderItemsHistory = new FastDeliveryOrderItemsHistory
						{
							Nomenclature = orderItem.Nomenclature,
							Count = orderItem.Count,
							FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory
						};

						uow.Save(fastDeliveryOrderItemsHistory);
					}
				}

				#endregion

				#region Save FastDeliveryNomenclatureDistributionHistory

				var distributions = uow.GetAll<AdditionalLoadingNomenclatureDistribution>();
				foreach(var distribution in distributions)
				{
					var fastDeliveryNomenclatureDistributionHistory = new FastDeliveryNomenclatureDistributionHistory
					{
						Nomenclature = distribution.Nomenclature,
						Percent = distribution.Percent,
						FastDeliveryAvailabilityHistory = fastDeliveryAvailabilityHistory
					};

					uow.Save(fastDeliveryNomenclatureDistributionHistory);
				}

				#endregion

				uow.Commit();
			}
		}
	}
}
