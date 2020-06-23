using System;
using System.Collections.Generic;
using System.Linq;

namespace Android
{
	public static class TracksService
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		static List<TrackMaintainer> Maintainers = new List<TrackMaintainer>();

		public static bool ReceivedCoordinates(int trackId, TrackPointList trackPointList)
		{
			TrackMaintainer worker;
			lock(Maintainers)
			{
				worker = Maintainers.FirstOrDefault(x => x.Track.Id == trackId);
				if(worker == null)
				{
					worker = new TrackMaintainer(trackId);
					Maintainers.Add(worker);
				}
			}

			try
			{
				return worker.SaveNewCoordinates(trackPointList);
			}
			catch (Exception e)
			{
				lock(Maintainers) 
				{
					worker.Close();
					Maintainers.Remove(worker); //Убираем поломаный воркер
				}
				logger.Error(e, "На обработке трека {0}", trackId);
				return false;
			}
		}

		public static void RemoveOldWorkers()
		{
			lock(Maintainers)
			{
				foreach(var worker in Maintainers.ToList())
				{
					if(worker.LastActive < DateTime.Now.AddMinutes(-5))
					{
						logger.Info("Удаляем worker для трека №{0}.", worker.Track.Id);
						worker.Close();
						Maintainers.Remove(worker);
					}
				}
			}
		}
	}
}

