using GMap.NET;
using QS.DomainModel.Entity;
using QS.Osrm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings.Common;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "треки",
		Nominative = "трек")]
	public class Track : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private Employee _driver;
		private RouteList _routeList;
		private DateTime _startDate;
		private bool _distanceEdited = false;
		private double? _distance;
		private double? _distanceToBase;
		private IList<TrackPoint> _trackPoints = new List<TrackPoint>();
		private GenericObservableList<TrackPoint> _observableTrackPoints;

		public virtual int Id { get; set; }

		[Display(Name = "Водитель")]
		public virtual Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Дата и время начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Точки трека")]
		public virtual IList<TrackPoint> TrackPoints
		{
			get => _trackPoints;
			set => SetField(ref _trackPoints, value);
		}

		[Display(Name = "Расстояние изменялось")]
		public virtual bool DistanceEdited
		{
			get => _distanceEdited;
			set => SetField(ref _distanceEdited, value);
		}

		[Display(Name = "Пройденное расстояние")]
		public virtual double? Distance
		{
			get => _distance;
			set => SetField(ref _distance, value);
		}

		[Display(Name = "Дистанция до базы")]
		public virtual double? DistanceToBase
		{
			get => _distanceToBase;
			set => SetField(ref _distanceToBase, value);
		}

		public virtual double? TotalDistance
		{
			get
			{
				if(Distance == null && DistanceToBase == null)
					return null;
				return (Distance ?? 0) + (DistanceToBase ?? 0);
			}
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<TrackPoint> ObservableTrackPoints
		{
			get
			{
				if(_observableTrackPoints == null)
				{
					_observableTrackPoints = new GenericObservableList<TrackPoint>(TrackPoints);
				}

				return _observableTrackPoints;
			}
		}

		public virtual void CalculateDistance()
		{
			if(TrackPoints.Count == 0)
			{
				Distance = null;
				return;
			}

			var points = TrackPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude));

			Distance = new MapRoute(points, "").Distance;
		}

		public virtual Result<RouteResponse> CalculateDistanceToBase(
			IOsrmSettings osrmSettings,
			IOsrmClient osrmClient
		)
		{
			var lastAddress = RouteList.Addresses
				.Where(x => x.Status == RouteListItemStatus.Completed)
				.OrderByDescending(x => x.StatusLastUpdate)
				.FirstOrDefault();

			if(lastAddress == null)
			{
				DistanceToBase = null;
				return Result.Failure<RouteResponse>(new Error("DistanceToBaseResponse", "Нет завершенных адресов, нечего рассчитывать!"));
			}

			var points = new List<PointOnEarth>();
			var lastPoint = lastAddress.Order.DeliveryPoint;
			points.Add(new PointOnEarth(lastPoint.Latitude.Value, lastPoint.Longitude.Value));

			GeoGroupVersion geoGroupVersion = null;
			if(lastPoint.District != null)
			{
				geoGroupVersion = lastPoint.District.GeographicGroup.GetActualVersionOrNull();
			}

			if(lastPoint.District == null)
			{
				_logger.Warn("Для точки доставки не удалось подобрать часть города. Расчёт расстояния до центра СПб");
				points.Add(new PointOnEarth(Constants.CenterOfCityLatitude, Constants.CenterOfCityLongitude));
			}
			else if(lastPoint.District != null && geoGroupVersion != null && geoGroupVersion.BaseCoordinatesExist)
			{
				points.Add(new PointOnEarth((double)geoGroupVersion.BaseLatitude.Value, (double)geoGroupVersion.BaseLongitude.Value));
			}
			else
			{
				var geoGroup = lastPoint.District.GeographicGroup;
				_logger.Error("В подобранной части города не указаны координаты базы");
				return Result.Failure<RouteResponse>(
					new Error(
						"DistanceToBaseResponse",
						$"В подобранной части города { geoGroup } не указаны координаты базы"));
			}

			var response = osrmClient.GetRoute(points, false, GeometryOverview.Simplified, osrmSettings.ExcludeToll);

			if(response is null || response.Code != "Ok")
			{
				if(response is null)
				{
					_logger.Error("Ошибка при получении расстояния до базы");
				}
				else
				{
					_logger.Error("Ошибка при получении расстояния до базы {0}: {1}", response.Code, response.Message);
				}
				
				return Result.Failure<RouteResponse>(
					new Error(
						"DistanceToBaseResponse",
						"Ошибка при получении расстояния до базы, попробуйте позднее"));
			}

			if(response.Code == "Ok")
			{
				DistanceToBase = (double)response.Routes.First().TotalDistanceKm;
			}

			return response;
		}
	}
}

