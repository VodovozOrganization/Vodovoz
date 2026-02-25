using Microsoft.Extensions.Logging;
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
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Service;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Delivery
{
	internal sealed class DeliveryRepository : IDeliveryRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IOsrmSettings _osrmSettings;
		private readonly IOsrmClient _osrmClient;

		public DeliveryRepository(
			IUnitOfWorkFactory uowFactory, 
			IDeliveryRulesSettings deliveryRulesSettings, 
			IOsrmSettings osrmSettings,
			IOsrmClient osrmClient
		)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_osrmSettings = osrmSettings ?? throw new ArgumentNullException(nameof(osrmSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
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
		/// Возвращает первый попавшийся район, в котором содержатся переданные координаты
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="districtsSet">Версия районов, из которой будет подбираться район. Если равна null, то район подбирается из активной версии</param>
		public async Task<District> GetDistrictAsync(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken,
			DistrictsSet districtsSet = null
		)
		{
			var districts = await GetDistrictsAsync(uow, latitude, longitude, cancellationToken, districtsSet);
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
			var query = GetDistrictsQuery(uow, districtsSet);

			var districtsWithBorders = query.List<District>();

			var point = new Point((double)latitude, (double)longitude);
			return GetDistrictsByPoint(point, districtsWithBorders);
		}

		/// <summary>
		/// Возвращает все районы, в которых содержатся переданные координаты
		/// </summary>
		/// <param name="uow">Unit of Work</param>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="districtsSet">Версия районов, из которой будут подбираться районы. Если равна null, то районы подбираются из активной версии</param>
		public async Task<IEnumerable<District>> GetDistrictsAsync(
			IUnitOfWork uow,
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken,
			DistrictsSet districtsSet = null
			)
		{
			var query = GetDistrictsQuery(uow, districtsSet);

			var districtsWithBorders = await query.ListAsync<District>(cancellationToken);

			var point = new Point((double)latitude, (double)longitude);
			return GetDistrictsByPoint(point, districtsWithBorders);
		}

		private IQueryOver<District, District> GetDistrictsQuery(IUnitOfWork uow, DistrictsSet districtsSet)
		{
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

			return query;
		}

		private IEnumerable<District> GetDistrictsByPoint(Point point, IList<District> districtsWithBorders)
		{
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

		public FastDeliveryAvailabilityHistory GetRouteListsForFastDeliveryForOrder(
			IUnitOfWork uow,
			double latitude,
			double longitude,
			bool isGetClosestByRoute,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			int? tariffZoneId,
			Order fastDeliveryOrder
		)
		{
			var date = DateTime.Now;

			var maxDistanceToTrackPoint = GetGetMaxDistanceToLatestTrackPointKm(uow);

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
					fastDeliveryHistoryConverter.ConvertNomenclatureAmountNodesToOrderItemsHistory(
						nomenclatureNodes,
						fastDeliveryAvailabilityHistory
					);
			}

			var distributions = uow.GetAll<AdditionalLoadingNomenclatureDistribution>();
			fastDeliveryAvailabilityHistory.NomenclatureDistributionHistoryItems =
				fastDeliveryHistoryConverter.ConvertNomenclatureDistributionToDistributionHistory(
					distributions,
					fastDeliveryAvailabilityHistory
				);

			TariffZone tariffZone;
			if(tariffZoneId.HasValue)
			{
				tariffZone = uow.Session.Get<TariffZone>(tariffZoneId.Value);
			}
			else
			{
				var district = GetDistrict(uow, (decimal)latitude, (decimal)longitude);
				tariffZone = district?.TariffZone;
			}

			if(tariffZone == null || !tariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				fastDeliveryAvailabilityHistory.AdditionalInformation =
					new List<string> { "Не найден район, у района отсутствует тарифная зона, " +
					"либо недоступна экспресс-доставка в текущее время." };

				return fastDeliveryAvailabilityHistory;
			}

			var neededNomenclatures = nomenclatureNodes
				.GroupBy(x => x.NomenclatureId)
				.ToDictionary(
					g => g.Key,
					g => g.Sum(x => x.Amount)
				);


			var routeListNodesQuery = GetRouteListNodesQuery(uow, date);
			var routeListNodes = routeListNodesQuery.List<FastDeliveryVerificationDetailsNode>();

			var routeListIds = routeListNodes.Select(x => x.RouteList.Id).ToArray();
			var routeListFastDeliveriesCountQuery = GetRouteListFastDeliveriesCountQuery(uow, date, routeListIds);
			var routeListFastDeliveriesCount = routeListFastDeliveriesCountQuery.List<RouteListFastDeliveriesCountNode>();
			var rlsFastDeliveriesCount = routeListFastDeliveriesCount.ToDictionary(x => x.RouteListId);

			var addressesForFastDeliveryQuery = GetAddressesForFastDeliveryQuery(uow, routeListIds);
			var addressesForFastDelivery = addressesForFastDeliveryQuery.List<AddressInfoForFastDelivery>();
			var addressesLookup = addressesForFastDelivery.ToLookup(x => x.RouteListId);

			var freeBalancesQuery = GetFreeBalancesQuery(uow, neededNomenclatures, routeListIds);
			var freeBalances = freeBalancesQuery.List<RouteListNomenclatureAmount>();

			var i = 0;

			foreach(var node in routeListNodes)
			{
				UpdateDistanceAndLastCoordinateTimeParameters(node, latitude, longitude, trackPointTimeOffset);
				UpdateUnClosedFastDeliveriesParameter(node, rlsFastDeliveriesCount);
				UpdateRemainingTimeForShipmentNewOrder(
					node,
					addressesLookup,
					maxTimeForFastDelivery,
					minTimeForNewOrder,
					driverGoodWeightLiftPerHand,
					driverUnloadTime,
					maxTimeForFastDeliveryTimespan
				);
				UpdateRouteListBalanceParameter(node, freeBalances, neededNomenclatures);
			}

			routeListNodes = routeListNodes
				.OrderBy(x => isGetClosestByRoute 
					? x.DistanceByRoadToClient.ParameterValue 
					: x.DistanceByLineToClient.ParameterValue
				)
				.ToList();

			if(routeListNodes.Any())
			{
				fastDeliveryAvailabilityHistory.Items = fastDeliveryHistoryConverter
					.ConvertVerificationDetailsNodesToAvailabilityHistoryItems(
						routeListNodes,
						fastDeliveryAvailabilityHistory
					);
			}

			return fastDeliveryAvailabilityHistory;
		}

		private static IQueryOver<DeliveryFreeBalanceOperation, DeliveryFreeBalanceOperation> GetFreeBalancesQuery(
			IUnitOfWork uow,
			Dictionary<int, decimal> neededNomenclatures,
			int[] routeListIds
		)
		{
			RouteListNomenclatureAmount ordersAmountAlias = null;
			DeliveryFreeBalanceOperation deliveryFreeBalanceOperationAlias = null;

			var freeBalancesQuery = uow.Session.QueryOver(() => deliveryFreeBalanceOperationAlias)
				.WhereRestrictionOn(() => deliveryFreeBalanceOperationAlias.RouteList.Id).IsIn(routeListIds)
				.WhereRestrictionOn(() => deliveryFreeBalanceOperationAlias.Nomenclature.Id).IsIn(neededNomenclatures.Keys)
				.SelectList(list => list
					.SelectGroup(() => deliveryFreeBalanceOperationAlias.Nomenclature.Id).WithAlias(() => ordersAmountAlias.NomenclatureId)
					.SelectGroup(() => deliveryFreeBalanceOperationAlias.RouteList.Id).WithAlias(() => ordersAmountAlias.RouteListId)
					.SelectSum(() => deliveryFreeBalanceOperationAlias.Amount).WithAlias(() => ordersAmountAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<RouteListNomenclatureAmount>());
			return freeBalancesQuery;
		}

		private IQueryOver<RouteListItem, RouteListItem> GetAddressesForFastDeliveryQuery(
			IUnitOfWork uow,
			int[] routeListIds
		)
		{
			Order orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Nomenclature nomenclatureAlias = null;
			AddressInfoForFastDelivery addressInfoAlias = null;
			OrderItem orderItemAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var waterCount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.And(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var itemsSummaryWeight = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => nomenclatureAlias.TareVolume != TareVolume.Vol19L
					|| nomenclatureAlias.Category != NomenclatureCategory.water)
				.Select(Projections.Sum(() => nomenclatureAlias.Weight * orderItemAlias.Count));

			var equipmentsSummaryWeight = QueryOver.Of(() => orderEquipmentAlias)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderEquipmentAlias.Order.Id == orderAlias.Id)
				.And(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.Select(Projections.Sum(() => nomenclatureAlias.Weight * orderEquipmentAlias.Count));

			var addressesQuery = uow.Session.QueryOver<RouteListItem>()
				.JoinAlias(address => address.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
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
				.TransformUsing(Transformers.AliasToBean<AddressInfoForFastDelivery>());

			return addressesQuery;
		}

		private IQueryOver<RouteList, RouteList> GetRouteListFastDeliveriesCountQuery(
			IUnitOfWork uow,
			DateTime date,
			int[] routeListIds
		)
		{
			//Не более определённого кол-ва заказов с быстрой доставкой
			RouteListFastDeliveriesCountNode routeListFastDeliveriesCountAlias = null;
			RouteList routeListAlias = null;
			Order orderAlias = null;
			RouteListItem routeListItemAlias = null;

			var routeListMaxFastDeliveryOrdersCount = QueryOver.Of<RouteListMaxFastDeliveryOrders>()
				.Where(d =>
					d.RouteList.Id == routeListAlias.Id
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

			var fastDeliveryCountSubquery = QueryOver.Of(() => routeListItemAlias)
				.Inner.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Where(() => routeListItemAlias.RouteList.Id == routeListAlias.Id)
				.And(() => routeListItemAlias.Status == RouteListItemStatus.EnRoute)
				.And(() => orderAlias.IsFastDelivery)
				.Select(Projections.Count(() => routeListItemAlias.Id));

			var routeListFastDeliveriesCountQuery = uow.Session.QueryOver(() => routeListAlias)
				.WhereRestrictionOn(() => routeListAlias.Id).IsInG(routeListIds)
				.SelectList(list => list
					.Select(() => routeListAlias.Id).WithAlias(() => routeListFastDeliveriesCountAlias.RouteListId)
					.SelectSubQuery(fastDeliveryCountSubquery)
						.WithAlias(() => routeListFastDeliveriesCountAlias.UnclosedFastDeliveryAddresses)
					.Select(maxFastDeliveryOrdersCount)
						.WithAlias(() => routeListFastDeliveriesCountAlias.MaxFastDeliveryOrdersCount)
				)
				.TransformUsing(Transformers.AliasToBean<RouteListFastDeliveriesCountNode>());

			return routeListFastDeliveriesCountQuery;
		}

		private IQueryOver<RouteList, RouteList> GetRouteListNodesQuery(IUnitOfWork uow, DateTime date)
		{
			Track trackAlias = null;
			TrackPoint trackPointAlias = null;
			RouteList routeListAlias = null;
			TrackPoint innerTrackPointAlias = null;
			FastDeliveryVerificationDetailsNode resultAlias = null;
			Employee employeeAlias = null;

			var lastTimeTrackQuery = QueryOver.Of(() => innerTrackPointAlias)
				.Where(() => innerTrackPointAlias.Track.Id == trackAlias.Id)
				.Select(Projections.Max(() => innerTrackPointAlias.TimeStamp));

			var fastDeliveryMaxDistanceParameterVersion = QueryOver.Of<FastDeliveryMaxDistanceParameterVersion>()
				.Where(v => v.StartDate <= date
					&& (v.EndDate == null || v.EndDate > date))
				.Select(v => v.Value)
				.Take(1);

			var routeListFastDeliveryMaxDistance = QueryOver.Of<RouteListFastDeliveryMaxDistance>()
				.Where(d =>
					d.RouteList.Id == routeListAlias.Id
					&& d.StartDate <= date
					&& (d.EndDate == null || d.EndDate > date))
				.Select(d => d.Distance)
				.Take(1);

			//МЛ только в пути и с погруженным запасом
			var routeListNodesQuery = uow.Session.QueryOver(() => routeListAlias)
				.JoinEntityAlias(() => trackAlias, () => trackAlias.RouteList.Id == routeListAlias.Id)
				.Inner.JoinAlias(() => trackAlias.TrackPoints, () => trackPointAlias)
				.Inner.JoinAlias(() => routeListAlias.Driver, () => employeeAlias)
				.WithSubquery.WhereProperty(() => trackPointAlias.TimeStamp).Eq(lastTimeTrackQuery)
				.And(() => routeListAlias.Status == RouteListStatus.EnRoute)
				.And(() => routeListAlias.AdditionalLoadingDocument.Id != null) // только с погруженным запасом
				.SelectList(list => list
					.Select(() => trackPointAlias.TimeStamp).WithAlias(() => resultAlias.TimeStamp)
					.Select(() => trackPointAlias.Latitude).WithAlias(() => resultAlias.Latitude)
					.Select(() => trackPointAlias.Longitude).WithAlias(() => resultAlias.Longitude)
					.Select(Projections.Entity(() => routeListAlias)).WithAlias(() => resultAlias.RouteList)
					.Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
					.Select(Projections.SubQuery(routeListFastDeliveryMaxDistance))
						.WithAlias(() => resultAlias.RouteListFastDeliveryMaxDistance)
					.Select(Projections.SubQuery(fastDeliveryMaxDistanceParameterVersion))
						.WithAlias(() => resultAlias.FastDeliveryMaxDistanceParameterVersion)
				)
				.TransformUsing(Transformers.AliasToBean<FastDeliveryVerificationDetailsNode>());

			return routeListNodesQuery;
		}

		public async Task<FastDeliveryAvailabilityHistory> GetRouteListsForFastDeliveryAsync(
			IUnitOfWork uow,
			double latitude,
			double longitude,
			bool isGetClosestByRoute,
			IEnumerable<NomenclatureAmountNode> nomenclatureNodes,
			int? tariffZoneId,
			CancellationToken cancellationToken
		)
		{
			var date = DateTime.Now;

			var maxDistanceToTrackPoint = await GetGetMaxDistanceToLatestTrackPointKmAsync(uow, cancellationToken);

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
					fastDeliveryHistoryConverter.ConvertNomenclatureAmountNodesToOrderItemsHistory(
						nomenclatureNodes,
						fastDeliveryAvailabilityHistory
					);
			}

			var distributions = await uow.Session.QueryOver<AdditionalLoadingNomenclatureDistribution>()
				.ListAsync(cancellationToken);

			fastDeliveryAvailabilityHistory.NomenclatureDistributionHistoryItems =
				fastDeliveryHistoryConverter.ConvertNomenclatureDistributionToDistributionHistory(
					distributions,
					fastDeliveryAvailabilityHistory
				);

			TariffZone tariffZone;
			if(tariffZoneId.HasValue)
			{
				tariffZone = await uow.Session.GetAsync<TariffZone>(tariffZoneId.Value, cancellationToken);
			}
			else
			{
				var district = await GetDistrictAsync(uow, (decimal)latitude, (decimal)longitude, cancellationToken);
				tariffZone = district?.TariffZone;
			}

			if(tariffZone == null || !tariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				fastDeliveryAvailabilityHistory.AdditionalInformation =
					new List<string> { "Не найден район, у района отсутствует тарифная зона, " +
						"либо недоступна экспресс-доставка в текущее время." 
					};

				return fastDeliveryAvailabilityHistory;
			}

			var neededNomenclatures = nomenclatureNodes
				.GroupBy(x => x.NomenclatureId)
				.ToDictionary(
					g => g.Key,
					g => g.Sum(x => x.Amount)
				);

			var routeListNodesQuery = GetRouteListNodesQuery(uow, date);
			var routeListNodes = await routeListNodesQuery
				.ListAsync<FastDeliveryVerificationDetailsNode>(cancellationToken);

			var routeListIds = routeListNodes.Select(x => x.RouteList.Id).ToArray();
			var routeListFastDeliveriesCountQuery = GetRouteListFastDeliveriesCountQuery(uow, date, routeListIds);
			var routeListFastDeliveriesCount = await routeListFastDeliveriesCountQuery
				.ListAsync<RouteListFastDeliveriesCountNode>(cancellationToken);
			var rlsFastDeliveriesCount = routeListFastDeliveriesCount.ToDictionary(x => x.RouteListId);

			var addressesForFastDeliveryQuery = GetAddressesForFastDeliveryQuery(uow, routeListIds);
			var addressesForFastDelivery = await addressesForFastDeliveryQuery
				.ListAsync<AddressInfoForFastDelivery>(cancellationToken);
			var addressesLookup = addressesForFastDelivery.ToLookup(x => x.RouteListId);

			var freeBalancesQuery = GetFreeBalancesQuery(uow, neededNomenclatures, routeListIds);
			var freeBalances = await freeBalancesQuery
				.ListAsync<RouteListNomenclatureAmount>(cancellationToken);

			var i = 0;

			foreach(var node in routeListNodes)
			{
				await UpdateDistanceAndLastCoordinateTimeParametersAsync(
					node,
					latitude,
					longitude,
					trackPointTimeOffset,
					cancellationToken
				);
			}

			routeListNodes = routeListNodes
				.OrderBy(x => isGetClosestByRoute 
					? x.DistanceByRoadToClient.ParameterValue 
					: x.DistanceByLineToClient.ParameterValue
				)
				.ToList();

			foreach(var node in routeListNodes)
			{
				UpdateUnClosedFastDeliveriesParameter(node, rlsFastDeliveriesCount);
				UpdateRemainingTimeForShipmentNewOrder(
					node,
					addressesLookup,
					maxTimeForFastDelivery,
					minTimeForNewOrder,
					driverGoodWeightLiftPerHand,
					driverUnloadTime,
					maxTimeForFastDeliveryTimespan
				);

				UpdateRouteListBalanceParameter(node, freeBalances, neededNomenclatures);
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

			if(routeListNodes.Any())
			{
				fastDeliveryAvailabilityHistory.Items = fastDeliveryHistoryConverter
					.ConvertVerificationDetailsNodesToAvailabilityHistoryItems(
						routeListNodes,
						fastDeliveryAvailabilityHistory
					);
			}

			return fastDeliveryAvailabilityHistory;
		}

		private void UpdateDistanceAndLastCoordinateTimeParameters(
			FastDeliveryVerificationDetailsNode node,
			double latitude,
			double longitude,
			double trackPointTimeOffset
		)
		{
			var deliveryPointCoordinate = new PointOnEarth(latitude, longitude);
			var nodeCoordinate = new PointOnEarth(node.Latitude, node.Longitude);
			var points = new List<PointOnEarth> { nodeCoordinate, deliveryPointCoordinate };
			var excludeToll = _osrmSettings.ExcludeToll;
			var osrmResponse = _osrmClient.GetRoute(points, excludeToll: excludeToll);

			var proposedRoute = osrmResponse?.Routes?.FirstOrDefault();
			UpdateDistanceForNode(node, latitude, longitude, trackPointTimeOffset, proposedRoute);
		}

		private async Task UpdateDistanceAndLastCoordinateTimeParametersAsync(
			FastDeliveryVerificationDetailsNode node,
			double latitude,
			double longitude,
			double trackPointTimeOffset,
			CancellationToken cancellationToken
		)
		{
			var deliveryPointCoordinate = new PointOnEarth(latitude, longitude);
			var nodeCoordinate = new PointOnEarth(node.Latitude, node.Longitude);
			var points = new List<PointOnEarth> { nodeCoordinate, deliveryPointCoordinate };
			var excludeToll = _osrmSettings.ExcludeToll;
			var osrmResponse = await _osrmClient.GetRouteAsync(points, cancellationToken, excludeToll: excludeToll);

			var proposedRoute = osrmResponse?.Routes?.FirstOrDefault();
			UpdateDistanceForNode(node, latitude, longitude, trackPointTimeOffset, proposedRoute);
		}

		private void UpdateDistanceForNode(
			FastDeliveryVerificationDetailsNode node,
			double latitude,
			double longitude,
			double trackPointTimeOffset,
			Route proposedRoute
		)
		{
			var distance = DistanceHelper.GetDistanceKm(node.Latitude, node.Longitude, latitude, longitude);
			var routeDistance = (decimal)(proposedRoute?.TotalDistance ?? int.MaxValue) / 1000;
			node.DistanceByLineToClient.ParameterValue = (decimal)distance;
			node.DistanceByRoadToClient.ParameterValue = decimal.Round(routeDistance, 2);

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

		private void UpdateUnClosedFastDeliveriesParameter(
			FastDeliveryVerificationDetailsNode node,
			Dictionary<int, RouteListFastDeliveriesCountNode> rlsFastDeliveriesCount
		)
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
		private void UpdateRemainingTimeForShipmentNewOrder(
			FastDeliveryVerificationDetailsNode node,
			ILookup<int, AddressInfoForFastDelivery> addressesLookup,
			int maxTimeForFastDelivery,
			int minTimeForNewOrder,
			decimal driverGoodWeightLiftPerHand,
			decimal driverUnloadTime,
			TimeSpan maxTimeForFastDeliveryTimespan
			)
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
				latestAddress = orderedEnRouteAddresses
					.FirstOrDefault(x => x.IndexInRoute > latestCompletedAddress.IndexInRoute);
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
		private void UpdateRouteListBalanceParameter(
			FastDeliveryVerificationDetailsNode node,
			IEnumerable<RouteListNomenclatureAmount> freeBalances,
			Dictionary<int, decimal> neededNomenclatures
			)
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

		#endregion

		public double GetGetMaxDistanceToLatestTrackPointKm()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				return GetGetMaxDistanceToLatestTrackPointKm(uow);
			}
		}

		private double GetGetMaxDistanceToLatestTrackPointKm(IUnitOfWork uow)
		{
			var version = GetGetMaxDistanceToLatestTrackPointKmQuery(uow)
				.SingleOrDefault();
			return version.Value;
		}

		private async Task<double> GetGetMaxDistanceToLatestTrackPointKmAsync(
			IUnitOfWork uow,
			CancellationToken cancellationToken
			)
		{
			var version = await GetGetMaxDistanceToLatestTrackPointKmQuery(uow)
				.SingleOrDefaultAsync(cancellationToken);
			return version.Value;
		}

		private IQueryOver<FastDeliveryMaxDistanceParameterVersion> GetGetMaxDistanceToLatestTrackPointKmQuery(
			IUnitOfWork uow
		)
		{
			return uow.Session.QueryOver<FastDeliveryMaxDistanceParameterVersion>()
				.Where(x => x.EndDate == null);
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
