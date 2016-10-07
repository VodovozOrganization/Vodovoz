using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using GMap.NET;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
	NominativePlural = "треки",
	Nominative = "трек")]
	public class Track : PropertyChangedBase, IDomainObject
	{
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
	}
}

