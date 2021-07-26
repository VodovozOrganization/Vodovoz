using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Osrm;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Sectors;

namespace Vodovoz.Domain.Sectors
{
	public class DeliveryPointSectorVersion : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		public int Id { get; }

		private DateTime _startDate;

		[Display(Name = "Время создания")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		private DateTime? _endDate;

		[Display(Name = "Время закрытия")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		private Sector _sector;

		[Display(Name = "Район доставки")]
		public virtual Sector Sector
		{
			get => _sector;
			set => SetField(ref _sector, value);
		}

		private DeliveryPoint _deliveryPoint;

		public DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}

		private decimal? _latitude;

		/// <summary>
		/// Широта. Для установки координат используйте метод SetСoordinates
		/// </summary>
		[Display(Name = "Широта")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? Latitude
		{
			get => _latitude;
			protected set => SetField(ref _latitude, value);
		}

		private decimal? _longitude;

		/// <summary>
		/// Долгота. Для установки координат используйте метод SetСoordinates
		/// </summary>
		[Display(Name = "Долгота")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? Longitude
		{
			get => _longitude;
			protected set => SetField(ref _longitude, value);
		}

		private int? _distanceFromBaseMeters;

		[Display(Name = "Расстояние от базы в метрах")]
		public virtual int? DistanceFromBaseMeters
		{
			get => _distanceFromBaseMeters;
			set => SetField(ref _distanceFromBaseMeters, value);
		}

		private SectorsSetStatus _status;

		public virtual SectorsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public DeliveryPointSectorVersion()
		{
			Status = SectorsSetStatus.Active;
		}

		#region Расчетные

		public virtual string CoordinatesText
		{
			get
			{
				if(Latitude == null || Longitude == null)
					return string.Empty;
				return string.Format("(ш. {0:F5}, д. {1:F5})", Latitude, Longitude);
			}
		}

		public virtual bool CoordinatesExist => Latitude.HasValue && Longitude.HasValue;

		public virtual Point NetTopologyPoint => CoordinatesExist ? new Point((double) Latitude, (double) Longitude) : null;

		public virtual PointOnEarth PointOnEarth => new PointOnEarth(Latitude.Value, Longitude.Value);

		public virtual GMap.NET.PointLatLng GmapPoint => new GMap.NET.PointLatLng((double) Latitude, (double) Longitude);

		#endregion


		/// <summary>
		/// Поиск района города, в котором находится текущая точка доставки
		/// </summary>
		/// <returns><c>true</c>, если район города найден</returns>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города</param>
		/// <param name="sectorsRepository">Репозиторий где достается сектор по координатам</param>
		public bool FindAndAssociateDistrict(IUnitOfWork uow, ISectorsRepository sectorsRepository)
		{
			if(!CoordinatesExist)
			{
				return false;
			}

			SectorVersion foundSectorVersion =
				sectorsRepository.GetSectorVersionInCoordinates(uow, Latitude.Value, Longitude.Value).FirstOrDefault();
			if(foundSectorVersion == null)
			{
				return false;
			}

			Sector = foundSectorVersion.Sector;
			return true;
		}


		public virtual IList<SectorVersion> CalculateDistricts(IUnitOfWork uow, ISectorsRepository sectorsRepository)
		{
			if(!CoordinatesExist)
			{
				return new List<SectorVersion>();
			}

			return sectorsRepository.GetSectorVersionInCoordinates(uow, Latitude.Value, Longitude.Value);
		}

		/// <summary>
		/// Устанавливает правильно координаты точки.
		/// </summary>
		/// <returns><c>true</c>, если координаты установлены</returns>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города
		/// для определения расстояния до базы</param>
		public virtual bool SetСoordinates(decimal? latitude, decimal? longitude, ISectorsRepository sectorsRepository, IUnitOfWork uow = null)
		{
			Latitude = latitude;
			Longitude = longitude;

			OnPropertyChanged(nameof(CoordinatesExist));

			if(Longitude == null || Latitude == null || !FindAndAssociateDistrict(uow, sectorsRepository))
				return true;
			var gg = Sector.ActiveSectorVersion.GeographicGroup;
			var route = new List<PointOnEarth>(2)
			{
				new PointOnEarth(gg.BaseLatitude.Value, gg.BaseLongitude.Value),
				new PointOnEarth(Latitude.Value, Longitude.Value)
			};

			var result = OsrmMain.GetRoute(route, false, GeometryOverview.False);
			if(result == null)
			{
				_logger.Error("Сервер расчета расстояний не вернул ответа.");
				return false;
			}

			if(result.Code != "Ok")
			{
				_logger.Error("Сервер расчета расстояний вернул следующее сообщение:\n" + result.StatusMessageRus);
				return false;
			}

			DistanceFromBaseMeters = result.Routes[0].TotalDistance;
			return true;
		}
	}
}