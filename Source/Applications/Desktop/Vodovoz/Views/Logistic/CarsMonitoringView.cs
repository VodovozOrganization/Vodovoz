﻿using Gamma.ColumnConfig;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Gtk;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class CarsMonitoringView : TabViewBase<CarsMonitoringViewModel>
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private Window _mapSeparateWindow;

		private const MapProviders _defaultMapProvider = MapProviders.GoogleMap;

		private readonly GMapOverlay _carsOverlay;
		private readonly GMapOverlay _tracksOverlay;
		private readonly GMapOverlay _fastDeliveryCarCirclesOverlay;
		private readonly GMapOverlay _fastDeliveryDistrictsOverlay;

		private uint _timerId;

		private GLib.TimeoutHandler _timeoutTimerHandler;

		private IDictionary<int, CarMarker> _carMarkers;
		private IDictionary<int, CarMarkerType> _lastSelectedDrivers;
		private IList<DistanceTextInfo> _tracksDistanceTextInfo;

		public CarsMonitoringView(CarsMonitoringViewModel viewModel) : base(viewModel)
		{
			_carsOverlay = new GMapOverlay(ViewModel.CarsOverlayId);
			_tracksOverlay = new GMapOverlay(ViewModel.TracksOverlayId);
			_fastDeliveryCarCirclesOverlay = new GMapOverlay(ViewModel.FastDeliveryOverlayId);
			_fastDeliveryDistrictsOverlay = new GMapOverlay(ViewModel.FastDeliveryDistrictsOverlayId);

			_carMarkers = new Dictionary<int, CarMarker>();
			_lastSelectedDrivers = new Dictionary<int, CarMarkerType>();
			_tracksDistanceTextInfo = new List<DistanceTextInfo>();

			_timeoutTimerHandler = new GLib.TimeoutHandler(UpdateCarPosition);

			Build();

			ybtnOpenKeeping.Binding.AddBinding(ViewModel, vm => vm.CanOpenKeepingTab, w => w.Sensitive);

			ytbtnShowAddressesList.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowAddresses, w => w.Active)
				.AddBinding(vm => vm.CanShowAddresses, w => w.Sensitive)
				.InitializeFromSource();

			ylblAddressesList.Binding.AddBinding(ViewModel, wm => wm.ShowAddresses, w => w.Visible)
				.InitializeFromSource();

			ycheckbuttonIsFastDeliveryOnly.Binding.AddBinding(ViewModel, vm => vm.ShowFastDeliveryOnly, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonShowFastDeliveryCircle.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowCarCirclesOverlay, w => w.Active)
				.AddBinding(vm => vm.ShowFastDeliveryOnly, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureWorkingDriversTreeView();

			ConfigureRouteListAddressesTreeView();

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
			yenumcomboMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			yenumcomboMapType.SelectedItem = _defaultMapProvider;

			ychkbtnShowHistory.Binding.AddBinding(ViewModel, vm => vm.ShowHistory, w => w.Active)
				.InitializeFromSource();


			ydatepickerHistoryDate.Binding.AddBinding(ViewModel, vm => vm.HistoryDate, w => w.Date)
				.InitializeFromSource();

			yspeccomboboxHistoryHour.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.HistoryHours, w => w.ItemsList)
				.AddBinding(vm => vm.HistoryHour, w => w.SelectedItem)
				.InitializeFromSource();

			ConfigureMap();
			SubscribeToEvents();

			ViewModel.RefreshWorkingDriversCommand?.Execute();

			UpdateCarPosition();

			_timerId = GLib.Timeout.Add(ViewModel.CarRefreshInterval, _timeoutTimerHandler);
		}

		private void ShowHistoryToggled(object sender, EventArgs e)
		{
			if(ViewModel.ShowHistory)
			{
				GLib.Source.Remove(_timerId);
			}
			else
			{
				_timerId = GLib.Timeout.Add(ViewModel.CarRefreshInterval, _timeoutTimerHandler);
			}
		}

		private void ConfigureRouteListAddressesTreeView()
		{
			yTreeAddresses.ColumnsConfig = FluentColumnsConfig<RouteListAddressNode>.Create()
				.AddColumn("МЛ №").AddTextRenderer(node => node.RouteListNumber.ToString())
				.AddColumn("Время").AddTextRenderer(node => node.Time.DeliveryTime)
				.AddColumn("Статус").AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Адрес").AddTextRenderer(node => node.DeliveryPoint.CompiledAddress)
				.Finish();

			yTreeAddresses.ItemsDataSource = ViewModel.RouteListAddresses;
		}

		private void ConfigureWorkingDriversTreeView()
		{
			yTreeViewDrivers.ColumnsConfig = FluentColumnsConfig<WorkingDriverNode>.Create()
				.AddColumn("№").AddNumericRenderer(node => node.RowNumber)
				.AddColumn("Имя").AddTextRenderer(node => node.ShortName)
				.AddColumn("Машина").AddTextRenderer().AddSetter((c, node) => c.Markup = node.CarText)
				.AddColumn("МЛ").AddTextRenderer().AddSetter((c, node) => c.Markup = node.RouteListsText)
				.AddColumn("Выполнено").AddProgressRenderer(x => x.CompletedPercent).AddSetter((c, n) => c.Text = n.CompletedText)
				.AddColumn("Остаток бут.").AddTextRenderer().AddSetter((c, node) => c.Markup = $"{node.BottlesLeft:N0}")
				.AddColumn("Остаток запаса").AddTextRenderer().AddSetter((c, node) => c.Markup = $"{node.Water19LReserve:N0}")
				.Finish();

			yTreeViewDrivers.ItemsDataSource = ViewModel.WorkingDrivers;
			yTreeViewDrivers.Selection.Mode = SelectionMode.Multiple;
		}

		private void SubscribeToEvents()
		{
			ViewModel.WorkingDrivers.CollectionChanged += (s, e) => { yTreeViewDrivers.YTreeModel.EmitModelChanged(); };
			ViewModel.RouteListAddresses.CollectionChanged += (s, e) => { yTreeAddresses.YTreeModel.EmitModelChanged(); };
			ViewModel.FastDeliveryDistricts.CollectionChanged += FastDeliveryDistrictsGeometryChanged;
			ViewModel.SelectedWorkingDrivers.CollectionChanged += SelectedDriversChanged;
			ViewModel.WorkingDrivers.CollectionChanged += WorkingDriversChanged;

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;
			yTreeViewDrivers.RowActivated += OnYTreeViewDriversRowActivated;
			ybtnOpenKeeping.Clicked += OnButtonOpenKeepingClicked;
			ychkbtnShowHistory.Toggled += ShowHistoryToggled;

			ybuttonTrackPoints.Clicked += OnButtonTrackPointsClicked;
			buttonRefresh.Clicked += OnButtonRefreshClicked;
			buttonCleanTrack.Clicked += OnButtonCleanTrackClicked;
			yenumcomboMapType.ChangedByUser += OnYenumcomboMapTypeChangedByUser;
			buttonMapInWindow.Clicked += OnButtonMapInWindowClicked;
		}

		private void UnSubscribeFromEvents()
		{
			ViewModel.FastDeliveryDistricts.CollectionChanged -= FastDeliveryDistrictsGeometryChanged;
			ViewModel.SelectedWorkingDrivers.CollectionChanged -= SelectedDriversChanged;
			ViewModel.WorkingDrivers.CollectionChanged -= WorkingDriversChanged;
			
			ViewModel.PropertyChanged -= ViewModelPropertyChanged;

			yTreeViewDrivers.Selection.Changed -= OnSelectionChanged;
			yTreeViewDrivers.RowActivated -= OnYTreeViewDriversRowActivated;
			ybtnOpenKeeping.Clicked -= OnButtonOpenKeepingClicked;
			ychkbtnShowHistory.Toggled -= ShowHistoryToggled;

			ybuttonTrackPoints.Clicked -= OnButtonTrackPointsClicked;
			buttonRefresh.Clicked -= OnButtonRefreshClicked;
			buttonCleanTrack.Clicked -= OnButtonCleanTrackClicked;
			yenumcomboMapType.ChangedByUser -= OnYenumcomboMapTypeChangedByUser;
			buttonMapInWindow.Clicked -= OnButtonMapInWindowClicked;
		}

		private void WorkingDriversChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_tracksOverlay.Clear();
			_carsOverlay.Clear();
			UpdateCarPosition();
		}

		private void SelectedDriversChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach(var driver in ViewModel.SelectedWorkingDrivers)
			{
				if(!_lastSelectedDrivers.ContainsKey(driver.Id) && _carMarkers != null && _carMarkers.ContainsKey(driver.Id))
				{
					_lastSelectedDrivers.Add(driver.Id, _carMarkers[driver.Id].Type);
					_carMarkers[driver.Id].Type = GetCarSelectedIconType(driver);
				}
			}

			foreach(var pair in _lastSelectedDrivers.ToList())
			{
				if(!ViewModel.SelectedWorkingDrivers.Any(d => d.Id == pair.Key) && _carMarkers != null)
				{
					if(_carMarkers.ContainsKey(pair.Key))
					{
						_carMarkers[pair.Key].Type = pair.Value;
					}

					_lastSelectedDrivers.Remove(pair.Key);
				}
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ShowCarCirclesOverlay))
			{
				UpdateCarPosition();
			}

			if(e.PropertyName == nameof(ViewModel.ShowAddresses))
			{
				swAddressesListContainer.Visible = ViewModel.ShowAddresses;
			}
		}

		private void FastDeliveryDistrictsGeometryChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateDeliveryDistrictsOverlay();
			UpdateCarPosition();
		}

		private void UpdateDeliveryDistrictsOverlay()
		{
			_fastDeliveryDistrictsOverlay.Clear();

			if(!ViewModel.FastDeliveryDistricts.Any())
			{
				return;
			}

			foreach(var district in ViewModel.FastDeliveryDistricts)
			{
				var polygon = CreateDistrictPolygon(
					district.DistrictBorder.Coordinates.ToPointLatLng(),
					district.DistrictName);

				_fastDeliveryDistrictsOverlay.Polygons.Add(polygon);
			}
		}

		private void ConfigureMap()
		{
			gmapWidget.MapProvider = GMapProviders.GoogleMap;
			gmapWidget.Position = ViewModel.DefaultMapCenterPosition.ToPointLatLng();
			gmapWidget.HeightRequest = 150;
			gmapWidget.Overlays.Add(_fastDeliveryDistrictsOverlay);
			gmapWidget.Overlays.Add(_carsOverlay);
			gmapWidget.Overlays.Add(_tracksOverlay);
			gmapWidget.Overlays.Add(_fastDeliveryCarCirclesOverlay);
			gmapWidget.ExposeEvent += GmapWidget_ExposeEvent;
			gmapWidget.OnMarkerEnter += GmapWidgetOnMarkerEnter;
		}

		private void OnYenumcomboMapTypeChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		private void OnButtonCleanTrackClicked(object sender, EventArgs e)
		{
			_tracksOverlay.Clear();
		}

		private void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			_logger.Info("Обновляем данные диалога...");
			ViewModel.RefreshWorkingDriversCommand?.Execute();
			UpdateCarPosition();
			_logger.Info("Ок");
		}

		private void OnButtonTrackPointsClicked(object sender, EventArgs e)
		{
			ViewModel.OpenTrackPointsJournalTabCommand?.Execute();
		}

		private void OnButtonOpenKeepingClicked(object sender, EventArgs e)
		{
			var selectedDrivers = ViewModel.SelectedWorkingDrivers;
			foreach(var driver in selectedDrivers)
			{
				foreach(var routeListId in driver.RouteListsIds.Select(x => x.Key))
				{
					ViewModel.OpenKeepingDialogCommand?.Execute(routeListId);
				}
			}
		}

		private void OnYTreeViewDriversRowActivated(object o, RowActivatedArgs args)
		{
			var driverId = yTreeViewDrivers.GetSelectedId();
			ViewModel.RefreshRouteListAddressesCommand?.Execute(driverId);
			LoadTracksForDriver(driverId);
		}

		void GmapWidget_ExposeEvent(object o, ExposeEventArgs args)
		{
			if(_tracksDistanceTextInfo.Count == 0)
			{
				return;
			}

			var g = args.Event.Window;
			var aria = args.Event.Area;
			int voffset = 0;
			var gc = gmapWidget.Style.TextGC(StateType.Normal);

			foreach(var distance in _tracksDistanceTextInfo)
			{
				distance.PangoLayout.GetPixelSize(out int layoutWidth, out int layoutHeight);
				g.DrawLayout(gc, aria.Right - 6 - layoutWidth, aria.Top + 6 + voffset, distance.PangoLayout);
				voffset += 3 + layoutHeight;
			}
		}

		private void GmapWidgetOnMarkerEnter(GMapMarker item)
		{
			if(!(item.Tag is RouteListAddressNode node) || !node.Order.IsFastDelivery)
			{
				return;
			}

			var index = item.ToolTipText.LastIndexOf("\nОсталось времени", StringComparison.CurrentCulture);

			if(index != -1)
			{
				item.ToolTipText = item.ToolTipText.Remove(index);
			}

			if(node.RouteListItem.Status != RouteListItemStatus.EnRoute)
			{
				return;
			}

			var timeDiff = node.RouteListItem.CreationDate.Add(ViewModel.FastDeliveryTime) - DateTime.Now;
			var timeRemainingStr = timeDiff.Days == 0
				? $"{timeDiff: hh':'mm':'ss}"
				: $"{Math.Abs(timeDiff.Days)} {NumberToTextRus.Case(timeDiff.Days, "день", "дня", "дней")} {timeDiff:hh':'mm':'ss}";

			if(DateTime.Now > node.RouteListItem.CreationDate.Add(ViewModel.FastDeliveryTime))
			{
				timeRemainingStr = $"-{timeRemainingStr}";
			}

			item.ToolTipText += $"\nОсталось времени: {timeRemainingStr}";
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			var selected = yTreeViewDrivers.SelectedRows.Cast<WorkingDriverNode>().ToList();

			ViewModel.SelectedWorkingDrivers.Clear();
			foreach(var selectedNode in selected)
			{
				ViewModel.SelectedWorkingDrivers.Add(selectedNode);
			}
		}

		private bool UpdateCarPosition()
		{
			try
			{
				var routesIds = ViewModel.WorkingDrivers
					.SelectMany(x => x.RouteListsIds.Keys)
					.ToArray();

				var start = DateTime.Now; // Значение времени? 0_о

				DateTime disconnectedDateTime = start.Add(ViewModel.DriverDisconnectedTimespan);

				IList<DriverPosition> lastPoints = ViewModel.GetLastRouteListTrackPoints(routesIds);

				var movedDrivers = lastPoints.Where(x => x.Time > disconnectedDateTime)
					.Select(x => x.RouteListId)
					.ToArray();

				var ere20Minuts = ViewModel.GetLastRouteListTrackPoints(movedDrivers, disconnectedDateTime);
				_logger.Debug("Время запроса точек: {0}", DateTime.Now - start);

				var driversWithAdditionalLoading = ViewModel.GetDriversWithAdditionalLoadingFrom(routesIds);

				_carsOverlay.Clear();
				_fastDeliveryCarCirclesOverlay.Clear();
				_carMarkers.Clear();

				foreach(var pointsForDriver in lastPoints.GroupBy(x => x.DriverId))
				{
					var lastPoint = pointsForDriver.OrderBy(x => x.Time).Last();
					var driverRow = ViewModel.WorkingDrivers.First(x => x.Id == lastPoint.DriverId);

					CarMarkerType iconType = GetIconType(disconnectedDateTime, ere20Minuts, pointsForDriver, lastPoint, driverRow);

					if(_lastSelectedDrivers.ContainsKey(lastPoint.DriverId))
					{
						_lastSelectedDrivers[lastPoint.DriverId] = iconType;
						iconType = GetCarSelectedIconType(driverRow);
					}

					CarMarker marker = CreateMarker(
						lastPoint.ToPointLatLng(),
						iconType,
						GetCarMarkerText(lastPoint, driverRow));

					_carsOverlay.Markers.Add(marker);

					if(ViewModel.ShowCarCirclesOverlay
					&& driversWithAdditionalLoading.Contains(pointsForDriver.Key))
					{
						_fastDeliveryCarCirclesOverlay.Polygons.Add(CustomPolygons.CreateCirclePolygon(
							lastPoint.ToPointLatLng(),
							ViewModel.FastDeliveryMaxDistance,
							ViewModel.FastDeliveryCircleFillColor));
					}

					_carMarkers.Add(lastPoint.DriverId, marker);
				}
			}
			catch(Exception ex)
			{
				_logger.Error("Ошибка при обновлении позиции автомобиля", ex);
				return false;
			}
			return true;
		}

		private static string GetCarMarkerText(DriverPosition lastPoint, WorkingDriverNode driverRow)
		{
			string text = $"{driverRow.ShortName}({driverRow.CarNumber})";

			if(lastPoint.Time < DateTime.Now.AddSeconds(-30))
			{
				text += lastPoint.Time.Date == DateTime.Today
					? $"\nБыл виден: {lastPoint.Time:t} "
					: $"\nБыл виден: {lastPoint.Time:g} ";
			}

			return text;
		}

		private static CarMarkerType GetCarSelectedIconType(WorkingDriverNode driverRow)
		{
			return driverRow.IsVodovozAuto ? CarMarkerType.BlackCarVodovoz : CarMarkerType.BlackCar;
		}

		private CarMarkerType GetIconType(
			DateTime disconnectedDateTime,
			IList<DriverPosition> ere20Minuts,
			IGrouping<int, DriverPosition> pointsForDriver,
			DriverPosition lastPoint,
			WorkingDriverNode driverRow)
		{
			CarMarkerType iconType;

			var ere20 = ere20Minuts.Where(x => x.DriverId == pointsForDriver.Key)
				.OrderBy(x => x.Time)
				.LastOrDefault();

			if(lastPoint.Time < disconnectedDateTime)
			{
				iconType = driverRow.IsVodovozAuto ? CarMarkerType.BlueCarVodovoz : CarMarkerType.BlueCar;
			}
			else if(ere20 != null)
			{
				var distance = gmapWidget.MapProvider.Projection.GetDistance(
					lastPoint.ToPointLatLng(),
					ere20.ToPointLatLng());

				if(distance <= 0.1)
				{
					iconType = driverRow.IsVodovozAuto ? CarMarkerType.RedCarVodovoz : CarMarkerType.RedCar;
				}
				else
				{
					iconType = driverRow.IsVodovozAuto ? CarMarkerType.GreenCarVodovoz : CarMarkerType.GreenCar;
				}
			}
			else
			{
				iconType = driverRow.IsVodovozAuto ? CarMarkerType.GreenCarVodovoz : CarMarkerType.GreenCar;
			}

			return iconType;
		}

		private void LoadTracksForDriver(int driverId)
		{
			_tracksOverlay.Clear();
			_tracksDistanceTextInfo.Clear();

			//Load tracks
			var driverRow = ViewModel.WorkingDrivers.FirstOrDefault(x => x.Id == driverId);
			int colorIter = 0;

			foreach(var routeId in driverRow.RouteListsIds)
			{
				var pointList = ViewModel.GetRouteListTrackPoints(routeId.Key);

				if(pointList.Count == 0)
				{
					continue;
				}

				GMapRoute route = CreateTrackRoute(colorIter, routeId, pointList.ToPointLatLng());

				colorIter++;

				_tracksDistanceTextInfo.Add(CreateRouteInfo(route));
				_tracksOverlay.Routes.Add(route);
			}

			//LoadAddresses
			foreach(var point in ViewModel.RouteListAddresses)
			{
				if(point.DeliveryPoint is null)
				{
					_logger.Warn("Добавление маркера для заказа №{OrderId} пропущено, отсутствует точка доставки.", point.Order.Id);
					continue;
				}
				if(point.DeliveryPoint.Latitude.HasValue && point.DeliveryPoint.Longitude.HasValue)
				{
					_tracksOverlay.Markers.Add(
						CreateAddressMarker(point, $"{point.DeliveryPoint.ShortAddress}\n" +
							$"Время доставки: {point.Time?.Name ?? "Не назначено"}"));
				}
			}
			buttonCleanTrack.Sensitive = true;
		}

		#region Map Drawing

		private GMapRoute CreateTrackRoute(int colorIter, KeyValuePair<int, int?> routeId, IEnumerable<PointLatLng> points)
		{
			return new GMapRoute(points, routeId.ToString())
			{
				Stroke = GetTrackPen(colorIter)
			};
		}

		private Color GetTrackColor(int iteration)
		{
			var colorNum = iteration % 10;
			return Color.FromArgb(144, ViewModel.AvailableTrackColors[colorNum]);
		}

		private Pen GetTrackPen(int colorIter)
		{
			return new Pen(GetTrackColor(colorIter))
			{
				Width = 4,
				DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
			};
		}

		private static GMarkerGoogle CreateAddressMarker(RouteListAddressNode addressNode, string tooltip)
		{
			GMarkerGoogleType type;

			switch(addressNode.Status)
			{
				case RouteListItemStatus.Completed:
					type = GMarkerGoogleType.green_small;
					break;
				case RouteListItemStatus.EnRoute:
					if(addressNode.Order != null && addressNode.Order.IsFastDelivery)
					{
						type = GMarkerGoogleType.yellow_small;
					}
					else
					{
						type = GMarkerGoogleType.gray_small;
					}
					break;
				case RouteListItemStatus.Canceled:
					type = GMarkerGoogleType.purple_small;
					break;
				case RouteListItemStatus.Overdue:
					type = GMarkerGoogleType.red_small;
					break;
				default:
					type = GMarkerGoogleType.none;
					break;
			}

			return new GMarkerGoogle(addressNode.DeliveryPoint.GetPointLatLng(), type)
			{
				Tag = addressNode,
				ToolTipText = tooltip
			};
		}

		private static CarMarker CreateMarker(PointLatLng pointLatLng, CarMarkerType iconType, string text)
		{
			return new CarMarker(pointLatLng, iconType)
			{
				ToolTipText = text
			};
		}

		private GMapPolygon CreateDistrictPolygon(List<PointLatLng> points, string title)
		{
			return new GMapPolygon(points, title)
			{
				Fill = new SolidBrush(ViewModel.DistrictFillColor),
			};
		}

		private DistanceTextInfo CreateRouteInfo(GMapRoute route)
		{
			var layout = new Pango.Layout(PangoContext) { Alignment = Pango.Alignment.Right };
			var colTXT = ColorTranslator.ToHtml(route.Stroke.Color);
			layout.SetMarkup($"<span foreground=\"{colTXT}\"><span font=\"Segoe UI Symbol\">⛽</span> {route.Distance:N1} км.</span>");

			return new DistanceTextInfo
			{
				PangoLayout = layout
			};
		}
		#endregion

		#region Separate Window
		private void OnButtonMapInWindowClicked(object sender, EventArgs e)
		{
			if(_mapSeparateWindow == null)
			{
				ViewModel.SeparateVindowOpened = true;
				_mapSeparateWindow = new Window("Карта мониторинга автомобилей на маршруте");
				_mapSeparateWindow.SetDefaultSize(700, 600);
				_mapSeparateWindow.DeleteEvent += MapWindow_DeleteEvent;
				vboxRightSideContainer.Remove(gmapWidget);
				_mapSeparateWindow.Add(gmapWidget);
				_mapSeparateWindow.Show();
			}
			else
			{
				ViewModel.SeparateVindowOpened = false;
				_mapSeparateWindow.Remove(gmapWidget);
				vboxRightSideContainer.PackEnd(gmapWidget, true, true, 1);
				gmapWidget.Show();
				_mapSeparateWindow.Destroy();
				_mapSeparateWindow = null;
			}
		}

		void MapWindow_DeleteEvent(object o, DeleteEventArgs args)
		{
			buttonMapInWindow.Click();
			args.RetVal = false;
		}

		#endregion

		public override void Destroy()
		{
			GLib.Source.Remove(_timerId);
			gmapWidget.Destroy();
			_mapSeparateWindow?.Destroy();
			base.Destroy();
		}

		public override void Dispose()
		{
			UnSubscribeFromEvents();
			base.Dispose();
		}

		class DistanceTextInfo
		{
			public string Id;
			public Pango.Layout PangoLayout;
		}
	}
}
