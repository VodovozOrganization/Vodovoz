﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using GMap.NET;
using QS.DomainModel.Entity;
using QS.Osrm;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "треки",
	Nominative = "трек")]
	public class Track : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IGlobalSettings _globalSettings = new GlobalSettings(new ParametersProvider());

		public virtual int Id { get; set; }

		Employee driver;

		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		RouteList routeList;

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField(ref routeList, value, () => RouteList); }
		}

		DateTime startDate;

		[Display(Name = "Дата и время начала")]
		public virtual DateTime StartDate {
			get { return startDate; }
			set { SetField(ref startDate, value, () => StartDate); }
		}

		IList<TrackPoint> trackPoints = new List<TrackPoint>();

		[Display(Name = "Точки трека")]
		public virtual IList<TrackPoint> TrackPoints {
			get { return trackPoints; }
			set {
				SetField(ref trackPoints, value, () => TrackPoints);
			}
		}

		private bool distanceEdited = false;

		[Display(Name = "Расстояние изменялось")]
		public virtual bool DistanceEdited {
			get { return distanceEdited; }
			set { SetField(ref distanceEdited, value, () => DistanceEdited); }
		}

		double? distance;

		[Display(Name = "Пройденное расстояние")]
		public virtual double? Distance {
			get { return distance; }
			set {
				SetField(ref distance, value, () => distance);
			}
		}

		private double? distanceToBase;

		[Display(Name = "Дистанция до базы")]
		public virtual double? DistanceToBase {
			get { return distanceToBase; }
			set { SetField(ref distanceToBase, value, () => DistanceToBase); }
		}

		public virtual double? TotalDistance {
			get {
				if(Distance == null && DistanceToBase == null)
					return null;
				return (Distance ?? 0) + (DistanceToBase ?? 0);
			}
		}

		GenericObservableList<TrackPoint> observableTrackPoints;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<TrackPoint> ObservableTrackPoints {
			get {
				if(observableTrackPoints == null) {
					observableTrackPoints = new GenericObservableList<TrackPoint>(TrackPoints);
				}
				return observableTrackPoints;
			}
		}

		public virtual void CalculateDistance()
		{
			if(TrackPoints.Count == 0) {
				Distance = null;
				return;
			}

			var points = TrackPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude));

			Distance = new MapRoute(points, "").Distance;
		}

		public virtual RouteResponse CalculateDistanceToBase()
		{
			var lastAddress = RouteList.Addresses
				.Where(x => x.Status == RouteListItemStatus.Completed)
				.OrderByDescending(x => x.StatusLastUpdate)
				.FirstOrDefault();

			if(lastAddress == null) {
				DistanceToBase = null;
				return null;
			}

			var points = new List<PointOnEarth>();
			var lastPoint = lastAddress.Order.DeliveryPoint;
			points.Add(new PointOnEarth(lastPoint.Latitude.Value, lastPoint.Longitude.Value));

			GeoGroupVersion geoGroupVersion = null;
			if(lastPoint.District != null)
			{
				geoGroupVersion = lastPoint.District.GeographicGroup.GetActualVersionOrNull();
			}

			if(lastPoint.District == null) {
				logger.Warn("Для точки доставки не удалось подобрать часть города. Расчёт расстояния до центра СПб");
				points.Add(new PointOnEarth(Constants.CenterOfCityLatitude, Constants.CenterOfCityLongitude));
			} else if(lastPoint.District != null && geoGroupVersion != null && geoGroupVersion.BaseCoordinatesExist) {
				points.Add(new PointOnEarth((double)geoGroupVersion.BaseLatitude.Value, (double)geoGroupVersion.BaseLongitude.Value));
			} else {
				logger.Error("В подобранной части города не указаны координаты базы");
				return null;
			}
			var response = OsrmClientFactory.Instance.GetRoute(points, false, GeometryOverview.Simplified, _globalSettings.ExcludeToll);
			if(response.Code == "Ok") {
				DistanceToBase = (double)response.Routes.First().TotalDistanceKm;
			} else
				logger.Error("Ошибка при получении расстояния до базы {0}: {1}", response.Code, response.Message);
			return response;
		}
	}
}

