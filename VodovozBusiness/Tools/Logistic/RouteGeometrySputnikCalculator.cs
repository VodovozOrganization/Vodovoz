﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GMap.NET;
using Polylines;
using QSOrmProject;
using QSOsm.Spuntik;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Tools.Logistic
{
	public class RouteGeometrySputnikCalculator : IDistanceCalculator
	{
		#region Настройки
		public static int DistanceFalsePenality = 100000;
		#endregion

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public static long BaseHash = CachedDistance.GetHash(Constants.BaseLatitude, Constants.BaseLongitude);

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		Dictionary<int, long[]> routeQueue = new Dictionary<int, long[]>();
		Dictionary<int, int> calculetedRoutes = new Dictionary<int, int>();
		Thread backgroundThread;

		public event EventHandler RouteCalculeted;

		int startCached, totalCached, addedCached, totalErrors;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		public RouteGeometrySputnikCalculator(WayHash[] ways)
		{
			var fromDB = Repository.Logistics.CachedDistanceRepository.GetCache(UoW, ways);
			startCached = fromDB.Count;
			foreach (var distance in fromDB)
			{
				AddNewCacheDistance(distance);
			}
		}

		public RouteGeometrySputnikCalculator()
		{
			
		}

		private void AddNewCacheDistance(CachedDistance distance)
		{
			if(!cache.ContainsKey(distance.FromGeoHash))
				cache[distance.FromGeoHash] = new Dictionary<long, CachedDistance>();
			cache[distance.FromGeoHash][distance.ToGeoHash] = distance;
			UoW.TrySave(distance);
			UoW.Commit();
			totalCached++;
		}

		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toHash = CachedDistance.GetHash(toDP);
			return DistanceMeter(fromHash, toHash);
		}

		public int DistanceFromBaseMeter(DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			return DistanceMeter(BaseHash, toHash);
		}

		public int DistanceToBaseMeter(DeliveryPoint fromDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			return DistanceMeter(fromHash, BaseHash);
		}

		private int DistanceMeter(long fromHash, long toHash)
		{
			var wayHash = new WayHash(fromHash, toHash);
			var way = GetCachedGeometry(wayHash, true);
			return way?.DistanceMeters ?? GetSimpleDistance(wayHash);
		}

		public List<WayHash> GenerateWaysOfRoute(long[] route)
		{
			List<WayHash> resultRoute = new List<WayHash>();
			for (int ix = 1; ix < route.Length; ix++)
			{
				resultRoute.Add(new WayHash(route[ix-1], route[ix]));
			}
			return resultRoute;
		}

		public List<WayHash> FilterForDBCheck(List<WayHash> list)
		{
			return list.Where(x => !(cache.ContainsKey(x.FromHash) && cache[x.FromHash].ContainsKey(x.ToHash))
			                  && !ErrorWays.Any(y => x.FromHash == y.FromHash && x.ToHash == y.ToHash)
			                 )
				       .ToList();
		}

		public int GetRouteDistance(long[] route)
		{
			var routeHash = String.Concat(route).GetHashCode();
			if (calculetedRoutes.ContainsKey(routeHash))
				return calculetedRoutes[routeHash];
			var ways = GenerateWaysOfRoute(route);
			LoadDBCacheIfNeed(ways);
			var distance = ways.Sum(x =>
			                GetCachedGeometry(x, false)?.DistanceMeters ?? GetSimpleDistance(x)
			               );
			if (distance > 0)
				calculetedRoutes.Add(routeHash, distance);
			return distance;
		}

		public int GetRouteDistanceBackground(long[] route)
		{
			var routeHash = String.Concat(route).GetHashCode();
			if (calculetedRoutes.ContainsKey(routeHash))
				return calculetedRoutes[routeHash];

			if (routeQueue.ContainsKey(routeHash))
				return -1; //В процессе обработки.

			routeQueue.Add(routeHash, route);

			if(backgroundThread == null)
			{
				backgroundThread = new Thread(delegate() {
					//Вызываем чисто чтобы посчитать.
					while(routeQueue.Count > 0)
					{
						var curPair = routeQueue.First();
						GetRouteDistance(curPair.Value);
						routeQueue.Remove(curPair.Key);
						Gtk.Application.Invoke(delegate {
							RouteCalculeted?.Invoke(this, EventArgs.Empty);
						});
					}
					backgroundThread = null;
				});
				backgroundThread.Start();
			}

			return -1;
		}

		private int GetSimpleDistance(WayHash way)
		{
			return (int)(GMap.NET.MapProviders.GMapProviders.EmptyProvider.Projection.GetDistance(
				CachedDistance.GetPointLatLng(way.FromHash),
				CachedDistance.GetPointLatLng(way.ToHash)
			) * 1000);
		}

		public void LoadDBCacheIfNeed(List<WayHash> ways)
		{
			var prepared = FilterForDBCheck(ways);
			if (prepared.Count > 0)
			{
				var fromDB = CachedDistanceRepository.GetCache(UoW, prepared.ToArray());
				foreach (var loaded in fromDB)
				{
					AddNewCacheDistance(loaded);
				}
			}

		}

		/// <param name="pulseProgress">делегат вызываемый для отображения прогресс, первый параметр текущее значение. Второй сколько всего.</param>
		public List<PointLatLng> GetGeometryOfRoute(long[] route, Action<uint, uint> pulseProgress)
		{
			//Запрашиваем кешь одним запросом для всего маршрута.
			LoadDBCacheIfNeed(GenerateWaysOfRoute(route));

			List<PointLatLng> resultRoute = new List<PointLatLng>();
			for (int ix = 1; ix < route.Length; ix++)
			{
				CachedDistance way = GetCachedGeometry(route[ix - 1], route[ix], false);

				if(way?.PolylineGeometry != null)
				{
					var decodedPoints = Polyline.DecodePolyline(way.PolylineGeometry);
					resultRoute.AddRange(decodedPoints.Select(p => new PointLatLng(p.Latitude * 0.1, p.Longitude * 0.1)));
				}
				else
				{
					double lat, lon;
					CachedDistance.GetLatLon(route[ix-1], out lat, out lon);
					resultRoute.Add(new PointLatLng(lat, lon));
					CachedDistance.GetLatLon(route[ix], out lat, out lon);
					resultRoute.Add(new PointLatLng(lat, lon));
				}

				if (pulseProgress != null)
					pulseProgress((uint)ix, (uint)route.Length);
			}

			return resultRoute;
		}

		CachedDistance GetCachedGeometry(WayHash way, bool checkDB = true)
		{
			return GetCachedGeometry(way.FromHash, way.ToHash, checkDB);
		}

		CachedDistance GetCachedGeometry(long fromP, long toP, bool checkDB = true)
		{
			CachedDistance distance = null;
			bool needAdd = false;
			//Проверяем в локальном кеше
			if (cache.ContainsKey(fromP) && cache[fromP].ContainsKey(toP))
			{
				distance = cache[fromP][toP];
			}
			//Проверяем в базе данных если разрешено.
			if(distance == null && checkDB)
			{
				var list = CachedDistanceRepository.GetCache(UoW, new[] { new WayHash(fromP, toP) });
				distance = list.FirstOrDefault();
			}
			//Не нашли создаем новый.
			if(distance == null)
			{
				distance = new CachedDistance();
				distance.FromGeoHash = fromP;
				distance.ToGeoHash = toP;
				needAdd = true;
			}
			if(distance.PolylineGeometry == null)
			{
				if (!UpdateFromSputnik(distance))
					return null;
			}

			if (distance.PolylineGeometry == null)
				return null;

			if (needAdd)
			{
				AddNewCacheDistance(distance);
				UoW.TrySave(distance);
				UoW.Commit();
			}

			return distance;
		}

		bool UpdateFromSputnik(CachedDistance distance)
		{
			if (ErrorWays.Any(x => x.FromHash == distance.FromGeoHash && x.ToHash == distance.ToGeoHash))
			{
				logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return false;
			}

			logger.Info("Запрашиваем путь {0}->{1} у спутника.", distance.FromGeoHash, distance.ToGeoHash);
			List<PointOnEarth> points = new List<PointOnEarth>();
			double latitude, longitude;
			CachedDistance.GetLatLon(distance.FromGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			CachedDistance.GetLatLon(distance.ToGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			var result = SputnikMain.GetRoute(points, false, true);
			if (result.Status == 0)
			{
				distance.Created = DateTime.Now;
				distance.DistanceMeters = result.RouteSummary.TotalDistance;
				distance.TravelTimeSec = result.RouteSummary.TotalTimeSeconds;
				distance.PolylineGeometry = result.RouteGeometry;
				AddNewCacheDistance(distance);
				UoW.TrySave(distance);
				UoW.Commit();
				addedCached++;
				return true;
			}
			ErrorWays.Add(new WayHash(distance.FromGeoHash, distance.ToGeoHash));
			totalErrors++;
			return false;
		}
	}
}
