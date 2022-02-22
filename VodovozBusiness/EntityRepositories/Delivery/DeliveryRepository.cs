using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NetTopologySuite.Geometries;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Utilities.Spatial;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;

namespace Vodovoz.EntityRepositories.Delivery
{
	public class DeliveryRepository : IDeliveryRepository
	{
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
			double m = 1 / (2 * pi / 360 * earth) / 1000;  //1 meter in degree

			double newLatitude = lat + (metersToAdd * m);

			return newLatitude;
		}

		private double GetNewLongitude(double lon, double metersToAdd)
		{
			double earth = 6378.137;  //radius of the earth in kilometer
			double pi = Math.PI;
			double m = 1 / (2 * pi / 360 * earth) / 1000;  //1 meter in degree

			double newLongitude = lon + metersToAdd * m / Math.Cos(lon * (pi / 180));
			return newLongitude;
		}

		#endregion Получение районов по координатам

		#region Доставка за час

		public bool OneHourDeliveryAvailable(IUnitOfWork uow, double latitude, double longitude,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider, IList<NomenclatureAmountNode> nomenclatureNodes)
		{
			var routeList = GetRouteListIdForOneHourDelivery(uow, latitude, longitude, deliveryRulesParametersProvider, nomenclatureNodes);
			return routeList != null;
		}

		public int? GetRouteListIdForOneHourDelivery(IUnitOfWork uow, double latitude, double longitude,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider, IList<NomenclatureAmountNode> nomenclatureNodes)
		{
			var latestTrackPointDateTime = DateTime.Now.Subtract(deliveryRulesParametersProvider.MaxTrackPointTimeOffsetForOneHourDelivery);
			var maxDistanceToTrackPoint = deliveryRulesParametersProvider.MaxDistanceToLatestTrackPointForOneHourDeliveryKm;
			var neededNomenclatures = nomenclatureNodes.ToDictionary(x => x.NomenclatureId, x => x.Amount);

			var rlIds = uow.Session.Query<RouteList>().Where(x => x.Status == RouteListStatus.EnRoute).Select(x => x.Id).ToList();

			Track t = null;
			TrackPoint tp = null;
			RouteListCoordinates routeListCoordinatesAlias = null;

			var allCoordinates = uow.Session.QueryOver<Track>(() => t)
				.Inner.JoinAlias(() => t.TrackPoints, () => tp)
				.WhereRestrictionOn(() => t.RouteList.Id).IsIn(rlIds)
				.And(() => tp.TimeStamp > latestTrackPointDateTime)
				.SelectList(list => list
					.Select(() => t.RouteList.Id).WithAlias(() => routeListCoordinatesAlias.RouteListId)
					.Select(() => tp.TimeStamp).WithAlias(() => routeListCoordinatesAlias.TimeStamp)
					.Select(() => tp.Latitude).WithAlias(() => routeListCoordinatesAlias.Latitude)
					.Select(() => tp.Longitude).WithAlias(() => routeListCoordinatesAlias.Longitude))
				.TransformUsing(Transformers.AliasToBean<RouteListCoordinates>())
				.List<RouteListCoordinates>();

			var latestRouteListCoordinates = allCoordinates
				.GroupBy(x => x.RouteListId)
				.Select(group => group.MaxBy(x => x.TimeStamp))
				.Where(x => DistanceHelper.GetDistanceKm(x.Latitude, x.Longitude, latitude, longitude) < maxDistanceToTrackPoint)
				.OrderBy(x => DistanceHelper.GetDistanceKm(x.Latitude, x.Longitude, latitude, longitude))
				.ToList();

			var latestRouteListIds = latestRouteListCoordinates.Select(x => x.RouteListId).ToArray();

			RouteListItem rla = null;
			Order o = null;
			OrderItem oi = null;
			OrderEquipment oe = null;
			CarLoadDocument scld = null;
			CarLoadDocumentItem scldi = null;

			RouteListNomenclatureAmount ordersAmountAlias = null;
			RouteListNomenclatureAmount loadDocumentsAmountAlias = null;

			//OrderItems
			var orderItemsNomenclatureAmount = uow.Session.QueryOver<RouteListItem>(() => rla)
				.Inner.JoinAlias(() => rla.Order, () => o)
				.Inner.JoinAlias(() => o.OrderItems, () => oi)
				.WhereRestrictionOn(() => rla.RouteList.Id).IsIn(latestRouteListIds)
				.WhereRestrictionOn(() => oi.Nomenclature.Id).IsIn(neededNomenclatures.Keys)
				.SelectList(list => list
					.SelectGroup(() => rla.RouteList.Id).WithAlias(() => ordersAmountAlias.RouteListId)
					.SelectGroup(() => oi.Nomenclature.Id).WithAlias(() => ordersAmountAlias.NomenclatureId)
					.SelectSum(() => oi.Count).WithAlias(() => ordersAmountAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<RouteListNomenclatureAmount>())
				.Future<RouteListNomenclatureAmount>();

			//OrderEquipments
			var orderEquipmentsNomenclatureAmount = uow.Session.QueryOver<RouteListItem>(() => rla)
				.Inner.JoinAlias(() => rla.Order, () => o)
				.Inner.JoinAlias(() => o.OrderEquipments, () => oe)
				.WhereRestrictionOn(() => rla.RouteList.Id).IsIn(latestRouteListIds)
				.WhereRestrictionOn(() => oe.Nomenclature.Id).IsIn(neededNomenclatures.Keys)
				.And(() => oe.Direction == Direction.Deliver)
				.SelectList(list => list
					.SelectGroup(() => rla.RouteList.Id).WithAlias(() => ordersAmountAlias.RouteListId)
					.SelectGroup(() => oe.Nomenclature.Id).WithAlias(() => ordersAmountAlias.NomenclatureId)
					.SelectSum(() => oe.Count).WithAlias(() => ordersAmountAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<RouteListNomenclatureAmount>())
				.Future<RouteListNomenclatureAmount>();

			//CarLoadDocuments
			var loadDocumentsNomenclatureAmount = uow.Session.QueryOver<CarLoadDocument>(() => scld)
				.Inner.JoinAlias(() => scld.Items, () => scldi)
				.WhereRestrictionOn(() => scld.RouteList.Id).IsIn(latestRouteListIds)
				.WhereRestrictionOn(() => scldi.Nomenclature.Id).IsIn(neededNomenclatures.Keys)
				.SelectList(list => list
					.SelectGroup(() => scld.RouteList.Id).WithAlias(() => loadDocumentsAmountAlias.RouteListId)
					.SelectGroup(() => scldi.Nomenclature.Id).WithAlias(() => loadDocumentsAmountAlias.NomenclatureId)
					.SelectSum(() => scldi.Amount).WithAlias(() => loadDocumentsAmountAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<RouteListNomenclatureAmount>())
				.Future<RouteListNomenclatureAmount>();

			var ordersNomenclautreAmount = orderItemsNomenclatureAmount
				.Union(orderEquipmentsNomenclatureAmount)
				.GroupBy(x => new { x.RouteListId, x.NomenclatureId }).Select(group => new RouteListNomenclatureAmount
				{
					RouteListId = group.Key.RouteListId,
					NomenclatureId = group.Key.NomenclatureId,
					Amount = group.Sum(x => x.Amount)
				})
				.ToList();

			foreach(var rlCoordinate in latestRouteListCoordinates)
			{
				var inOrdersNomenclatures = ordersNomenclautreAmount.Where(x => x.RouteListId == rlCoordinate.RouteListId).ToList();
				var inLoadDocs = loadDocumentsNomenclatureAmount.Where(x => x.RouteListId == rlCoordinate.RouteListId).ToList();
				bool isValidRl = true;

				foreach(var loaded in inLoadDocs)
				{
					var inOrders = inOrdersNomenclatures.FirstOrDefault(x => x.NomenclatureId == loaded.NomenclatureId);
					if(loaded.Amount - (inOrders?.Amount ?? 0) < neededNomenclatures[loaded.NomenclatureId])
					{
						isValidRl = false;
						break;
					}
				}

				if(isValidRl)
				{
					return rlCoordinate.RouteListId;
				}
			}

			return null;
		}

		private class RouteListCoordinates
		{
			public int RouteListId { get; set; }
			public DateTime TimeStamp { get; set; }
			public double Latitude { get; set; }
			public double Longitude { get; set; }
		}

		private class RouteListNomenclatureAmount
		{
			public int RouteListId { get; set; }
			public int NomenclatureId { get; set; }
			public decimal Amount { get; set; }
		}

		#endregion
	}
}
