using MassTransit;
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
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Factories;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools.Orders;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Core.Domain.Goods;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Infrastructure.Persistance.Delivery
{
	internal sealed class DeliveryRepository : IDeliveryRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IGlobalSettings _globalSettings;

		public DeliveryRepository(IUnitOfWorkFactory uowFactory, IDeliveryRulesSettings deliveryRulesSettings, IGlobalSettings globalSettings)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
		}

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

			var query = uow.Session.QueryOver(() => districtAlias)
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
		/// Возвращает точный район по координатам, null если не нашёл
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="latitude"></param>
		/// <param name="longitude"></param>
		/// <returns></returns>
		public District GetAccurateDistrict(IUnitOfWork uow, decimal latitude, decimal longitude)
		{
			var point = new Point((double)latitude, (double)longitude);

			District districtAlias = null;
			DistrictsSet districtsSetAlias = null;

			var districtsWithBorders = uow.Session.QueryOver(() => districtAlias)
				.Left.JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
				.Where(x => x.DistrictBorder != null)
				.And(() => districtsSetAlias.Status == DistrictsSetStatus.Active)
				.List<District>();

			var districts = districtsWithBorders.Where(x => x.DistrictBorder.Contains(point)).ToList();

			return districts?.FirstOrDefault();
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

			double newLatitude = lat + metersToAdd * m;

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
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			int? tariffZoneId,
			bool isRequestFromDesktopApp = true,
			Order fastDeliveryOrder = null)
		{
			var maxDistanceToTrackPoint = MaxDistanceToLatestTrackPointKm;
			var driverGoodWeightLiftPerHand = _deliveryRulesSettings.DriverGoodWeightLiftPerHandInKg;
			var maxFastOrdersPerSpecificTime = _deliveryRulesSettings.MaxFastOrdersPerSpecificTime;

			var maxTimeForFastDeliveryTimespan = _deliveryRulesSettings.MaxTimeForFastDelivery;

			//Переводим всё в минуты
			var trackPointTimeOffset = (int)_deliveryRulesSettings.MaxTimeOffsetForLatestTrackPoint.TotalMinutes;
			var maxTimeForFastDelivery = (int)maxTimeForFastDeliveryTimespan.TotalMinutes;
			var minTimeForNewOrder = (int)_deliveryRulesSettings.MinTimeForNewFastDeliveryOrder.TotalMinutes;
			var driverUnloadTime = (int)_deliveryRulesSettings.DriverUnloadTime.TotalMinutes;
			var specificTimeForFastOrdersCount = (int)_deliveryRulesSettings.SpecificTimeForMaxFastOrdersCount.TotalMinutes;

			var fastDeliveryAvailabilityHistory = new FastDeliveryAvailabilityHistory
			{
				IsGetClosestByRoute = isGetClosestByRoute,
				Order = fastDeliveryOrder,
				MaxDistanceToLatestTrackPointKm = maxDistanceToTrackPoint,
				DriverGoodWeightLiftPerHandInKg = driverGoodWeightLiftPerHand,
				MaxFastOrdersPerSpecificTime = maxFastOrdersPerSpecificTime,
				MaxTimeForFastDelivery = maxTimeForFastDeliveryTimespan,
				MinTimeForNewFastDeliveryOrder = _deliveryRulesSettings.MinTimeForNewFastDeliveryOrder,
				DriverUnloadTime = _deliveryRulesSettings.DriverUnloadTime,
				SpecificTimeForMaxFastOrdersCount = _deliveryRulesSettings.SpecificTimeForMaxFastOrdersCount,
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

			var tariffZone = tariffZoneId.HasValue
				? uow.GetById<TariffZone>(tariffZoneId.Value)
				: GetDistrict(uow, (decimal)latitude, (decimal)longitude)?.TariffZone;

			if(tariffZone == null || !tariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				fastDeliveryAvailabilityHistory.AdditionalInformation =
					new List<string> { "Не найден район, у района отсутствует тарифная зона, либо недоступна экспресс-доставка в текущее время." };

				return fastDeliveryAvailabilityHistory;
			}

			var neededNomenclatures = nomenclatureNodes.ToDictionary(x => x.NomenclatureId, x => x.Amount);

			Track t = null;
			TrackPoint tp = null;
			RouteList rl = null;
			TrackPoint tpInner = null;
			FastDeliveryVerificationDetailsNode resultAlias = null;
			Employee e = null;

			RouteListItem rla = null;
			RouteListItem rlaTransfered = null;
			Order o = null;
			OrderItem oi = null;
			OrderEquipment oe = null;
			CarLoadDocument scld = null;
			CarLoadDocumentItem scldi = null;
			RouteListFastDeliveriesCountNode routeListFastDeliveriesCountAlias = null;

			RouteListNomenclatureAmount ordersAmountAlias = null;
			RouteListNomenclatureAmount loadDocumentsAmountAlias = null;

			DeliveryFreeBalanceOperation freeBalanceOperation = null;

			var date = DateTime.Now;

			var lastTimeTrackQuery = QueryOver.Of(() => tpInner)
				.Where(() => tpInner.Track.Id == t.Id)
				.Select(Projections.Max(() => tpInner.TimeStamp));

			var fastDeliveryMaxDistanceParameterVersion = QueryOver.Of<FastDeliveryMaxDistanceParameterVersion>()
				.Where(v => v.StartDate <= date
					&& (v.EndDate == null || v.EndDate > date))
				.Select(v => v.Value)
				.Take(1);

			var routeListFastDeliveryMaxDistance = QueryOver.Of<RouteListFastDeliveryMaxDistance>()
				.Where(d =>
					d.RouteList.Id == rl.Id
					&& d.StartDate <= date
					&& (d.EndDate == null || d.EndDate > date))
				.Select(d => d.Distance)
				.Take(1);

			//МЛ только в пути и с погруженным запасом
			var routeListNodes = uow.Session.QueryOver(() => rl)
				.JoinEntityAlias(() => t, () => t.RouteList.Id == rl.Id)
				.Inner.JoinAlias(() => t.TrackPoints, () => tp)
				.Inner.JoinAlias(() => rl.Driver, () => e)
				.WithSubquery.WhereProperty(() => tp.TimeStamp).Eq(lastTimeTrackQuery)
				.And(() => rl.Status == RouteListStatus.EnRoute)
				.And(() => rl.AdditionalLoadingDocument.Id != null) // только с погруженным запасом
				.SelectList(list => list
					.Select(() => tp.TimeStamp).WithAlias(() => resultAlias.TimeStamp)
					.Select(() => tp.Latitude).WithAlias(() => resultAlias.Latitude)
					.Select(() => tp.Longitude).WithAlias(() => resultAlias.Longitude)
					.Select(Projections.Entity(() => rl)).WithAlias(() => resultAlias.RouteList)
					.Select(() => rl.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(Projections.SubQuery(routeListFastDeliveryMaxDistance))
						.WithAlias(() => resultAlias.RouteListFastDeliveryMaxDistance)
					.Select(Projections.SubQuery(fastDeliveryMaxDistanceParameterVersion))
						.WithAlias(() => resultAlias.FastDeliveryMaxDistanceParameterVersion)
				)
				.TransformUsing(Transformers.AliasToBean<FastDeliveryVerificationDetailsNode>())
				.List<FastDeliveryVerificationDetailsNode>();

			var routeListIds = routeListNodes.Select(x => x.RouteList.Id).ToArray();

			//Не более определённого кол-ва заказов с быстрой доставкой
			var routeListMaxFastDeliveryOrdersCount = QueryOver.Of<RouteListMaxFastDeliveryOrders>()
				.Where(d =>
					d.RouteList.Id == rl.Id
					&& d.StartDate <= date
					&& (d.EndDate == null || d.EndDate > date))
				.Select(d => d.MaxOrders)
				.Take(1);

			var maxFastDeliveryOrdersCount = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "IFNULL(?1, ?2)"),
				NHibernateUtil.Int32,
				Projections.SubQuery(routeListMaxFastDeliveryOrdersCount),
				Projections.Constant(_deliveryRulesSettings.MaxFastOrdersPerSpecificTime)
			);

			var fastDeliveryCountSubquery = QueryOver.Of(() => rla)
				.Inner.JoinAlias(() => rla.Order, () => o)
				.Where(() => rla.RouteList.Id == rl.Id)
				.And(() => rla.Status == RouteListItemStatus.EnRoute)
				.And(() => o.IsFastDelivery)
				.Select(Projections.Count(() => rla.Id));

			var routeListFastDeliveriesCount = uow.Session.QueryOver(() => rl)
				.WhereRestrictionOn(() => rl.Id).IsInG(routeListIds)
				.SelectList(list => list
					.Select(() => rl.Id).WithAlias(() => routeListFastDeliveriesCountAlias.RouteListId)
					.SelectSubQuery(fastDeliveryCountSubquery)
						.WithAlias(() => routeListFastDeliveriesCountAlias.UnclosedFastDeliveryAddresses)
					.Select(maxFastDeliveryOrdersCount)
						.WithAlias(() => routeListFastDeliveriesCountAlias.MaxFastDeliveryOrdersCount)
				)
				.TransformUsing(Transformers.AliasToBean<RouteListFastDeliveriesCountNode>())
				.List<RouteListFastDeliveriesCountNode>();

			DeliveryPoint deliveryPointAlias = null;
			Nomenclature nomenclatureAlias = null;
			AddressInfoForFastDelivery addressInfoAlias = null;

			var waterCount = QueryOver.Of(() => oi)
				.JoinAlias(() => oi.Nomenclature, () => nomenclatureAlias)
				.Where(() => oi.Order.Id == o.Id)
				.And(() => nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.And(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.Select(Projections.Sum(() => oi.Count));

			var itemsSummaryWeight = QueryOver.Of(() => oi)
				.JoinAlias(() => oi.Nomenclature, () => nomenclatureAlias)
				.Where(() => oi.Order.Id == o.Id)
				.And(() => nomenclatureAlias.TareVolume != TareVolume.Vol19L
					|| nomenclatureAlias.Category != NomenclatureCategory.water)
				.Select(Projections.Sum(() => nomenclatureAlias.Weight * oi.Count));

			var equipmentsSummaryWeight = QueryOver.Of(() => oe)
				.JoinAlias(() => oe.Nomenclature, () => nomenclatureAlias)
				.Where(() => oe.Order.Id == o.Id)
				.And(() => oe.Direction == Direction.Deliver)
				.Select(Projections.Sum(() => nomenclatureAlias.Weight * oe.Count));

			var addressesLookup = uow.Session.QueryOver<RouteListItem>()
				.JoinAlias(address => address.Order, () => o)
				.JoinAlias(() => o.DeliveryPoint, () => deliveryPointAlias)
				.WhereRestrictionOn(address => address.RouteList.Id).IsInG(routeListIds)
				.SelectList(list => list
					.Select(address => address.RouteList.Id).WithAlias(() => addressInfoAlias.RouteListId)
					.Select(address => address.IndexInRoute).WithAlias(() => addressInfoAlias.IndexInRoute)
					.Select(address => address.Status).WithAlias(() => addressInfoAlias.AddressStatus)
					.Select(address => address.StatusLastUpdate).WithAlias(() => addressInfoAlias.StatusLastUpdate)
					.Select(() => deliveryPointAlias.MinutesToUnload).WithAlias(() => addressInfoAlias.MinutesToUnload)
					.Select(Projections.SubQuery(waterCount)).WithAlias(() => addressInfoAlias.WaterCount)
					.Select(Projections.SubQuery(itemsSummaryWeight)).WithAlias(() => addressInfoAlias.ItemsSummaryWeight)
					.Select(Projections.SubQuery(equipmentsSummaryWeight)).WithAlias(() => addressInfoAlias.EquipmentsSummaryWeight)
				)
				.TransformUsing(Transformers.AliasToBean<AddressInfoForFastDelivery>())
				.List<AddressInfoForFastDelivery>()
				.ToLookup(x => x.RouteListId);

			var rlsFastDeliveriesCount =
				routeListFastDeliveriesCount.ToDictionary(x => x.RouteListId);

			var freeBalances = uow.Session.QueryOver(() => freeBalanceOperation)
				.WhereRestrictionOn(() => freeBalanceOperation.RouteList.Id).IsIn(routeListIds)
				.WhereRestrictionOn(() => freeBalanceOperation.Nomenclature.Id).IsIn(neededNomenclatures.Keys)
				.SelectList(list => list
					.SelectGroup(() => freeBalanceOperation.Nomenclature.Id).WithAlias(() => ordersAmountAlias.NomenclatureId)
					.SelectGroup(() => freeBalanceOperation.RouteList.Id).WithAlias(() => ordersAmountAlias.RouteListId)
					.SelectSum(() => freeBalanceOperation.Amount).WithAlias(() => ordersAmountAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<RouteListNomenclatureAmount>())
				.List<RouteListNomenclatureAmount>();

			var i = 0;

			if(isRequestFromDesktopApp)
			{
				foreach(var node in routeListNodes)
				{
					UpdateDistanceAndLastCoordinateTimeParameters(node);
					UpdateUnClosedFastDeliveriesParameter(node);
					UpdateRemainingTimeForShipmentNewOrder(node);
					UpdateRouteListBalanceParameter(node);
				}

				routeListNodes = routeListNodes
					.OrderBy(x => isGetClosestByRoute ? x.DistanceByRoadToClient.ParameterValue : x.DistanceByLineToClient.ParameterValue)
					.ToList();
			}
			else
			{
				foreach(var node in routeListNodes)
				{
					UpdateDistanceAndLastCoordinateTimeParameters(node);
				}

				routeListNodes = routeListNodes
					.OrderBy(x => isGetClosestByRoute ? x.DistanceByRoadToClient.ParameterValue : x.DistanceByLineToClient.ParameterValue)
					.ToList();

				foreach(var node in routeListNodes)
				{
					UpdateUnClosedFastDeliveriesParameter(node);
					UpdateRemainingTimeForShipmentNewOrder(node);
					UpdateRouteListBalanceParameter(node);
					i++;

					if(node.IsValidRLToFastDelivery)
					{
						break;
					}
				}

				if(i < routeListNodes.Count)
				{
					routeListNodes = routeListNodes.Take(i).ToList();
				}
			}

			if(routeListNodes.Any())
			{
				fastDeliveryAvailabilityHistory.Items = fastDeliveryHistoryConverter
					.ConvertVerificationDetailsNodesToAvailabilityHistoryItems(routeListNodes, fastDeliveryAvailabilityHistory);
			}

			return fastDeliveryAvailabilityHistory;

			//Последняя координата в указанном радиусе
			void UpdateDistanceAndLastCoordinateTimeParameters(FastDeliveryVerificationDetailsNode node)
			{
				var distance = DistanceHelper.GetDistanceKm(node.Latitude, node.Longitude, latitude, longitude);
				var deliveryPoint = new PointOnEarth(latitude, longitude);
				var proposedRoute = OsrmClientFactory.Instance
					.GetRoute(new List<PointOnEarth> { new PointOnEarth(node.Latitude, node.Longitude), deliveryPoint }, false, GeometryOverview.False, _globalSettings.ExcludeToll)?.Routes?
					.FirstOrDefault();

				node.DistanceByLineToClient.ParameterValue = (decimal)distance;
				node.DistanceByRoadToClient.ParameterValue = decimal.Round((decimal)(proposedRoute?.TotalDistance ?? int.MaxValue) / 1000, 2);

				double routeListFastDeliveryMaxRadius;

				if(node.RouteListFastDeliveryMaxDistance.HasValue)
				{
					routeListFastDeliveryMaxRadius = (double)node.RouteListFastDeliveryMaxDistance;
				}
				else
				{
					routeListFastDeliveryMaxRadius = node.FastDeliveryMaxDistanceParameterVersion;
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

			void UpdateUnClosedFastDeliveriesParameter(FastDeliveryVerificationDetailsNode node)
			{
				var fastDeliveriesInfo = rlsFastDeliveriesCount[node.RouteListId];
				var unclosedFastDeliveryAddresses = fastDeliveriesInfo.UnclosedFastDeliveryAddresses;

				node.UnClosedFastDeliveries.ParameterValue = unclosedFastDeliveryAddresses;

				var routeListMaxFastDeliveryOrders = fastDeliveriesInfo.MaxFastDeliveryOrdersCount;

				if(unclosedFastDeliveryAddresses < routeListMaxFastDeliveryOrders)
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
			void UpdateRemainingTimeForShipmentNewOrder(FastDeliveryVerificationDetailsNode node)
			{
				AddressInfoForFastDelivery latestAddress = null;

				var addresses = addressesLookup.Contains(node.RouteListId)
					? addressesLookup[node.RouteListId]
					: Array.Empty<AddressInfoForFastDelivery>();

				var orderedEnRouteAddresses = addresses
					.Where(x => x.AddressStatus == RouteListItemStatus.EnRoute)
					.OrderBy(x => x.IndexInRoute)
					.ToList();

				var orderedCompletedAddresses = addresses
					.Where(x => x.AddressStatus == RouteListItemStatus.Completed)
					.OrderBy(x => x.IndexInRoute)
					.ToList();

				var latestCompletedAddress = orderedCompletedAddresses
					.OrderByDescending(x => x.StatusLastUpdate)
					.FirstOrDefault();

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
					var neededTime1 = maxTimeForFastDelivery - latestAddress.MinutesToUnload;
					if(neededTime1 < minTimeForNewOrder)
					{
						node.RemainingTimeForShipmentNewOrder.ParameterValue = new TimeSpan(0, neededTime1, 0);
						node.RemainingTimeForShipmentNewOrder.IsValidParameter = false;
						node.IsValidRLToFastDelivery = false;
						return;
					}

					var water19Count = latestAddress.WaterCount;
					var orderItemsSummaryWeight = latestAddress.ItemsSummaryWeight;

					var orderEquipmentsSummaryWeight = latestAddress.EquipmentsSummaryWeight;
					var goodsSummaryWeight = orderItemsSummaryWeight + orderEquipmentsSummaryWeight;

					//Время выгрузки след. заказа:
					//(Суммарный вес прочих товаров / кол-во кг, которое водитель может унести в одной руке + кол-во 19л) / 2 руки * время выгрузки в 2 руках 2 бутылей или товара
					var unloadTime = (goodsSummaryWeight / driverGoodWeightLiftPerHand + water19Count) / 2 * driverUnloadTime;
					var neededTime2 = maxTimeForFastDelivery - (int)unloadTime;

					if(neededTime2 < minTimeForNewOrder)
					{
						node.RemainingTimeForShipmentNewOrder.ParameterValue = new TimeSpan(0, neededTime2, 0);
						node.RemainingTimeForShipmentNewOrder.IsValidParameter = false;
						node.IsValidRLToFastDelivery = false;
					}
					else
					{
						node.RemainingTimeForShipmentNewOrder.ParameterValue = new TimeSpan(0, neededTime2, 0);
						node.RemainingTimeForShipmentNewOrder.IsValidParameter = true;
					}
				}
				else
				{
					node.RemainingTimeForShipmentNewOrder.ParameterValue = maxTimeForFastDeliveryTimespan;
					node.RemainingTimeForShipmentNewOrder.IsValidParameter = true;
				}
			}

			//Выбираем МЛ, в котором хватает запаса номенклатур на поступивший быстрый заказ
			void UpdateRouteListBalanceParameter(FastDeliveryVerificationDetailsNode node)
			{
				var routeListFreeBalance = freeBalances.Where(x => x.RouteListId == node.RouteList.Id).ToArray();

				foreach(var need in neededNomenclatures)
				{
					var nomenclatureFreeBalance = routeListFreeBalance?.SingleOrDefault(x => x.NomenclatureId == need.Key)?.Amount ?? 0;

					if(nomenclatureFreeBalance < need.Value)
					{
						node.IsGoodsEnough.ParameterValue = false;
						node.IsGoodsEnough.IsValidParameter = false;
						node.IsValidRLToFastDelivery = false;
						break;
					}
				}
			}
		}

		#endregion

		public double MaxDistanceToLatestTrackPointKm
		{
			get
			{
				using(var unitOfWork = _uowFactory.CreateWithoutRoot())
				{
					return unitOfWork.Query<FastDeliveryMaxDistanceParameterVersion>()
						.Where(x => x.EndDate == null)
						.SingleOrDefault().Value;
				}
			}
		}

		public double GetMaxDistanceToLatestTrackPointKmFor(DateTime dateTime)
		{
			using(var unitOfWork = _uowFactory.CreateWithoutRoot())
			{
				FastDeliveryMaxDistanceParameterVersion fastDeliveryMaxDistanceParameterVersionAlias = null;

				return unitOfWork.Session.QueryOver(() => fastDeliveryMaxDistanceParameterVersionAlias)
					.Where(Restrictions.And(
						Restrictions.Le(Projections.Property(() => fastDeliveryMaxDistanceParameterVersionAlias.StartDate), dateTime),
						Restrictions.Or(
							Restrictions.Gt(Projections.Property(() => fastDeliveryMaxDistanceParameterVersionAlias.EndDate), dateTime),
							Restrictions.IsNull(Projections.Property(() => fastDeliveryMaxDistanceParameterVersionAlias.EndDate)))))
					.SingleOrDefault().Value;
			}
		}

		public void UpdateFastDeliveryMaxDistanceParameter(double value)
		{
			using(var unitOfWork = _uowFactory.CreateWithoutRoot())
			{
				var activationTime = DateTime.Now;

				var lastVersion = unitOfWork.Query<FastDeliveryMaxDistanceParameterVersion>()
					.Where(x => x.EndDate == null)
					.SingleOrDefault();

				if(lastVersion.Value == value)
				{
					return;
				}

				lastVersion.EndDate = activationTime;

				var newVersion = new FastDeliveryMaxDistanceParameterVersion
				{
					StartDate = activationTime,
					Value = value
				};

				unitOfWork.Save(lastVersion);
				unitOfWork.Save(newVersion);

				unitOfWork.Commit();
			}
		}

		public IList<Order> GetFastDeliveryLateOrders(IUnitOfWork uow, DateTime fromDateTime,
			IGeneralSettings generalSettings, int complaintDetalizationId)
		{
			var fastDeliveryIntervalFrom = generalSettings.FastDeliveryIntervalFrom;
			var fastDeliveryMaximumPermissibleLate = generalSettings.FastDeliveryMaximumPermissibleLateMinutes;
			var maxTimeForFastDelivery = _deliveryRulesSettings.MaxTimeForFastDelivery;
			var maxTimeForFastDeliveryWithLateMinutes = maxTimeForFastDelivery.TotalMinutes + fastDeliveryMaximumPermissibleLate;

			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Order orderAlias = null;
			Complaint complaintAlias = null;

			var alreadyExistsComplaintSubquery = QueryOver.Of(() => complaintAlias)
				.Where(() => complaintAlias.Order.Id == orderAlias.Id)
				.And(() => complaintAlias.ComplaintDetalization.Id == complaintDetalizationId)
				.Select(Projections.Property(() => complaintAlias.Id))
				.Take(1);

			var fastDeliveryOrdersLateQuery = uow.Session.QueryOver(() => orderAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => orderAlias.Id == routeListItemAlias.Order.Id)
				.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Where(() => orderAlias.IsFastDelivery)
				.And(() => orderAlias.DeliveryDate >= fromDateTime)
				.And(() => routeListItemAlias.Status == RouteListItemStatus.EnRoute)
				.WithSubquery.WhereNotExists(alreadyExistsComplaintSubquery)
				;

			IProjection intervalProjection = null;

			if(fastDeliveryIntervalFrom == FastDeliveryIntervalFromEnum.OrderCreated)
			{
				intervalProjection = Projections.Property(() => orderAlias.CreateDate);
			}

			if(fastDeliveryIntervalFrom == FastDeliveryIntervalFromEnum.AddedInFirstRouteList)
			{
				var rlaFirstSubquery = QueryOver.Of(() => routeListItemAlias)
					.Where(() => routeListItemAlias.Order.Id == orderAlias.Id)
					.OrderBy(() => routeListItemAlias.CreationDate).Asc()
					.Select(Projections.Property(() => routeListItemAlias.CreationDate))
					.Take(1);

				intervalProjection = Projections.SubQuery(rlaFirstSubquery);
			}

			if(fastDeliveryIntervalFrom == FastDeliveryIntervalFromEnum.RouteListItemTransfered)
			{
				intervalProjection = Projections.Property(() => routeListItemAlias.CreationDate);
			}

			var dateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(
					NHibernateUtil.Date,
					"DATE_ADD(?1, INTERVAL ?2 MINUTE)"
					),
				NHibernateUtil.Date,
				intervalProjection,
				Projections.Constant(maxTimeForFastDeliveryWithLateMinutes)
			);

			fastDeliveryOrdersLateQuery.Where(Restrictions.Lt(dateProjection, DateTime.Now));

			return fastDeliveryOrdersLateQuery
				.TransformUsing(Transformers.RootEntity)
				.List<Order>();
		}

		public ServiceDistrict GetServiceDistrictByCoordinates(IUnitOfWork unitOfWork, decimal latitude, decimal longitude)
		{
			var point = new Point((double)latitude, (double)longitude);

			var serviceDistricts =
				(
					from serviceDistrict in unitOfWork.Session.Query<ServiceDistrict>()
					join serviceDistrictSet in unitOfWork.Session.Query<ServiceDistrictsSet>()
					on serviceDistrict.ServiceDistrictsSet.Id equals serviceDistrictSet.Id
					where serviceDistrictSet.Status == ServiceDistrictsSetStatus.Active
						&& serviceDistrict.ServiceDistrictBorder != null
					select serviceDistrict
				)
				.ToList();
			
			var result = serviceDistricts.FirstOrDefault(x => x.ServiceDistrictBorder.Contains(point));

			return result;
		}
	}
}
