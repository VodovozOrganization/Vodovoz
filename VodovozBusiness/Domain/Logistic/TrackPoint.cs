using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "точки трека",
		Nominative = "точка трека")]
	public class TrackPoint : PropertyChangedBase
	{
		Track track;

		[Display (Name = "Трек")]
		public virtual Track Track {
			get { return track; }
			set { SetField (ref track, value, () => Track); }
		}

		Double latitude;

		[Display (Name = "Широта")]
		public virtual Double Latitude {
			get { return latitude; }
			set { SetField (ref latitude, value, () => Latitude); }
		}

		Double longitude;

		[Display (Name = "Долгота")]
		public virtual Double Longitude {
			get { return longitude; }
			set { SetField (ref longitude, value, () => Longitude); }
		}
			
		DateTime timeStamp;

		[Display (Name = "Время")]
		public virtual DateTime TimeStamp {
			get { return timeStamp; }
			set { SetField (ref timeStamp, value, () => TimeStamp); }
		}

		public override bool Equals (object obj)
		{
			var tp = obj as TrackPoint;
			if (tp != null) {
				return DomainHelper.EqualDomainObjects (tp.Track, this.Track) && tp.TimeStamp == this.TimeStamp;
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return this.TimeStamp.GetHashCode () ^ this.Track.Id.GetHashCode ();
		}
	}
}
