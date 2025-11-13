using Autofac;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Microsoft.Extensions.Logging;
using Polylines;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Osrm;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Settings.Common;

namespace Dialogs.Logistic
{
	public partial class TrackOnMapWnd : Gtk.Window
	{
		private static ILogger<TrackOnMapWnd> _logger = ScopeProvider.Scope.Resolve<ILogger<TrackOnMapWnd>>();
		private readonly ITrackRepository _trackRepository = ScopeProvider.Scope.Resolve<ITrackRepository>();
		private readonly IOsrmSettings _osrmSettings = ScopeProvider.Scope.Resolve<IOsrmSettings>();
		private readonly IOsrmClient _osrmClient = ScopeProvider.Scope.Resolve<IOsrmClient>();

		#region Поля

		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private readonly GMapOverlay trackToBaseOverlay = new GMapOverlay("track_to_base");
		private readonly GMapOverlay trackOnGapOverlay = new GMapOverlay ("track_on_gap");
		private readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();

		RouteList routeList = null;
		Track track;
		GMapRoute trackRoute;
		IUnitOfWork UoW;

		#endregion

		/// <summary>
		/// Создание диалога для трека с используемой в другом диалоге сессией 
		/// </summary>
		public TrackOnMapWnd(IUnitOfWorkGeneric<RouteList> uow) : base(Gtk.WindowType.Toplevel)
		{
			this.Build();

			UoW = uow;
			routeList = uow.Root;

			Configure();
		}

		/// <summary>
		/// Создание диалога для трека с новой сессией
		/// НЕ ИСПОЛЬЗОВАТЬ если создается из диалога МЛ.
		/// </summary>
		public TrackOnMapWnd(int routeListId) : base(Gtk.WindowType.Toplevel)
		{
			this.Build();

			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			this.routeList = UoW.Session.QueryOver<RouteList>()
				.Where(rl => rl.Id == routeListId).SingleOrDefault();
			if (routeList == null)
				return;
			
			Configure();
		}

		private void Configure()
		{
			track = _trackRepository.GetTrackByRouteListId(UoW, routeList.Id);
			if(track == null){
				buttonRecalculateToBase.Sensitive = buttonFindGap.Sensitive = buttonCutTrack.Sensitive = buttonLastAddress.Sensitive = false;
				MessageDialogHelper.RunInfoDialog($"Маршрутный лист №{routeList.Id}\nТрек не обнаружен");
			}else if(routeList.Status < RouteListStatus.OnClosing)
				buttonRecalculateToBase.Sensitive = buttonFindGap.Sensitive = buttonCutTrack.Sensitive = buttonLastAddress.Sensitive = false;

			ConfigureMap();
			OpenMap();
			UpdateDistanceLabel();
		}

		private void ConfigureMap()
		{
			gmapWidget.MapProvider = GMapProviders.GoogleMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.MinZoom = 0;
			gmapWidget.MaxZoom = 24;
			gmapWidget.Zoom = 9;
			gmapWidget.MouseWheelZoomEnabled = true;
			gmapWidget.Overlays.Add(tracksOverlay);
			gmapWidget.Overlays.Add(trackToBaseOverlay);
			gmapWidget.Overlays.Add (trackOnGapOverlay);
			gmapWidget.Overlays.Add(addressesOverlay);
			gmapWidget.ExposeEvent += GmapWidget_ExposeEvent;
			gmapWidget.MotionNotifyEvent += GmapWidget_MotionNotifyEvent;

			comboMapProvider.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			comboMapProvider.ItemsEnum = typeof(MapProviders);
			comboMapProvider.SelectedItem = MapProviders.GoogleMap;
			comboMapProvider.EnumItemSelected += (sender, args) =>
				gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);

		}

		private void OpenMap()
		{
			ClearTracks();

			this.Title = $"Трек маршрутного листа №{routeList.Id}";
			this.SetDefaultSize(700, 600);
			this.DeleteEvent += MapWindow_DeleteEvent;
			radioNumbers.Active = true;

			LoadTrack();
			LoadAddresses();

			gmapWidget.Show();
		}

		private void ClearTracks()
		{
			tracksOverlay.Clear();
			tracksDistance.Clear();
		}

		void MapWindow_DeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			args.RetVal = false;
		}

		private void LoadTrack(List<PointOnEarth> pointsRecalculateMileage = null)
		{
			if (track == null && pointsRecalculateMileage == null)
				return;

			var points = new List<PointLatLng>();

			if(pointsRecalculateMileage == null)
				points = track.TrackPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude)).ToList();
			 else
				points = pointsRecalculateMileage.Select(p => new PointLatLng(p.Latitude, p.Longitude)).ToList();

			trackRoute = new GMapRoute(points, routeList.Id.ToString()) {
				Stroke = new Pen(Color.Red) {
					Width = 4,
					DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
				},
				IsHitTestVisible = true
			};

			tracksDistance.Add(MakeDistanceLayout(trackRoute));
			tracksOverlay.Routes.Add(trackRoute);
		}

		void UpdateDistanceLabel()
		{
			if(track == null)
			{
				labelDistance.LabelProp = String.Empty;
				return;
			}

			var text = new List<string>();
			if (track.Distance.HasValue)
				text.Add(String.Format("Дистанция трека: {0:N1} км.{1}", track.Distance, track.DistanceEdited ? "(пересчитанная)" : ""));
			if(track.DistanceToBase.HasValue)
				text.Add(String.Format("Дистанция до базы: {0:N1} км.", track.DistanceToBase));
			if (track.DistanceToBase.HasValue && track.Distance.HasValue)
				text.Add(String.Format("Общее расстояние: {0:N1} км.", track.TotalDistance));

			labelDistance.LabelProp = String.Join("\n", text);
		}

		private void LoadAddresses(bool recalculateMileage = false)
		{
			Console.WriteLine("Загружаем адреса");
			addressesOverlay.Clear();

			foreach(var orderItem in (recalculateMileage ? routeList.Addresses.OrderBy(x => x.StatusLastUpdate).ToList() : routeList.Addresses))
			{
				var point = orderItem.Order.DeliveryPoint;
				if (point == null)
					continue;
				if(point.Latitude.HasValue && point.Longitude.HasValue)
				{
					GMarkerGoogleType type;
					switch(orderItem.Status)
					{
						case RouteListItemStatus.EnRoute:
							type = GMarkerGoogleType.blue_small;
							break;
						case RouteListItemStatus.Completed:
							type = GMarkerGoogleType.green_small;
							break;
						case RouteListItemStatus.Canceled:
							type = GMarkerGoogleType.purple_small;
							break;
						case RouteListItemStatus.Overdue:
							type = GMarkerGoogleType.red_small;
							break;
						case RouteListItemStatus.Transfered:
							type = GMarkerGoogleType.gray_small;
							break;
						default:
							continue;
					}
					GMapMarker addressMarker;
					if(radioNumbers.Active && type == GMarkerGoogleType.green_small)
					{
						int index = routeList.Addresses
							.Where(x => x.Status == RouteListItemStatus.Completed)
							.OrderBy(x => x.StatusLastUpdate)
							.ToList().IndexOf(orderItem);

						addressMarker = new NumericPointMarker(new PointLatLng((double)point.Latitude, (double)point.Longitude),
							NumericPointMarkerType.green_large, index + 1);
					}
					else
						addressMarker = new GMarkerGoogle(new PointLatLng((double)point.Latitude, (double)point.Longitude),	type);

					var identicalPoint = addressesOverlay.Markers.Count(g => g.Position.Lat == (double)point.Latitude && g.Position.Lng == (double)point.Longitude);
					var pointShift = 4;
					if(identicalPoint > 0)
					{
						addressMarker.Offset = new Point(addressMarker.Offset.X + (int)(Math.Pow(-1, (identicalPoint - 1) / 2)) * ((identicalPoint - 1) / 4 + 1) * pointShift, addressMarker.Offset.Y + (int)(Math.Pow(-1, identicalPoint/ 2)) * ((identicalPoint - 1) / 4 + 1) * pointShift);
					}


					var text = point.ShortAddress;
					if (orderItem.StatusLastUpdate.HasValue)
						text += string.Format("\nСтатус изменялся в {0:t}", orderItem.StatusLastUpdate.Value);
					addressMarker.ToolTipText = text;
					addressesOverlay.Markers.Add(addressMarker);
				}
			}
		}

		private DistanceTextInfo MakeDistanceLayout(params GMapRoute[] route)
		{
			var layout = new Pango.Layout(this.PangoContext) {
				Alignment = Pango.Alignment.Right
			};
			var colTXT = route[0].Stroke.Color.ToHtmlColor();
			layout.SetMarkup(string.Format("<span foreground=\"{1}\"><span font=\"Segoe UI Symbol\">⛽</span> {0:N1} км.</span>", route.Sum(x =>x.Distance), colTXT));

			return new DistanceTextInfo{
				PangoLayout = layout
			};
		}

		void GmapWidget_ExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			if (tracksDistance.Count == 0)
				return;
			var g = args.Event.Window;
			var area = args.Event.Area;
			int voffset = 0;
			var gc = gmapWidget.Style.TextGC(Gtk.StateType.Normal);

			foreach(var distance in tracksDistance)
			{
				distance.PangoLayout.GetPixelSize(out int layoutWidth, out int layoutHeight);
				g.DrawLayout(gc, area.Right - 6 - layoutWidth, area.Top + 6 + voffset, distance.PangoLayout);
				voffset += 3 + layoutHeight;
			}
		}

		protected void OnRadioSmallClicked(object sender, EventArgs e)
		{
			if(radioSmall.Active)
				LoadAddresses();
		}

		protected void OnRadioNumbersClicked(object sender, EventArgs e)
		{
			if(radioNumbers.Active)
				LoadAddresses();
		}

		protected void OnButtonRecalculateToBaseClicked(object sender, EventArgs e)
		{
			var curTrack = _trackRepository.GetTrackByRouteListId(UoW, routeList.Id);
			var response = curTrack.CalculateDistanceToBase(_osrmSettings, _osrmClient);
			UoW.Save(curTrack);
			UoW.Commit();
			UpdateDistanceLabel();

			trackToBaseOverlay.Clear();
			var decodedPoints = Polyline.DecodePolyline(response.Routes.First().RouteGeometry);
			var points = decodedPoints.Select(p => new PointLatLng(p.Latitude * 0.1, p.Longitude * 0.1)).ToList();

			var route = new GMapRoute(points, "RouteToBase") {
				Stroke = new Pen(Color.Blue) {
					Width = 4,
					DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
				}
			};

			tracksDistance.Add(MakeDistanceLayout(route));
			trackToBaseOverlay.Routes.Add(route);

			buttonRecalculateToBase.Sensitive = false;

			MessageDialogHelper.RunInfoDialog(string.Format("Расстояние от {0} до склада {1} км. Время в пути {2}.",
				response.Waypoints.First().Name,
				response.Routes.First().TotalDistanceKm,
				response.Routes.First().TotalTime
			));
		}

		protected void OnButtonFindGapClicked (object sender, EventArgs e)
		{
			trackOnGapOverlay.Clear ();
			tracksDistance.RemoveAll (x => x.Id == "MissingTrack");
			string message = "Найдены разрывы в треке:";
			double replacedDistance = 0;

			TrackPoint lastPoint = null;
			foreach(var point in track.TrackPoints)
			{
				if(lastPoint == null)
				{
					lastPoint = point;
					continue;
				}

				var distance = GMapProviders.EmptyProvider.Projection.GetDistance (
					new PointLatLng (lastPoint.Latitude, lastPoint.Longitude), 
					new PointLatLng (point.Latitude, point.Longitude));
				
				if(distance > 0.5)
				{
					_logger.LogInformation("Найден разрыв в треке расстоянием в {Distance}", distance);
					message += string.Format ("\n* разрыв c {1:t} по {2:t} — {0:N1} км.",
					                          distance,
					                          lastPoint.TimeStamp,
					                          point.TimeStamp
					                         );
					replacedDistance += distance;

					var addressesByCompletion = routeList.Addresses
							.Where (x => x.Status == RouteListItemStatus.Completed)
							.OrderBy (x => x.StatusLastUpdate)
							.ToList ();

					RouteListItem addressBeforeGap = addressesByCompletion.LastOrDefault (x => x.StatusLastUpdate < lastPoint.TimeStamp.AddMinutes (2));
					RouteListItem addressAfterGap = addressesByCompletion.FirstOrDefault (x => x.StatusLastUpdate > point.TimeStamp.AddMinutes (-2));

					var beforeIndex = addressBeforeGap == null ? -1 : addressesByCompletion.IndexOf (addressBeforeGap);
					var afterIndex = addressAfterGap == null ? addressesByCompletion.Count : addressesByCompletion.IndexOf (addressAfterGap);
					var routePoints = new List<PointOnEarth> {
						new PointOnEarth(lastPoint.Latitude, lastPoint.Longitude)
					};

					if(afterIndex - beforeIndex > 1)
					{
						var throughAddress = addressesByCompletion.GetRange (beforeIndex + 1, afterIndex - beforeIndex - 1);
						_logger.LogInformation("В разрыве найдены выполенные адреса порядковый(е) номер(а) {IndexesInRoute}", String.Join (", ", throughAddress.Select (x => x.IndexInRoute)));
						routePoints.AddRange (
							throughAddress.Where (x => x.Order?.DeliveryPoint?.Latitude != null && x.Order?.DeliveryPoint?.Longitude != null)
							.Select (x => new PointOnEarth (x.Order.DeliveryPoint.Latitude.Value, x.Order.DeliveryPoint.Longitude.Value)));

						message += $" c выполненными адресами({beforeIndex + 2}-{afterIndex}) п/п в МЛ " + String.Join (", ", throughAddress.Select (x => x.IndexInRoute));
					}
					routePoints.Add (new PointOnEarth (point.Latitude, point.Longitude));

					var missedTrack = _osrmClient.GetRoute(routePoints, false, GeometryOverview.Simplified, _osrmSettings.ExcludeToll);
					if (missedTrack == null)
					{
						MessageDialogHelper.RunErrorDialog ("Не удалось получить ответ от сервиса \"Спутник\"");
						return;
					}
					if(missedTrack.Code != "Ok")
					{
						MessageDialogHelper.RunErrorDialog ("Cервис \"Спутник\" сообщил об ошибке {0}: {1}", missedTrack.Code, missedTrack.StatusMessageRus);
						return;
					}

					var decodedPoints = Polyline.DecodePolyline (missedTrack.Routes.First().RouteGeometry);
					var points = decodedPoints.Select (p => new PointLatLng (p.Latitude * 0.1, p.Longitude * 0.1)).ToList ();

					var route = new GMapRoute(points, "MissedRoute") {
						Stroke = new Pen(Color.DarkMagenta) {
							Width = 4,
							DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
						}
					};

					trackOnGapOverlay.Routes.Add (route);
				}

				lastPoint = point;
			}

			if(trackOnGapOverlay.Routes.Count > 0)
			{
				var distanceLayout = MakeDistanceLayout (trackOnGapOverlay.Routes.ToArray ());
				distanceLayout.Id = "MissingTrack";
				tracksDistance.Add (distanceLayout);
				var oldDistance = track.DistanceEdited ? trackRoute.Distance : track.Distance;
				var missedRouteDistance = trackOnGapOverlay.Routes.Sum (x => x.Distance);
				var newDistance = oldDistance - replacedDistance + missedRouteDistance;
				var diffDistance = newDistance - oldDistance;

				message += $"\n Старая длинна трека:{oldDistance:N1} км." +
					$"\n Новая длинна трека: {newDistance:N1} км.(+{diffDistance:N1})" +
					"\n Сохранить изменения длинны трека?";

				if(MessageDialogHelper.RunQuestionDialog(message))
				{
					track.Distance = newDistance;
					track.DistanceEdited = true;
					UoW.Save (track);
					UoW.Commit ();
					UpdateDistanceLabel ();
				}
			}
			else
			{
				MessageDialogHelper.RunInfoDialog ("Разрывов в треке не найдено.");
			}
		}

		void GmapWidget_MotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			if(gmapWidget.IsMouseOverRoute)
			{
				GPoint tl = new GPoint ((long)args.Event.X - 4, (long)args.Event.Y - 4);
				var TLPoint = gmapWidget.FromLocalToLatLng (tl);
				GPoint br = new GPoint ((long)args.Event.X + 4, (long)args.Event.Y + 4);
				var BRPoint = gmapWidget.FromLocalToLatLng (br);

				GPoint mouse = new GPoint ((long)args.Event.X, (long)args.Event.Y);
				var geopoint = gmapWidget.FromLocalToLatLng (mouse);

				var rect = RectLatLng.FromLTRB (TLPoint.Lng, TLPoint.Lat, BRPoint.Lng, BRPoint.Lat);

				var nearest = track.TrackPoints
				                   .Where(x => rect.Contains(x.Latitude, x.Longitude))
				                   .OrderBy (x => GMapProviders.EmptyProvider.Projection.GetDistance (geopoint, new PointLatLng (x.Latitude, x.Longitude)))
				                   .FirstOrDefault();
				if(nearest != null)
					ylabelPoint.LabelProp = String.Format ("(ш.{0:F6} д.{1:F6}) - {2:T}", nearest.Latitude, nearest.Longitude, nearest.TimeStamp);
				else
					ylabelPoint.LabelProp = String.Empty;
			}
			else
				ylabelPoint.LabelProp = String.Empty;

		}

		protected void OnButtonCutTrackClicked(object sender, EventArgs e)
		{
			
			IEnumerable<PointLatLng> midlPoints;
			var curTrack = _trackRepository.GetTrackByRouteListId(UoW, routeList.Id);

			tracksOverlay.Clear();

			var startDateTime = ydatepickerStart.Date;
			var endDateTime = ydatepickerEnd.Date;

			var startPoints = curTrack.TrackPoints.Where(x => x.TimeStamp < startDateTime).Select(p => new PointLatLng(p.Latitude, p.Longitude));
			var endPoints = curTrack.TrackPoints.Where(x => x.TimeStamp >= endDateTime).Select(p => new PointLatLng(p.Latitude, p.Longitude));

			if(!ydatepickerStart.IsEmpty) {

				midlPoints = curTrack.TrackPoints.Where(x => x.TimeStamp >= startDateTime && x.TimeStamp < endDateTime).Select(p => new PointLatLng(p.Latitude, p.Longitude));

				trackRoute = new GMapRoute(startPoints, routeList.Id.ToString()) {
					Stroke = new Pen(Color.Gray) {
						Width = 4,
						DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot
					},
					IsHitTestVisible = true
				};
				tracksOverlay.Routes.Add(trackRoute);
 
			} else
			{
				midlPoints = curTrack.TrackPoints.Where(x => x.TimeStamp < endDateTime).Select(p => new PointLatLng(p.Latitude, p.Longitude));
			}

			trackRoute = new GMapRoute(midlPoints, routeList.Id.ToString()) {
				Stroke = new Pen(Color.Red) {
					Width = 4,
					DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
				},
				IsHitTestVisible = true
			};

			labelCutTrack.Text = String.Format("Обработанный\nтрек: {0:N1} км.", Math.Round(trackRoute.Distance, 2)) ;
			 
			tracksOverlay.Routes.Add(trackRoute);

			trackRoute = new GMapRoute(endPoints, routeList.Id.ToString()) {
				Stroke = new Pen(Color.Gray) {
					Width = 4,
					DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot
				},
				IsHitTestVisible = true
			};

			tracksOverlay.Routes.Add(trackRoute);
		

		}

		protected void OnButtonLastAddressClicked(object sender, EventArgs e)
		{
 
			 ydatepickerEnd.Date = routeList.Addresses
									.Where(x => x.Status == RouteListItemStatus.Completed)
									.Select(x => x.StatusLastUpdate).Max(x => x.Value);  
		}

		protected void OnButtonRecountMileageClicked(object sender, EventArgs e)
		{
			var pointsToRecalculate = new List<PointOnEarth>();
			var pointsToBase = new List<PointOnEarth>();

			var geoGroup = routeList.GeographicGroups.FirstOrDefault();
			if(geoGroup == null)
			{
				throw new InvalidOperationException($"В маршрутном листе должна быть добавлена часть города");
			}

			var geoGroupVersion = geoGroup.GetActualVersionOrNull();
			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {geoGroup.Name}");
			}

			var baseLat = (double)geoGroupVersion.BaseLatitude.Value;
			var baseLon = (double)geoGroupVersion.BaseLongitude.Value;

			decimal totalDistanceTrack = 0;

			tracksOverlay.Clear();
			trackToBaseOverlay.Clear();
			IEnumerable<RouteListItem> completedAddresses = routeList.Addresses.Where(x => x.Status == RouteListItemStatus.Completed);
			if(!completedAddresses.Any()) {
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Для МЛ нет завершенных адресов, невозможно расчитать трек", "");
				return;
			}
			if(completedAddresses.Count() > 1)
			{
				foreach(RouteListItem address in routeList.Addresses.OrderBy(x => x.StatusLastUpdate)) {
					if(address.Status == RouteListItemStatus.Completed) {
						pointsToRecalculate.Add(new PointOnEarth((double)address.Order.DeliveryPoint.Latitude, (double)address.Order.DeliveryPoint.Longitude));
					}
				}

				var recalculatedTrackResponse = _osrmClient.GetRoute(pointsToRecalculate, false, GeometryOverview.Full, _osrmSettings.ExcludeToll);
				var recalculatedTrack = recalculatedTrackResponse.Routes.First();
				var decodedPoints = Polyline.DecodePolyline(recalculatedTrack.RouteGeometry);
				var pointsRecalculated = decodedPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude)).ToList();

				var routeRecalculated = new GMapRoute(pointsRecalculated, "RecalculatedRoute") {
					Stroke = new Pen(Color.Red) {
						Width = 4,
						DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
					}
				};

				tracksOverlay.Routes.Add(routeRecalculated);

				totalDistanceTrack = recalculatedTrack.TotalDistanceKm;
			} else
			{
				var point = routeList.Addresses.First(x => x.Status == RouteListItemStatus.Completed).Order.DeliveryPoint;
				pointsToRecalculate.Add(new PointOnEarth((double)point.Latitude, (double)point.Longitude));
			}

			pointsToBase.Add(pointsToRecalculate.Last());
			pointsToBase.Add(new PointOnEarth(baseLat, baseLon));
			pointsToBase.Add(pointsToRecalculate.First());

			var recalculatedToBaseResponse = _osrmClient.GetRoute(pointsToBase, false, GeometryOverview.Full, _osrmSettings.ExcludeToll);
			var recalculatedToBase = recalculatedToBaseResponse.Routes.First();
			var decodedToBase = Polyline.DecodePolyline(recalculatedToBase.RouteGeometry);
			var pointsRecalculatedToBase = decodedToBase.Select(p => new PointLatLng(p.Latitude, p.Longitude)).ToList();

			var routeRecalculatedToBase = new GMapRoute(pointsRecalculatedToBase, "RecalculatedToBase") {
				Stroke = new Pen(Color.Blue) {
					Width = 4,
					DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
				}
			};

			trackToBaseOverlay.Routes.Add(routeRecalculatedToBase);

			var text = new List<string> {
				string.Format("Дистанция трека: {0:N1} км.", totalDistanceTrack),
				string.Format("Дистанция до базы: {0:N1} км.", recalculatedToBase.TotalDistanceKm),
				string.Format("Общее расстояние: {0:N1} км.", totalDistanceTrack + recalculatedToBase.TotalDistanceKm)
			};

			labelDistance.LabelProp = string.Join("\n", text);

			routeList.RecalculatedDistance = decimal.Round(totalDistanceTrack + recalculatedToBase.TotalDistanceKm);
			UoW.Save(routeList);
			UoW.Commit();
		}
	}

	class DistanceTextInfo{
		public string Id;
		public Pango.Layout PangoLayout;
	}
}

