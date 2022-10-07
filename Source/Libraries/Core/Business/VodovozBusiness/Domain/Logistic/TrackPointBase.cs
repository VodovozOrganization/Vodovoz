using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public abstract class TrackPointBase : PropertyChangedBase, IDomainObject
	{
		private Track _track;
		private double _latitude;
		private double _longitude;
		private DateTime _timeStamp;
		private DateTime _receiveTimeStamp;

		public virtual int Id { get; set; }

		[Display(Name = "Трек")]
		public virtual Track Track
		{
			get => _track;
			set => SetField(ref _track, value, () => Track);
		}

		[Display(Name = "Широта")]
		public virtual double Latitude
		{
			get => _latitude;
			set => SetField(ref _latitude, value, () => Latitude);
		}

		[Display(Name = "Долгота")]
		public virtual double Longitude
		{
			get => _longitude;
			set => SetField(ref _longitude, value, () => Longitude);
		}

		[Display(Name = "Время")]
		public virtual DateTime TimeStamp
		{
			get => _timeStamp;
			set => SetField(ref _timeStamp, value, () => TimeStamp);
		}

		[Display(Name = "Время получения координаты")]
		public virtual DateTime ReceiveTimeStamp
		{
			get => _receiveTimeStamp;
			set => SetField(ref _receiveTimeStamp, value);
		}
		public override bool Equals(object obj)
		{
			if(obj is TrackPoint tp)
			{
				return DomainHelper.EqualDomainObjects(tp.Track, Track) && tp.TimeStamp == TimeStamp;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return TimeStamp.GetHashCode() ^ Track.Id.GetHashCode();
		}
	}
}
