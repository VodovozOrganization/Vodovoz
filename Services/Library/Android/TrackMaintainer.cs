using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

namespace Android
{
	public  class TrackMaintainer
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		IUnitOfWorkGeneric<Track> uow;

		bool IsBusy = false;

		object BusyLock= new Object(); 

		public DateTime LastActive { get; private set;}

		public Track Track {get{ return uow.Root;}}

		public TrackMaintainer(int trackId)
		{
			uow = UnitOfWorkFactory.CreateForRoot<Track>(trackId, $"[ADS]Получение координат по треку {trackId}");
			LastActive = DateTime.Now;
		}

		public bool SaveNewCoordinates(TrackPointList trackPointList)
		{
			lock (BusyLock)
			{
				if (IsBusy)
				{
					logger.Warn("Пакет координат для трека {0} еще обрабатывается. Отменяем получение следующего пакета.");
					return false;
				}
				IsBusy = true;
			}
			var result = Save(trackPointList);
			lock(BusyLock)
			{
				IsBusy = false;
			}
			return result;
		}

		private bool Save(TrackPointList trackPointList)
		{
			LastActive = DateTime.Now;
			DateTime startOp = DateTime.Now;
			var DecimalSeparatorFormat = new NumberFormatInfo { NumberDecimalSeparator = ".", NumberGroupSeparator = "," };
			var CommaSeparatorFormat = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };

			//Подготовка полученных координат.
			var receivedPoints = new Dictionary<DateTime, Vodovoz.Domain.Logistic.TrackPoint>();
			foreach (TrackPoint tp in trackPointList)
			{
				var trackPoint = new Vodovoz.Domain.Logistic.TrackPoint();
				Double Latitude, Longitude;
				if (!Double.TryParse(tp.Latitude, NumberStyles.Float, DecimalSeparatorFormat, out Latitude)
					&& !Double.TryParse(tp.Latitude, NumberStyles.Float, CommaSeparatorFormat, out Latitude))
				{
					logger.Error("Не получилось разобрать координату широты: {0}", tp.Latitude);
					return false;
				}
				if (!Double.TryParse(tp.Longitude, NumberStyles.Float, DecimalSeparatorFormat, out Longitude)
					&& !Double.TryParse(tp.Longitude, NumberStyles.Float, CommaSeparatorFormat, out Longitude))
				{
					logger.Error("Не получилось разобрать координату долготы: {0}", tp.Longitude);
					return false;
				}
				trackPoint.Latitude = Latitude;
				trackPoint.Longitude = Longitude;
				trackPoint.TimeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(long.Parse(tp.TimeStamp)).ToLocalTime();
				//Округляем время до секунд.
				trackPoint.TimeStamp = new DateTime(trackPoint.TimeStamp.Year, trackPoint.TimeStamp.Month, trackPoint.TimeStamp.Day, trackPoint.TimeStamp.Hour, trackPoint.TimeStamp.Minute, trackPoint.TimeStamp.Second);

				if (receivedPoints.ContainsKey(trackPoint.TimeStamp))
				{
					logger.Warn("Для секунды {0} трека {1}, присутствует вторая пара координат, пропускаем что бы округлить трек с точностью не чаще раза в секунду.", trackPoint.TimeStamp, Track.Id);
					continue;
				}
				receivedPoints.Add(trackPoint.TimeStamp, trackPoint);
			}

			var minTime = receivedPoints.Keys.Min();

			//Проверяем не были ли координаты получены ранее.
			foreach(var existPoint in Track.TrackPoints.Where(p => p.TimeStamp >= minTime && receivedPoints.ContainsKey(p.TimeStamp)))
			{
				var trackPoint = receivedPoints[existPoint.TimeStamp];
				if(Math.Abs(existPoint.Latitude - trackPoint.Latitude) < 0.00000001 && Math.Abs(existPoint.Longitude - trackPoint.Longitude) < 0.00000001)
				{
					logger.Warn("Координаты на время {0} для трека {1}, были получены повторно поэтому пропущены.", trackPoint.TimeStamp, existPoint.Track.Id);
				}
				else
				{
					logger.Warn($"Координаты на время {trackPoint.TimeStamp} для трека {existPoint.Track.Id}, были получены повторно и изменены " +
						$"lat: {existPoint.Latitude} -> {trackPoint.Latitude} log: {existPoint.Longitude} -> {trackPoint.Longitude}");
					existPoint.Latitude = trackPoint.Latitude ;
					existPoint.Longitude = trackPoint.Longitude ;
				}
				receivedPoints.Remove(existPoint.TimeStamp);
			}

			//Записываем полученные координаты.
			foreach(var trackPoint in receivedPoints.Values)
			{
				trackPoint.Track = Track;
				Track.TrackPoints.Add(trackPoint);
			}

			uow.Save();
			logger.Info("Обработаны координаты для трека {0} за {1} сек.", Track.Id, (DateTime.Now - startOp).TotalSeconds);
			return true;
		}

		public void Close()
		{
			uow.Dispose();
		}
	}
}

