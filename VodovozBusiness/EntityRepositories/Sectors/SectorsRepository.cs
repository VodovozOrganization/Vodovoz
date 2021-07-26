using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sectors;

namespace Vodovoz.EntityRepositories.Sectors
{
	public class SectorsRepository : ISectorsRepository
	{
		public IList<SectorVersion> GetSectorVersionInCoordinates(IUnitOfWork uow, decimal latitude, decimal longitude)
		{
			Point point = new Point((double)latitude, (double)longitude);

			Sector sectorAlias = null;
			SectorVersion sectorVersionAlias = null;
			
			IList<SectorVersion> sectorWithBorders = uow.Session.QueryOver<SectorVersion>(() => sectorVersionAlias)
				.Left.JoinAlias(() => sectorVersionAlias.Sector, () => sectorAlias)
				.Where(x => x.Polygon != null)
				.Where(() => sectorVersionAlias.Status == SectorsSetStatus.Active)
				.List<SectorVersion>();

			var sectorVersions = sectorWithBorders.Where(x => x.Polygon.Contains(point));

			if(sectorVersions.Any()) {
				return sectorVersions.ToList();
			}

			foreach(var nearPoint in Get4PointsInRadiusOfXMetersFromBasePoint(point)) {
				sectorVersions = sectorWithBorders.Where(x => x.Polygon.Contains(nearPoint));
				if(sectorVersions.Any()) {
					return sectorVersions.ToList();
				}
			}
			return new List<SectorVersion>();
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

		public IList<SectorVersion> GetSectorVersions(IUnitOfWork uow, DateTime? fromActivationDate, SectorsSetStatus? status)
		{
			var query = uow.Session.QueryOver<SectorVersion>();
			if(fromActivationDate.HasValue)
				query.Where(x => x.StartDate >= fromActivationDate);
			if(status.HasValue)
				query.And(x => x.Status == status);
			return query.List();
		}

		public IList<SectorDeliveryRuleVersion> GetSectorDeliveryRules(IUnitOfWork uow, Sector sector)
		{
			return uow.Session.QueryOver<SectorDeliveryRuleVersion>()
				.Where(x => x.Sector.Id == sector.Id).List();
		}

		public IList<SectorWeekDayRulesVersion> GetSectorWeekDayRules(IUnitOfWork uow, Sector sector)
		{
			return uow.Session.QueryOver<SectorWeekDayRulesVersion>()
				.Where(x => x.Sector.Id == sector.Id).List();
		}
		
		public IList<DeliveryPointSectorVersion> GetDeliveryPointSectorVersions(IUnitOfWork uow, Sector sector)
		{
			return uow.Session.QueryOver<DeliveryPointSectorVersion>()
				.Where(x => x.Sector.Id == sector.Id).List();
		}
	}
}
