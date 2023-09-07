using NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Osrm;
using QS.Utilities.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Delivery
{
	public class DeliveryRepository : IDeliveryRepository
	{
		private readonly IGlobalSettings _globalSettings = new GlobalSettings(new ParametersProvider());

		#region Получение районов по координатам

		/// <summary>
		/// Возвращает первый попавшийся район, в котором содержатся переданные координаты
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="districtsSet">Версия районов, из которой будет подбираться район. Если равна null, то район подбирается из активной версии</param>
		public District GetDistrict(IUnitOfWork uow, decimal latitude, decimal longitude, DistrictsSet districtsSet = null)
		{
			var districts = GetDistricts(uow, latitude, longitude, districtsSet);
			return districts.FirstOrDefault();
		}

		/// <summary>
		/// Возвращает все районы, в которых содержатся переданные координаты
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="districtsSet">Версия районов, из которой будут подбираться районы. Если равна null, то районы подбираются из активной версии</param>
		public IEnumerable<District> GetDistricts(IUnitOfWork uow, decimal latitude, decimal longitude, DistrictsSet districtsSet = null)
		{
			Point point = new Point((double)latitude, (double)longitude);

			District districtAlias = null;
			DistrictsSet districtsSetAlias = null;

			var query = uow.Session.QueryOver<District>(() => districtAlias)
				.Left.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.Where(x => x.DistrictBorder != null);

			if(districtsSet == null)
			{
				query.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active);
			}
			else
			{
				query.Where(() => districtsSetAlias.Id == districtsSet.Id);
			}

			var districtsWithBorders = query.List<District>();
			var districts = districtsWithBorders.Where(x => x.DistrictBorder.Contains(point)).ToList();

			if(districts.Any())
			{
				return districts;
			}

			foreach(var nearPoint in Get4PointsInRadiusOfXMetersFromBasePoint(point))
			{
				districts = districtsWithBorders.Where(x => x.DistrictBorder.Contains(nearPoint)).ToList();
				if(districts.Any())
				{
					return districts;
				}
			}
			return new List<District>();
		}

		/// <summary>
		/// Получение 4 точек, отстоящих от базовой точки на <paramref name="distanceInMeters"/> вправо, влево, вверх и вниз.
		/// </summary>
		/// <param name="basePoint">Базовая точка</param>
		/// <param name="distanceInMeters">Дистанция отступа от базовой точки <paramref name="basePoint"/></param>
		private Point[] Get4PointsInRadiusOfXMetersFromBasePoint(Point basePoint, double distanceInMeters = 100)
		{
			Point[] array = new Point[4];
			array[0] = new Point(GetNewLatitude(basePoint.X, distanceInMeters), basePoint.Y);
			array[1] = new Point(basePoint.X, GetNewLongitude(basePoint.Y, distanceInMeters));
			array[2] = new Point(GetNewLatitude(basePoint.X, -distanceInMeters), basePoint.Y);
			array[3] = new Point(basePoint.X, GetNewLongitude(basePoint.Y, -distanceInMeters));
			return array;
		}

		private double GetNewLatitude(double lat, double metersToAdd)
		{
			double earth = 6378.137; //radius of the earth in kilometer
			double pi = Math.PI;
			double m = 1 / (2 * pi / 360 * earth) / 1000; //1 meter in degree

			double newLatitude = lat + (metersToAdd * m);

			return newLatitude;
		}

		private double GetNewLongitude(double lon, double metersToAdd)
		{
			double earth = 6378.137; //radius of the earth in kilometer
			double pi = Math.PI;
			double m = 1 / (2 * pi / 360 * earth) / 1000; //1 meter in degree

			double newLongitude = lon + metersToAdd * m / Math.Cos(lon * (pi / 180));
			return newLongitude;
		}

		#endregion Получение районов по координатам

		#region Fast Delivery

		public FastDeliveryAvailabilityHistory GetRouteListsForFastDelivery(
			IUnitOfWork uow,
			double latitude,
			double longitude,
			bool isGetClosestByRoute,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			Order fastDeliveryOrder = null)
		{
			var maxDistanceToTrackPoint = deliveryRulesParametersProvider.MaxDistanceToLatestTrackPointKm;
			var driverGoodWeightLiftPerHand = deliveryRulesParametersProvider.DriverGoodWeightLiftPerHandInKg;
			var maxFastOrdersPerSpecificTime = deliveryRulesParametersProvider.MaxFastOrdersPerSpecificTime;

			var maxTimeForFastDeliveryTimespan = deliveryRulesParametersProvider.MaxTimeForFastDelivery;
			
			//Переводим всё в минуты
			var trackPointTimeOffset = (int)deliveryRulesParametersProvider.MaxTimeOffsetForLatestTrackPoint.TotalMinutes;
			var maxTimeForFastDelivery = (int)maxTimeForFastDeliveryTimespan.TotalMinutes;
			var minTimeForNewOrder = (int)deliveryRulesParametersProvider.MinTimeForNewFastDeliveryOrder.TotalMinutes;
			var driverUnloadTime = (int)deliveryRulesParametersProvider.DriverUnloadTime.TotalMinutes;
			var specificTimeForFastOrdersCount = (int)deliveryRulesParametersProvider.SpecificTimeForMaxFastOrdersCount.TotalMinutes;

			var fastDeliveryAvailabilityHistory = new FastDeliveryAvailabilityHistory
			{
				IsGetClosestByRoute = isGetClosestByRoute,
				Order = fastDeliveryOrder,
				MaxDistanceToLatestTrackPointKm = maxDistanceToTrackPoint,
				DriverGoodWeightLiftPerHandInKg = driverGoodWeightLiftPerHand,
				MaxFastOrdersPerSpecificTime = maxFastOrdersPerSpecificTime,
				MaxTimeForFastDelivery = maxTimeForFastDeliveryTimespan,
				MinTimeForNewFastDeliveryOrder = deliveryRulesParametersProvider.MinTimeForNewFastDeliveryOrder,
				DriverUnloadTime = deliveryRulesParametersProvider.DriverUnloadTime,
				SpecificTimeForMaxFastOrdersCount = deliveryRulesParametersProvider.SpecificTimeForMaxFastOrdersCount,
			};

			var order = fastDeliveryAvailabilityHistory.Order;
			if(order != null)
			{
				fastDeliveryAvailabilityHistory.Order = order.Id == 0 ? null : order;
				fastDeliveryAvailabilityHistory.Author = order.Author;
				fastDeliveryAvailabilityHistory.DeliveryPoint = order.DeliveryPoint;
				fastDeliveryAvailabilityHistory.District = order.DeliveryPoint.District;
				fastDeliveryAvailabilityHistory.Counterparty = order.Client;
			}

			var fastDeliveryHistoryConverter = new FastDeliveryHistoryConverter();

			if(nomenclatureNodes != null)
			{
				fastDeliveryAvailabilityHistory.OrderItemsHistory =
					fastDeliveryHistoryConverter.ConvertNomenclatureAmountNodesToOrderItemsHistory(nomenclatureNodes, fastDeliveryAvailabilityHistory);
			}

			var distributions = uow.GetAll<AdditionalLoadingNomenclatureDistribution>();
			fastDeliveryAvailabilityHistory.NomenclatureDistributionHistoryItems =
				fastDeliveryHistoryConverter.ConvertNomenclatureDistributionToDistributionHistory(distributions, fastDeliveryAvailabilityHistory);

			var district = GetDistrict(uow, (decimal)latitude, (decimal)longitude);
			if(district?.TariffZone == null || !district.TariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				fastDeliveryAvailabilityHistory.AdditionalInformation =
					new List<string> {"Не найден район, у района отсутствует тарифная зона, либо недоступна экспресс-доставка в текущее время."};

				return fastDeliveryAvailabilityHistory;
			}

			var neededNomenclatures = nomenclatureNodes.ToDictionary(x => x.NomenclatureId, x => x.Amount);

			Track t = null;
			TrackPoint tp = null;
			RouteList rl = null;
			TrackPoint tpInner = null;
			FastDeliveryVerificationDetailsNode result = null;
			Employee e = null;

			RouteListItem rla = null;
			RouteListItem rlaTransfered = null;
			Order o = null;
			OrderItem oi = null;
			OrderEquipment oe = null;
			CarLoadDocument scld = null;
			CarLoadDocumentItem scldi = null;
			CountUnclosedFastDeliveryAddressesNode countUnclosedFastDeliveryAddressesAlias = null;

			RouteListNomenclatureAmount ordersAmountAlias = null;
			RouteListNomenclatureAmount loadDocumentsAmountAlias = null;

			DeliveryFreeBalanceOperation freeBalanceOperation = null;

			var lastTimeTrackQuery = QueryOver.Of(() => tpInner)
				.Where(() => tpInner.Track.Id == t.Id)
				.Select(Projections.Max(() => tpInner.TimeStamp));

			//МЛ только в пути и с погруженным запасом
			var routeListNodes = uow.Session.QueryOver(() => rl)
				.JoinEntityAlias(() => t, () => t.RouteList.Id == rl.Id)
				.Inner.JoinAlias(() => t.TrackPoints, () => tp)
				.Inner.JoinAlias(() => rl.Driver, () => e)
				.WithSubquery.WhereProperty(() => tp.TimeStamp).Eq(lastTimeTrackQuery)
				.And(() => rl.Status == RouteListStatus.EnRoute)
				.And(() => rl.AdditionalLoadingDocument.Id != null) // только с погруженным запасом
				.SelectList(list => list
					.Select(() => tp.TimeStamp).WithAlias(() => result.TimeStamp)
					.Select(() => tp.Latitude).WithAlias(() => result.Latitude)
					.Select(() => tp.Longitude).WithAlias(() => result.Longitude)
					.Select(Projections.Entity(() => rl)).WithAlias(() => result.RouteList))
				.TransformUsing(Transformers.AliasToBean<FastDeliveryVerificationDetailsNode>())
				.List<FastDeliveryVerificationDetailsNode>();

			//Последняя координата в указанном радиусе
			foreach(var node in routeListNodes)
			{
				var distance = DistanceHelper.GetDistanceKm(node.Latitude, node.Longitude, latitude, longitude);
				var deliveryPoint = new PointOnEarth(latitude, longitude);
				var proposedRoute = OsrmClientFactory.Instance
					.GetRoute(new List<PointOnEarth> { new PointOnEarth(node.Latitude, node.Longitude), deliveryPoint }, false, GeometryOverview.False, _globalSettings.ExcludeToll)?.Routes?
					.FirstOrDefault();
				
				node.DistanceByLineToClient.ParameterValue = (decimal)distance;
				node.DistanceByRoadToClient.ParameterValue = decimal.Round((decimal)(proposedRoute?.TotalDistance ?? int.MaxValue) / 1000, 2);

				double routeListFastDeliveryMaxRadius = maxDistanceToTrackPoint;

				var nodeRouteList = uow.GetById<RouteList>(node.RouteList.Id);

				if(nodeRouteList?.FastDeliveryMaxDistanceItems.Count > 0)
				{
					routeListFastDeliveryMaxRadius = (double)nodeRouteList.GetFastDeliveryMaxDistanceValue();
				}

				if(distance < routeListFastDeliveryMaxRadius)
				{
					node.DistanceByLineToClient.IsValidParameter = node.DistanceByRoadToClient.IsValidParameter = true;
				}
				else
				{
					node.DistanceByLineToClient.IsValidParameter = node.DistanceByRoadToClient.IsValidParameter = false;
					node.IsValidRLToFastDelivery = false;
				}

				//Выставляем время последней координаты

				var timeSpan = DateTime.Now - node.TimeStamp;
				node.LastCoordinateTime.ParameterValue = timeSpan.TotalHours > 838 ? new TimeSpan(838, 0, 0) : timeSpan;

				if(node.LastCoordinateTime.ParameterValue.TotalMinutes <= trackPointTimeOffset)
				{
					node.LastCoordinateTime.IsValidParameter = true;
				}
				else
				{
					node.LastCoordinateTime.IsValidParameter = false;
					node.IsValidRLToFastDelivery = false;
				}
			}
			
			routeListNodes = routeListNodes
				.OrderBy(x => isGetClosestByRoute ? x.DistanceByRoadToClient.ParameterValue : x.DistanceByLineToClient.ParameterValue)
				.ToList();

			//Не более определённого кол-ва заказов с быстрой доставкой

			var addressCountSubquery = QueryOver.Of(() => rla)
				.Inner.JoinAlias(() => rla.Order, () => o)
				.Where(() => rla.RouteList.Id == rl.Id)
				.And(() => rla.Status == RouteListItemStatus.EnRoute)
				.And(() => o.IsFastDelivery)
				.Select(Projections.Count(() => rla.Id));

			var routeListsWithCountUnclosedFastDeliveries = uow.Session.QueryOver(() => rl)
				.WhereRestrictionOn(() => rl.Id).IsInG(routeListNodes.Select(x => x.RouteList.Id))
				.SelectList(list => list
					.Select(() => rl.Id).WithAlias(() => countUnclosedFastDeliveryAddressesAlias.RouteListId)
					.SelectSubQuery(addressCountSubquery).WithAlias(() => countUnclosedFastDeliveryAddressesAlias.UnclosedFastDeliveryAddresses))
				.TransformUsing(Transformers.AliasToBean<CountUnclosedFastDeliveryAddressesNode>())
				.List<CountUnclosedFastDeliveryAddressesNode>();

			var rlsWithCountUnclosedFastDeliveries =
				routeListsWithCountUnclosedFastDeliveries.ToDictionary(x => x.RouteListId, x => x.UnclosedFastDeliveryAddresses);

			foreach(var node in routeListNodes)
			{
				var countUnclosedFastDeliveryAddresses = rlsWithCountUnclosedFastDeliveries[node.RouteList.Id];
				node.UnClosedFastDeliveries.ParameterValue = countUnclosedFastDeliveryAddresses;

				var nodeRouteList = uow.GetById<RouteList>(node.RouteList.Id);

				var routeListMaxFastDeliveryOrders = nodeRouteList.GetMaxFastDeliveryOrdersValue();

				if(countUnclosedFastDeliveryAddresses < routeListMaxFastDeliveryOrders)
				{
					node.UnClosedFastDeliveries.IsValidParameter = true;
				}
				else
				{
					node.UnClosedFastDeliveries.IsValidParameter = false;
					node.IsValidRLToFastDelivery = false;
				}
			}
			
			//Время доставки следующего (текущего) заказа позволяет взять быструю доставку
			foreach(var routeListNode in routeListNodes)
			{
				RouteListItem latestAddress = null;

				var orderedEnRouteAddresses = routeListNode.RouteList.Addresses
					.Where(x => x.Status == RouteListItemStatus.EnRoute).OrderBy(x => x.IndexInRoute).ToList();

				var orderedCompletedAddresses = routeListNode.RouteList.Addresses
					.Where(x => x.Status == RouteListItemStatus.Completed).OrderBy(x => x.IndexInRoute).ToList();

				var latestCompletedAddress = orderedCompletedAddresses.OrderByDescending(x => x.StatusLastUpdate).FirstOrDefault();

				if(latestCompletedAddress != null)
				{
					latestAddress = orderedEnRouteAddresses.FirstOrDefault(x => x.IndexInRoute > latestCompletedAddress.IndexInRoute);
				}
				if(latestAddress == null)
				{
					latestAddress = orderedEnRouteAddresses.FirstOrDefault();
				}

				if(latestAddress != null)
				{
					var neededTime1 = maxTimeForFastDelivery - latestAddress.Order.DeliveryPoint.MinutesToUnload;
					if(neededTime1 < minTimeForNewOrder)
					{
						routeListNode.RemainingTimeForShipmentNewOrder.ParameterValue = new TimeSpan(0, neededTime1, 0);
						routeListNode.RemainingTimeForShipmentNewOrder.IsValidParameter = false;
						routeListNode.IsValidRLToFastDelivery = false;
						continue;
					}

					var water19Count = latestAddress.Order.OrderItems
						.Where(x => x.Nomenclature.TareVolume == TareVolume.Vol19L && x.Nomenclature.Category == NomenclatureCategory.water)
						.Sum(x => x.Count);

					var orderItemsSummaryWeight = latestAddress.Order.OrderItems
						.Where(x => x.Nomenclature.TareVolume != TareVolume.Vol19L || x.Nomenclature.Category != NomenclatureCategory.water)
						.Sum(x => x.Nomenclature.Weight * x.Count);

					var orderEquipmentsSummaryWeight = latestAddress.Order.OrderEquipments
						.Where(x => x.Direction == Direction.Deliver)
						.Sum(x => x.Nomenclature.Weight * x.Count);

					var goodsSummaryWeight = orderItemsSummaryWeight + orderEquipmentsSummaryWeight;

					//Время выгрузки след. заказа:
					//(Суммарный вес прочих товаров / кол-во кг, которое водитель может унести в одной руке + кол-во 19л) / 2 руки * время выгрузки в 2 руках 2 бутылей или товара
					var unloadTime = (goodsSummaryWeight / driverGoodWeightLiftPerHand + water19Count) / 2 * driverUnloadTime;
					var neededTime2 = maxTimeForFastDelivery - (int)unloadTime;
					
					if(neededTime2 < minTimeForNewOrder)
					{
						routeListNode.RemainingTimeForShipmentNewOrder.ParameterValue = new TimeSpan(0, neededTime2, 0);
						routeListNode.RemainingTimeForShipmentNewOrder.IsValidParameter = false;
						routeListNode.IsValidRLToFastDelivery = false;
					}
					else
					{
						routeListNode.RemainingTimeForShipmentNewOrder.ParameterValue = new TimeSpan(0, neededTime2, 0);
						routeListNode.RemainingTimeForShipmentNewOrder.IsValidParameter = true;
					}
				}
				else
				{
					routeListNode.RemainingTimeForShipmentNewOrder.ParameterValue = maxTimeForFastDeliveryTimespan;
					routeListNode.RemainingTimeForShipmentNewOrder.IsValidParameter = true;
				}
			}

			var rlIds =  routeListNodes.Select(x => x.RouteList.Id).ToArray();

			var freeBalances = uow.Session.QueryOver(() => freeBalanceOperation)
				.WhereRestrictionOn(() => freeBalanceOperation.RouteList.Id).IsIn(rlIds)
				.WhereRestrictionOn(() => freeBalanceOperation.Nomenclature.Id).IsIn(neededNomenclatures.Keys)
				.SelectList(list => list
					.SelectGroup(() => freeBalanceOperation.Nomenclature.Id).WithAlias(() => ordersAmountAlias.NomenclatureId)
					.SelectGroup(() => freeBalanceOperation.RouteList.Id).WithAlias(() => ordersAmountAlias.RouteListId)
					.SelectSum(() => freeBalanceOperation.Amount).WithAlias(() => ordersAmountAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<RouteListNomenclatureAmount>())
				.List<RouteListNomenclatureAmount>();

			//Выбираем МЛ, в котором хватает запаса номенклатур на поступивший быстрый заказ
			foreach(var routeListNode in routeListNodes)
			{
				var routeListFreeBalance = freeBalances.Where(x => x.RouteListId == routeListNode.RouteList.Id).ToArray();

				foreach(var need in neededNomenclatures)
				{
					var nomenclatureFreeBalance = routeListFreeBalance?.SingleOrDefault(x => x.NomenclatureId == need.Key)?.Amount ?? 0;

					if(nomenclatureFreeBalance < need.Value)
					{
						routeListNode.IsGoodsEnough.ParameterValue = false;
						routeListNode.IsGoodsEnough.IsValidParameter = false;
						routeListNode.IsValidRLToFastDelivery = false;
						break;
					}
				}
			}

			if(routeListNodes != null)
			{
				fastDeliveryAvailabilityHistory.Items = fastDeliveryHistoryConverter
					.ConvertVerificationDetailsNodesToAvailabilityHistoryItems(routeListNodes, fastDeliveryAvailabilityHistory);
			}

			return fastDeliveryAvailabilityHistory;
		}

		private class RouteListNomenclatureAmount
		{
			public int RouteListId { get; set; }
			public int NomenclatureId { get; set; }
			public decimal Amount { get; set; }
		}

		private class RouteListWithCoordinateNode
		{
			public DateTime TimeStamp { get; set; }
			public double Latitude { get; set; }
			public double Longitude { get; set; }
			public RouteList RouteList { get; set; }
		}

		#endregion
	}

	public class CountUnclosedFastDeliveryAddressesNode
	{
		public int RouteListId { get; set; }
		public int UnclosedFastDeliveryAddresses { get; set; }
	}
	
	public class FastDeliveryVerificationDetailsNode
	{
		public string DriverFIO => RouteList.Driver.GetPersonNameWithInitials();
		
		public FastDeliveryVerificationParameter<bool> IsGoodsEnough { get; set; }
			= new FastDeliveryVerificationParameter<bool>
			{
				IsValidParameter = true,
				ParameterValue = true
			};
		public FastDeliveryVerificationParameter<int> UnClosedFastDeliveries { get; set; }
			= new FastDeliveryVerificationParameter<int>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<TimeSpan> RemainingTimeForShipmentNewOrder { get; set; }
			= new FastDeliveryVerificationParameter<TimeSpan>
			{
				IsValidParameter = false,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<TimeSpan> LastCoordinateTime { get; set; }
			= new FastDeliveryVerificationParameter<TimeSpan>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<decimal> DistanceByRoadToClient { get; set; }
			= new FastDeliveryVerificationParameter<decimal>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public FastDeliveryVerificationParameter<decimal> DistanceByLineToClient { get; set; }
			= new FastDeliveryVerificationParameter<decimal>
			{
				IsValidParameter = true,
				ParameterValue = default
			};
		public bool IsValidRLToFastDelivery { get; set; } = true;
		public DateTime TimeStamp { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public RouteList RouteList { get; set; }
		public double RouteListFastDeliveryRadius { get; set; }
		public int RouteListMaxFastDeliveryOrders { get; set; }
	}

	public class FastDeliveryVerificationParameter<T>
		where T : struct
	{
		public T ParameterValue { get; set; }
		public bool IsValidParameter { get; set; }
	}
}
