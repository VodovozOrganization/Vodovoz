using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Globalization;
using System.Linq;
using GMap.NET;
using QSOrmProject;
using RestSharp;
using Vodovoz.Domain.Employees;
using QSOsm.Spuntik;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
	NominativePlural = "треки",
	Nominative = "трек")]
	public class Track : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public virtual int Id { get; set; }

		Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField (ref driver, value, () => Driver); }
		}

		RouteList routeList;

		[Display (Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField (ref routeList, value, () => RouteList); }
		}

		DateTime startDate;

		[Display (Name = "Дата и время начала")]
		public virtual DateTime StartDate {
			get { return startDate; }
			set { SetField (ref startDate, value, () => StartDate); }
		}

		IList<TrackPoint> trackPoints = new List<TrackPoint> ();

		[Display (Name = "Точки трека")]
		public virtual IList<TrackPoint> TrackPoints {
			get { return trackPoints; }
			set { 
				SetField (ref trackPoints, value, () => TrackPoints); 
			}
		}

		double? distance;

		[Display (Name = "Пройденное расстояние")]
		public virtual double? Distance {
			get { return distance; }
			set { 
				SetField (ref distance, value, () => distance); 
			}
		}

		private double? distanceToBase;

		[Display (Name = "Дистанция до базы")]
		public virtual double? DistanceToBase {
		    get { return distanceToBase; }
		    set { SetField (ref distanceToBase, value, () => DistanceToBase); }
		}

		public virtual double? TotalDistance{
			get{
				return Distance + DistanceToBase;
			}
		}

		GenericObservableList<TrackPoint> observableTrackPoints;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<TrackPoint> ObservableTrackPoints {
			get {
				if (observableTrackPoints == null) {
					observableTrackPoints = new GenericObservableList<TrackPoint> (TrackPoints);
				}
				return observableTrackPoints;
			}
		}

		public virtual void CalculateDistance() {
			if (TrackPoints.Count == 0)
			{
				Distance = null;
				return;
			}

			var points = TrackPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude));

			Distance = new MapRoute(points, "").Distance;
		}

		public virtual SputnikRouteResponse CalculateDistanceToBase()
		{
			var client = new RestClient();
			client.BaseUrl = new Uri("http://routes.maps.sputnik.ru");

			var request = new RestRequest("osrm/router/viaroute", Method.GET);
			var lastAddress = RouteList.Addresses
				.Where(x => x.Status == RouteListItemStatus.Completed)
				.OrderByDescending(x => x.StatusLastUpdate)
				.FirstOrDefault();

			if(lastAddress == null)
			{
				DistanceToBase = null;
				return null;
			}

			var lastPoint = lastAddress.Order.DeliveryPoint;

			request.AddQueryParameter("loc", String.Format(CultureInfo.InvariantCulture,"{0},{1}", lastPoint.Latitude, lastPoint.Longitude));
			//Координаты базы
			request.AddQueryParameter("loc", String.Format("{0},{1}", "59.88632093834261","30.394406318664547"));
			request.AddQueryParameter("alt", "false");

			var response = client.Execute<SputnikRouteResponse>(request);
			if (response.Data.Status == 0)
			{
				DistanceToBase = (double)response.Data.RouteSummary.TotalDistanceKm;
			}
			else
				logger.Error("Ошибка при получении расстояния до базы {0}: {1}", response.Data.Status, response.Data.StatusMessage);
			return response.Data;
		}
	}
}

