using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gdk;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using Gtk;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Project.Journal;
using QS.Utilities;
using QS.Views.GtkUI;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Vodovoz.Additions.Logistic;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Logistic;
using static Vodovoz.ViewModels.Logistic.RouteListsOnDayViewModel;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Views.Logistic
{

	public partial class RouteListsOnDayView : TabViewBase<RouteListsOnDayViewModel>
	{
		#region Поля
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly PointLatLng _defaultrPoint = new PointLatLng(59.93900, 30.31646);
		private readonly int _defaultHeight = 150;
		private readonly MapProviders _defaultMapProvider = MapProviders.GoogleMap;
		private readonly Pixbuf _vodovozCarIcon = Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		private readonly GMapOverlay _districtsOverlay = new GMapOverlay("districts");
		private readonly GMapOverlay _driverDistrictsOverlay = new GMapOverlay("driverDistricts");
		private readonly GMapOverlay _addressesOverlay = new GMapOverlay("addresses");
		private readonly GMapOverlay _addressOverlapOverlay = new GMapOverlay("addressOverlaps");
		private readonly GMapOverlay _driverAddressesOverlay = new GMapOverlay("driverAddresses");
		private readonly GMapOverlay _selectionOverlay = new GMapOverlay("selection");
		private readonly GMapOverlay _routeOverlay = new GMapOverlay("route");
		private GMapPolygon _brokenSelection;
		private List<GMapMarker> _selectedMarkers = new List<GMapMarker>();
		private Pixbuf[] _pixbufMarkers;
		private int _addressesWithoutCoordinats;
		private int _addressesWithoutRoutes;
		private int _totalBottlesCountAtDay;
		private int _bottlesWithoutRL;

		private bool _poligonSelection;
		private int _dragSelectionPointId = -1;

		private bool _creatingInProgress;
		#endregion

		public RouteListsOnDayView(RouteListsOnDayViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>
				.Create()
					.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing()
					.AddColumn("Район города").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();

			ytreeviewGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroupNodes, w => w.ItemsDataSource).InitializeFromSource();
			ytreeviewGeographicGroup.HeadersVisible = false;

			if(progressOrders.Adjustment == null)
			{
				progressOrders.Adjustment = new Adjustment(0, 0, 0, 1, 1, 0);
			}

			//Configure map
			_districtsOverlay.IsVisibile = false;
			gmapWidget.MapProvider = GMapProviders.GoogleMap;
			gmapWidget.Position = _defaultrPoint;
			gmapWidget.HeightRequest = _defaultHeight;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(_districtsOverlay);
			gmapWidget.Overlays.Add(_driverDistrictsOverlay);
			gmapWidget.Overlays.Add(_routeOverlay);
			gmapWidget.Overlays.Add(_addressesOverlay);
			gmapWidget.Overlays.Add(_addressOverlapOverlay);
			gmapWidget.Overlays.Add(_driverAddressesOverlay);
			gmapWidget.Overlays.Add(_selectionOverlay);
			gmapWidget.DisableAltForSelection = true;
			gmapWidget.OnSelectionChange += GmapWidget_OnSelectionChange;
			gmapWidget.ButtonPressEvent += GmapWidget_ButtonPressEvent;
			gmapWidget.ButtonReleaseEvent += GmapWidget_ButtonReleaseEvent;
			gmapWidget.MotionNotifyEvent += GmapWidget_MotionNotifyEvent;

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
			yenumcomboMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			yenumcomboMapType.EnumItemSelected += (sender, args) =>
				gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
			yenumcomboMapType.SelectedItem = _defaultMapProvider;

			LoadDistrictsGeometry();

			var colorPink = GdkColors.Pink;
			var primaryBaseColor = GdkColors.PrimaryBase;
			var colorLightRed = GdkColors.DangerBase;

			var primaryText = GdkColors.PrimaryText;
			var dangerText = GdkColors.DangerText;

			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<object>
				.Create()
					.AddColumn("Маркер").AddPixbufRenderer(x => GetRowMarker(x))
					.AddColumn("МЛ/Адрес").AddTextRenderer(x => ViewModel.GetRowTitle(x))
					.AddColumn("Адр./Время").AddTextRenderer(x => ViewModel.GetRowTime(x), useMarkup: true)
					.AddColumn("Бутылей").AddTextRenderer(x => ViewModel.GetRowBottles(x), useMarkup: true)
					.AddColumn("Вес, кг").AddTextRenderer(x => ViewModel.GetRowWeight(x), useMarkup: true)
					.AddColumn("Смена").AddTextRenderer(x => ViewModel.GetRowDeliveryShift(x), useMarkup: true)
					.AddColumn("План").AddTextRenderer(x => ViewModel.GetRowPlanTime(x), useMarkup: true)
					.AddColumn("Бут. 6л").AddTextRenderer(x => ViewModel.GetRowBottlesSix(x))
					.AddColumn("Бут. менее 6л").AddTextRenderer(x => ViewModel.GetRowBottlesSmall(x))
					.AddColumn("Объём, куб.м.").AddTextRenderer(x => ViewModel.GetRowVolume(x), useMarkup: true)
					.AddColumn("Погрузка").Tag(RouteColumnTag.OnloadTime)
						.AddTextRenderer(x => ViewModel.GetRowOnloadTime(x), useMarkup: true)
						.AddSetter((c, n) => c.Editable = n is RouteList)
						.EditedEvent(OnLoadTimeEdited)
					.AddColumn("Километраж").AddTextRenderer(x => ViewModel.GetRowDistance(x))
					.AddColumn("​​Вал. Маржа, %")
						.AddTextRenderer(x => $"{ViewModel.GetGrossMarginPercentage(x):F2}")
						.AddSetter((c, n) =>
						{
							var color = primaryText;

							if(n is RouteList rl && ViewModel.GetGrossMarginPercentage(rl) < 0)
							{
								color = dangerText;
							}

							c.ForegroundGdk = color;
						})
					.AddColumn("Вал. Маржа, руб")
						.AddTextRenderer(x => $"{ViewModel.GetGrossMarginMoney(x):F2}")
						.AddSetter((c, n) =>
						{
							var color = primaryText;

							if(n is RouteList rl && ViewModel.GetGrossMarginMoney(rl) < 0)
							{
								color = dangerText;
							}

							c.ForegroundGdk = color;
						})
					.AddColumn("К клиенту").AddTextRenderer(x => ViewModel.GetRowEquipmentToClient(x))
					.AddColumn("От клиента").AddTextRenderer(x => ViewModel.GetRowEquipmentFromClient(x))
				.Finish();

			ytreeRoutes.HasTooltip = true;
			ytreeRoutes.QueryTooltip += YtreeRoutes_QueryTooltip;
			ytreeRoutes.Selection.Changed += YtreeRoutes_Selection_Changed;

			ytreeviewOnDayDrivers.ColumnsConfig = FluentColumnsConfig<AtWorkDriver>
				.Create()
					.AddColumn("Водитель").AddTextRenderer(x => x.Employee.ShortName)
					.AddColumn("Автомобиль").AddPixbufRenderer(x => x.Car != null && x.CarVersion.IsCompanyCar ? _vodovozCarIcon : null)
						.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
					.AddColumn("База").AddComboRenderer(x => x.GeographicGroup).SetDisplayFunc(x => x.Name)
						.FillItems(ViewModel.GeographicGroupsExceptEast)
						.AddSetter(
							(c, n) =>
							{
								c.Editable = n.Car != null;
								c.BackgroundGdk = n.GeographicGroup == null && n.Car != null
									? colorLightRed
									: primaryBaseColor;
							}
						)
					.AddColumn("")
				.Finish();
			ytreeviewOnDayDrivers.Selection.Mode = SelectionMode.Multiple;
			ytreeviewOnDayDrivers.Selection.Changed += (sender, e) => ViewModel.SelectedDrivers = ytreeviewOnDayDrivers.GetSelectedObjects<AtWorkDriver>().ToArray();
			ytreeviewOnDayDrivers.Binding.AddBinding(ViewModel, vm => vm.ObservableDriversOnDay, w => w.ItemsDataSource).InitializeFromSource();

			ytreeviewAddressesTypes.ColumnsConfig = FluentColumnsConfig<FilterEnumParameterNode<OrderAddressType>>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("Тип адресов").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewAddressesTypes.ItemsDataSource = ViewModel.OrderAddressTypes;

			ytreeviewAddressAdditionalParameters.ColumnsConfig = FluentColumnsConfig<FilterEnumParameterNode<AddressAdditionalParameterType>>.Create()
				.AddColumn("").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("Тип параметра").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewAddressAdditionalParameters.ItemsDataSource = ViewModel.AddressAdditionalParameters;
			ytreeviewAddressAdditionalParameters.HeadersVisible = false;

			ytreeviewShift.ColumnsConfig = FluentColumnsConfig<DeliveryShiftNode>.Create()
				.AddColumn("").AddToggleRenderer(x => x.Selected)
				.AddColumn("Смены").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewShift.ItemsDataSource = ViewModel.DeliveryShiftNodes;


			buttonAddDriver.Clicked += (sender, e) => ViewModel.AddDriverCommand.Execute();

			buttonRemoveDriver.Binding.AddBinding(ViewModel, vm => vm.AreDriversSelected, w => w.Sensitive).InitializeFromSource();
			buttonRemoveDriver.Clicked += (sender, e) => ViewModel.RemoveDriverCommand.Execute(null);

			buttonDriverSelectAuto.Binding.AddBinding(ViewModel, vm => vm.AreDriversSelected, w => w.Sensitive).InitializeFromSource();

			ytreeviewOnDayForwarders.ColumnsConfig = FluentColumnsConfig<AtWorkForwarder>
				.Create()
					.AddColumn("Экспедитор").AddTextRenderer(x => x.Employee.ShortName)
				.Finish();

			ytreeviewOnDayForwarders.Selection.Mode = SelectionMode.Multiple;
			ytreeviewOnDayForwarders.Selection.Changed += (sender, e) => ViewModel.SelectedForwarder = ytreeviewOnDayForwarders.GetSelectedObjects<AtWorkForwarder>().FirstOrDefault();
			ytreeviewOnDayForwarders.Binding.AddBinding(ViewModel, vm => vm.ObservableForwardersOnDay, w => w.ItemsDataSource).InitializeFromSource();

			buttonAddForwarder.Clicked += (sender, e) => ViewModel.AddForwarderCommand.Execute();

			buttonRemoveForwarder.Binding.AddBinding(ViewModel, vm => vm.IsForwarderSelected, w => w.Sensitive).InitializeFromSource();
			buttonRemoveForwarder.Clicked += (sender, e) => ViewModel.RemoveForwarderCommand.Execute(ytreeviewOnDayForwarders.GetSelectedObjects<AtWorkForwarder>());

			yspinMaxTime.Binding.AddBinding(ViewModel.Optimizer, e => e.MaxTimeSeconds, w => w.ValueAsInt).InitializeFromSource();

			ydateForRoutes.Binding.AddBinding(ViewModel, vm => vm.DateForRouting, w => w.DateOrNull).InitializeFromSource();
			checkShowCompleted.Binding.AddBinding(ViewModel, vm => vm.ShowCompleted, w => w.Active).InitializeFromSource();
			ySpnMin19Btls.Binding.AddBinding(ViewModel, vm => vm.MinBottles19L, w => w.ValueAsInt).InitializeFromSource();
			ySpnMax19Btls.Binding.AddBinding(ViewModel, vm => vm.MaxBottles19L, w => w.ValueAsInt).InitializeFromSource();
			ydateForRoutes.Binding.AddBinding(ViewModel, vm => vm.HasNoChanges, w => w.Sensitive).InitializeFromSource();
			checkShowCompleted.Binding.AddBinding(ViewModel, vm => vm.HasNoChanges, w => w.Sensitive).InitializeFromSource();
			checkShowOnlyDriverOrders.Binding.AddBinding(ViewModel, vm => vm.ShowOnlyDriverOrders, w => w.Active).InitializeFromSource();
			checkShowOnlyDriverOrders.Toggled += (sender, e) => GetRowFromYTreeRoutes();

			timeRngPicker.Binding.AddBinding(ViewModel, vm => vm.HasNoChanges, w => w.Sensitive).InitializeFromSource();
			ySpnMin19Btls.Binding.AddBinding(ViewModel, vm => vm.HasNoChanges, w => w.Sensitive).InitializeFromSource();

			timeRngPicker.Binding.AddBinding(ViewModel, vm => vm.DeliveryFromTime, w => w.TimeStart).InitializeFromSource();
			timeRngPicker.Binding.AddBinding(ViewModel, vm => vm.DeliveryToTime, w => w.TimeEnd).InitializeFromSource();
			timeRngPicker.TimePeriodChangedByUser += (sender, e) => FillItems();

			timeDrvShiftRngpicker.Binding.AddBinding(ViewModel, vm => vm.DriverStartTime, t => t.TimeStart).InitializeFromSource();
			timeDrvShiftRngpicker.Binding.AddBinding(ViewModel, vm => vm.DriverEndTime, t => t.TimeEnd).InitializeFromSource();
			timeDrvShiftRngpicker.Binding.AddBinding(ViewModel, vm => vm.HasNoChanges, w => w.Sensitive).InitializeFromSource();

			checkShowDistricts.Toggled += (sender, e) => _districtsOverlay.IsVisibile = checkShowDistricts.Active;

			ViewModel.AutoroutingResultsSaved += (sender, e) => FillDialogAtDay();

			btnSave.Binding.AddBinding(ViewModel, e => e.IsAutoroutingModeActive, w => w.Visible).InitializeFromSource();
			btnSave.Clicked += (sender, e) => ViewModel.SaveCommand.Execute();

			btnCancel.Binding.AddBinding(ViewModel, e => e.IsAutoroutingModeActive, w => w.Visible).InitializeFromSource();
			btnCancel.Clicked += (sender, e) =>
			{
				ViewModel.DisposeUoW();
				ViewModel.CreateUoW();
				ViewModel.HasNoChanges = true;
				ViewModel.IsAutoroutingModeActive = false;
				FillDialogAtDay();
			};
			btnRefresh.Clicked += (sender, e) => Refresh();
			ydateForRoutes.DateChanged += (sender, e) => Refresh();
			Refresh();
			buttonRemoveAddress.Clicked += (sender, e) =>
			{
				ViewModel.RemoveRLItemCommand.Execute(ytreeRoutes.GetSelectedObject<RouteListItem>());
				UpdateMarkersInDriverDistricts(ViewModel.DriverFromRouteList);
				ytreeRoutes.YTreeModel.EmitModelChanged();
				UpdateAddressesOnMap();
			};
			checkShowCompleted.Toggled += (sender, e) => FillDialogAtDay();
			buttonOpen.Clicked += (sender, e) => ViewModel.OpenOrderOrRouteListCommand.Execute(ytreeRoutes.GetSelectedObject());
			buttonMapHelp.Clicked += (sender, e) => new RouresAtDayInfoWnd().Show();
			buttonRebuildRoute.Clicked += (sender, e) =>
			{
				ViewModel.RebuilOneRouteCommand.Execute(ytreeRoutes.GetSelectedObject());
				ytreeRoutes.YTreeModel.EmitModelChanged();
			};
			buttonWarnings.Clicked += (sender, e) => ViewModel.ShowWarningsCommand.Execute();
			ytreeviewOnDayDrivers.RowActivated += OnButtonDriverSelectAutoClicked;
			buttonFilter.Clicked += (sender, e) => FillItems();
			enumCmbDeliveryType.ItemsEnum = typeof(DeliveryScheduleFilterType);
			enumCmbDeliveryType.Binding.AddBinding(ViewModel, vm => vm.DeliveryScheduleType, w => w.SelectedItem).InitializeFromSource();
			enumCmbDeliveryType.ChangedByUser += (sender, e) => FillItems();

			ytextWorkDriversInfo.Binding.AddBinding(ViewModel, vm => vm.CanTake, w => w.Buffer.Text).InitializeFromSource();
			viewDeliverySummary.ColumnsConfig = FluentColumnsConfig<DeliverySummary>
				.Create()
				.AddColumn("Статус").AddTextRenderer(x => x.Name)
				.AddColumn("Адреса").AddTextRenderer(x => x.AddressCount.ToString()).XAlign(0.5f)
				.AddColumn("Бутыли").AddTextRenderer(x => x.Bottles.ToString("N0")).XAlign(0.5f)
				.Finish();

			viewDeliverySummary.Binding.AddBinding(ViewModel, vm => vm.ObservableDeliverySummary, w => w.ItemsDataSource).InitializeFromSource();

			chkExcludeTrukcs.Binding
				.AddBinding(ViewModel, vm => vm.ExcludeTrucks, w => w.Active)
				.InitializeFromSource();
			chkExcludeTrukcs.Toggled += (sender, args) => FillFullOrdersInfo();
		}

		private void GmapWidget_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(_dragSelectionPointId != -1)
			{
				gmapWidget.DisableAltForSelection = true;
				OnPoligonSelectionUpdated();
				_dragSelectionPointId = -1;
			}
		}

		private void GmapWidget_MotionNotifyEvent(object o, MotionNotifyEventArgs args)
		{
			if(_dragSelectionPointId > -1)
			{
				_brokenSelection.Points[_dragSelectionPointId] = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				gmapWidget.UpdatePolygonLocalPosition(_brokenSelection);
				gmapWidget.Refresh();
			}
		}

		private void YtreeRoutes_QueryTooltip(object o, QueryTooltipArgs args)
		{
			ytreeRoutes.ConvertWidgetToBinWindowCoords(args.X, args.Y, out int binX, out int binY);

			if(ytreeRoutes.GetPathAtPos(binX, binY, out TreePath path, out TreeViewColumn col) && ytreeRoutes.Model.GetIter(out TreeIter iter, path))
			{
				var loadtimeCol = ytreeRoutes.ColumnsConfig.GetColumnsByTag(RouteColumnTag.OnloadTime).Where(x => x == col).ToArray();
				if(loadtimeCol.Any() && ytreeRoutes.YTreeModel.NodeFromIter(iter) is RouteList node)
				{
					args.RetVal = true;
					args.Tooltip.Text = ViewModel.GenerateToolTip(node);
				}
			}
		}


		private void GmapWidget_ButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				bool markerIsSelect = false;
				if(args.Event.State.HasFlag(ModifierType.LockMask))
				{
					foreach(var marker in _addressesOverlay.Markers)
					{
						if(marker.IsMouseOver)
						{
							var markerUnderMouse = _selectedMarkers
													.Where(m => m.Tag is OrderOnDayNode)
													.FirstOrDefault(x => (x.Tag as OrderOnDayNode).OrderId == (marker.Tag as OrderOnDayNode)?.OrderId);

							if(markerUnderMouse == null)
							{
								_selectedMarkers.Add(marker);
								logger.Debug("Маркер с заказом №{0} добавлен в список выделенных", (marker.Tag as OrderOnDayNode)?.OrderId);
							}
							else
							{
								_selectedMarkers.Remove(markerUnderMouse);
								logger.Debug("Маркер с заказом №{0} исключен из списка выделенных", (marker.Tag as OrderOnDayNode)?.OrderId);
							}
							markerIsSelect = true;
						}
					}
					UpdateSelectedInfo(_selectedMarkers);
					UpdateAddressesOnMap();
					return;
				}
				if(!markerIsSelect)
				{
					_selectedMarkers.Clear();
					logger.Debug("Список выделенных маркеров очищен");
				}
				UpdateAddressesOnMap();

				if(_poligonSelection)
				{
					GRect rect = new GRect((long)args.Event.X - 5, (long)args.Event.Y - 5, 10, 10);
					rect.OffsetNegative(gmapWidget.RenderOffset);

					_dragSelectionPointId = _brokenSelection.LocalPoints.FindIndex(rect.Contains);
					if(_dragSelectionPointId != -1)
					{
						gmapWidget.DisableAltForSelection = false;
						return;
					}
				}

				if(args.Event.State.HasFlag(ModifierType.ControlMask))
				{
					if(!_poligonSelection)
					{
						_poligonSelection = true;
						logger.Debug("Старт выделения через полигон.");
						var startPoint = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
						_brokenSelection = new GMapPolygon(new List<PointLatLng> { startPoint }, "Выделение");
						gmapWidget.UpdatePolygonLocalPosition(_brokenSelection);
						_selectionOverlay.Polygons.Add(_brokenSelection);
					}
					else
					{
						logger.Debug("Продолжили.");
						var newPoint = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
						_brokenSelection.Points.Add(newPoint);
						gmapWidget.UpdatePolygonLocalPosition(_brokenSelection);
					}
					OnPoligonSelectionUpdated();
				}
				else
				{
					logger.Debug("Закончили.");
					_poligonSelection = false;
					UpdateSelectedInfo(new List<GMapMarker>());
					_selectionOverlay.Clear();
				}
			}

			if(args.Event.Button == 3 && _addressesOverlay.Markers.FirstOrDefault(m => m.IsMouseOver)?.Tag is OrderOnDayNode orderNode)
			{
				Menu popupMenu = new Menu();
				var item = new MenuItem($"Открыть закзаз №{orderNode.OrderId}");
				item.Activated += (sender, e) =>
				{
					var dlg = new OrderDlg(orderNode.OrderId)
					{
						HasChanges = false
					};
					Tab.TabParent.AddSlaveTab(Tab, dlg);
				};
				popupMenu.Add(item);
				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		private void OnPoligonSelectionUpdated()
		{
			var selected = _addressesOverlay.Markers.Where(m => _brokenSelection.IsInside(m.Position)).ToList();
			UpdateSelectedInfo(selected);
		}

		private void YtreeRoutes_Selection_Changed(object sender, EventArgs e)
		{
			object row = GetRowFromYTreeRoutes();

			buttonRemoveAddress.Sensitive = row is RouteListItem && !checkShowCompleted.Active;
			buttonOpen.Sensitive = buttonRebuildRoute.Sensitive = (row is RouteListItem) || (row is RouteList);

			//Рисуем выделенный маршрут
			_routeOverlay.Clear();
			if(row != null)
			{
				if(!(row is RouteList rl))
				{
					rl = (row as RouteListItem).RouteList;
				}

				MapDrawingHelper.DrawRoute(_routeOverlay, rl, ViewModel.DistanceCalculator);

				//Если выбран адрес, центруем на него карту.
				if(row is RouteListItem rli)
				{
					gmapWidget.Position = rli.Order.DeliveryPoint.GmapPoint;
				}
			}
			logger.Info("Ok");
		}

		private object GetRowFromYTreeRoutes()
		{
			var row = ytreeRoutes.GetSelectedObject();

			checkShowOnlyDriverOrders.Sensitive = row is RouteList || row is RouteListItem;

			if(row is RouteList)
			{
				LoadDriverDistrictsGeometry((row as RouteList).Driver);
				ShowOrdersInDriverDistricts((row as RouteList).Driver);
			}

			if(row is RouteListItem)
			{
				LoadDriverDistrictsGeometry((row as RouteListItem).RouteList.Driver);
				ShowOrdersInDriverDistricts((row as RouteListItem).RouteList.Driver);
			}

			return row;
		}

		private void GmapWidget_OnSelectionChange(RectLatLng Selection, bool ZoomToFit)
		{
			if(_poligonSelection)
			{
				return;
			}

			var selected = _addressesOverlay.Markers.Where(m => Selection.Contains(m.Position)).ToList();
			UpdateSelectedInfo(selected);
		}

		private void UpdateSelectedInfo(List<GMapMarker> selected)
		{
			var orderIds = selected.Select(x => x.Tag).OfType<OrderOnDayNode>()
				.Select(o => o.OrderId)
				.ToList();
			var orders = ViewModel.UoW.GetAll<Order>().Where(o => orderIds.Contains(o.Id)).ToList();

			if(!orders.Any())
			{
				labelSelected.Markup = "Адресов\nне выбрано";
				menuAddToRL.Sensitive = false;
				return;
			}

			var selectedBottle = orders.Sum(o => o.Total19LBottlesToDeliver);
			var selectedKilos = orders.Sum(o => o.TotalWeight);
			var selectedCbm = orders.Sum(o => o.TotalVolume);
			var selectedReverseCbm = orders.Sum(o => o.FullReverseVolume());
			labelSelected.Markup = string.Format(
				"{0} адр.; {1} бут.; {2} кг; \nК клиентам: {3:F4} м<sup>3</sup>. От клиентов: {4:F4} м<sup>3</sup>",
				orders.Count(),
				selectedBottle,
				selectedKilos,
				selectedCbm,
				selectedReverseCbm
			);
			menuAddToRL.Sensitive = ViewModel.RoutesOnDay.Any() && !checkShowCompleted.Active;
		}

		private Pixbuf GetRowMarker(object row)
		{
			return PointMarker.GetIconPixbuf(
				ViewModel.GetAddressMarker(
					ViewModel.GetMarkerIndex(
						row,
						_pixbufMarkers.Length
					)
				).ToString(),
				ViewModel.GetMarkerShape(row)
			);
		}

		private void FillFullOrdersInfo()
		{
			ytextFullOrdersInfo.Buffer.Text = ViewModel.GetOrdersInfo();
		}

		private void FillDialogAtDay()
		{
			_addressesOverlay.Clear();
			TurnOffCheckShowOnlyDriverOrders();


			logger.Info("Загружаем заказы на {0:d}...", ViewModel.DateForRouting);
			ViewModel.InitializeData();
			UpdateRoutesPixBuf();
			UpdateRoutesButton();
			UpdateAddressesOnMap();

			var levels = LevelConfigFactory.FirstLevel<RouteList, RouteListItem>(x => x.Addresses).LastLevel(c => c.RouteList).EndConfig();
			ytreeRoutes.YTreeModel = new LevelTreeModel<RouteList>(ViewModel.RoutesOnDay, levels);
		}

		private void UpdateAddressesOnMap()
		{
			logger.Info("Обновляем адреса на карте...");
			_addressesWithoutCoordinats = 0;
			_addressesWithoutRoutes = 0;
			_totalBottlesCountAtDay = 0;
			_bottlesWithoutRL = 0;
			_addressesOverlay.Clear();

			//добавляем маркеры складов
			foreach(var b in ViewModel.GeographicGroupsExceptEast)
			{
				_addressesOverlay.Markers.Add(FillBaseMarker(b));
			}

			var ordersOnDay = ViewModel.OrdersOnDay;
			var ordersRouteLists = ViewModel.OrderRepository.GetAllRouteListsForOrders(ViewModel.UoW, ordersOnDay.Select(o => o.OrderId));
			//добавляем маркеры адресов заказов
			foreach(var order in ordersOnDay)
			{
				_totalBottlesCountAtDay += order.Total19LBottlesToDeliver;

				IEnumerable<int> orderRls;
				if(!ordersRouteLists.TryGetValue(order.OrderId, out orderRls))
				{
					orderRls = new List<int>();
				}

				var route = ViewModel.RoutesOnDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.OrderId));

				if(!orderRls.Any())
				{
					_addressesWithoutRoutes++;
					_bottlesWithoutRL += order.Total19LBottlesToDeliver;
				}

				if(order.DeliveryPointLatitude.HasValue && order.DeliveryPointLongitude.HasValue)
				{
					bool overdueOrder = false;

					List<UndeliveryOrderNode> undeliveryOrderNodes = new List<UndeliveryOrderNode>();

					if(ViewModel.UndeliveredOrdersOnDay != null)
					{
						undeliveryOrderNodes = ViewModel.UndeliveredOrdersOnDay
							.Where(x => RouteListsOnDayViewModel.GuiltyTypesForMarkUndeliveries.Contains(x.GuiltySide))
							.ToList();
					}

					if(undeliveryOrderNodes.Any(x => x.NewOrderId == order.OrderId))
					{
						overdueOrder = true;
					}

					FillTypeAndShapeMarker(order, route, orderRls, out PointMarkerShape shape, out PointMarkerType type, overdueOrder);

					if(_selectedMarkers.FirstOrDefault(m => (m.Tag as OrderOnDayNode)?.OrderId == order.OrderId) != null)
					{
						type = PointMarkerType.white;
					}

					var addressMarker = FillAddressMarker(order, type, shape, _addressesOverlay, route);
					_addressesOverlay.Markers.Add(addressMarker);
				}
				else
				{
					_addressesWithoutCoordinats++;
				}
			}

			PushApartAddresses(_addressesOverlay);

			UpdateOrdersInfo();
			logger.Info("Ок.");
		}

		/// <summary>
		/// Разведение друг от друга слишком близких маркеров адресов и рисование на верхнем слое спец знака,
		/// информирующего о наличии нескольких заказов рядом
		/// </summary>
		/// <param name="addressOverlay"></param>
		private void PushApartAddresses(GMapOverlay addressOverlay)
		{
			_addressOverlapOverlay.Clear();

			var pushApartPrecision = 0.0001d;

			var addressMarkers = addressOverlay.Markers
				.Where(x => x.Tag is OrderOnDayNode)
				.OrderBy(x => x.Position.Lat)
				.ThenBy(x => x.Position.Lng)
				.ToArray();

			var overlapMarkers = new Dictionary<int, List<GMapMarker>>();

			var index = 0;

			foreach(var orderMarker in addressMarkers)
			{
				var intersections = addressMarkers
					.Except(new[] { orderMarker })
					.Where(g => Math.Abs(g.Position.Lat - orderMarker.Position.Lat) < pushApartPrecision
								&& Math.Abs(g.Position.Lng - orderMarker.Position.Lng) < pushApartPrecision)
					.ToList();

				for(var i = 0; i < intersections.Count; i++)
				{
					var intersectMarker = addressMarkers.Single(x => x.Tag == intersections[i].Tag);

					var lat = orderMarker.Position.Lat + (i + 1) * pushApartPrecision + pushApartPrecision / 10;
					var lng = orderMarker.Position.Lng + (i + 1) * pushApartPrecision + pushApartPrecision / 10;

					intersectMarker.Position = new PointLatLng(lat, lng);
				}

				if(intersections.Any() && !overlapMarkers.Values.Any(x => x.Contains(orderMarker)))
				{
					var intersectionsWithSelf = intersections.Concat(new[] { orderMarker }).ToList();
					overlapMarkers.Add(index, intersectionsWithSelf);

					index++;
				}
			}

			foreach(var marker in overlapMarkers)
			{
				var lat = marker.Value.Min(x => x.Position.Lat) +
						  (marker.Value.Max(x => x.Position.Lat) - marker.Value.Min(x => x.Position.Lat)) /
						  2;
				var lng = marker.Value.Min(x => x.Position.Lng) +
						  (marker.Value.Max(x => x.Position.Lng) - marker.Value.Min(x => x.Position.Lng)) /
						  2;

				var overlapMarker = new PointMarker(new PointLatLng(lat, lng), PointMarkerType.color21, PointMarkerShape.overduestar)
				{
					ToolTipText = $"{marker.Value.Count} заказа(ов) рядом."
				};

				_addressOverlapOverlay.Markers.Add(overlapMarker);
			}
		}

		private void FillTypeAndShapeMarker(OrderOnDayNode order, RouteList route, IEnumerable<int> orderRlsIds, out PointMarkerShape shape, out PointMarkerType type, bool overdueOrder = false)
		{
			shape = ViewModel.GetMarkerShapeFromBottleQuantity(order.Total19LBottlesToDeliver, overdueOrder);
			type = PointMarkerType.black;

			if(!orderRlsIds.Any())
			{
				if((order.DeliverySchedule.To - order.DeliverySchedule.From).TotalHours <= 1)
				{
					type = PointMarkerType.black_and_red;
				}
				else
				{
					double from = order.DeliverySchedule.From.TotalMinutes;
					double to = order.DeliverySchedule.To.TotalMinutes;
					if(from >= 1080 && to <= 1439)//>= 18:00, <= 23:59
					{
						type = PointMarkerType.grey_stripes;
					}
					else if(from >= 0)
					{
						if(to <= 720)//<= 12:00
						{
							type = PointMarkerType.red_stripes;
						}
						else if(to <= 900)//<=15:00
						{
							type = PointMarkerType.yellow_stripes;
						}
						else if(to <= 1080)//<= 18:00
						{
							type = PointMarkerType.green_stripes;
						}
					}
				}
			}

			if(route != null)
			{
				type = ViewModel.GetAddressMarker(ViewModel.RoutesOnDay.IndexOf(route));
			}
		}

		private void FillTypeAndShapeLogisticsRequrementsMarker(OrderOnDayNode order, out PointMarkerShape shape, out PointMarkerType type)
		{
			shape = PointMarkerShape.none;
			type = PointMarkerType.none;

			if(order.LogisticsRequirements == null || order.LogisticsRequirements.SelectedRequirementsCount == 0)
			{
				return;
			}

			shape = PointMarkerShape.custom;

			var selectedRequrementsCount = order.LogisticsRequirements.SelectedRequirementsCount;

			if(selectedRequrementsCount > 1)
			{
				type = PointMarkerType.logistics_requirements_many;
				return;
			}

			if(order.LogisticsRequirements.ForwarderRequired)
			{
				type = PointMarkerType.logistics_requirements_forwarder;
				return;
			}
			if(order.LogisticsRequirements.DocumentsRequired)
			{
				type = PointMarkerType.logistics_requirements_documents;
				return;
			}
			if(order.LogisticsRequirements.RussianDriverRequired)
			{
				type = PointMarkerType.logistics_requirements_nationality;
				return;
			}
			if(order.LogisticsRequirements.PassRequired)
			{
				type = PointMarkerType.logistics_requirements_pass;
				return;
			}
			if(order.LogisticsRequirements.LargusRequired)
			{
				type = PointMarkerType.logistics_requirements_largus;
				return;
			}
		}

		private void FillTypeAndShapeOrderInfoMarker(OrderOnDayNode order, out PointMarkerShape shape, out PointMarkerType type)
		{
			shape = PointMarkerShape.none;
			type = PointMarkerType.none;

			if(!order.IsCoolerAddedToOrder && !order.IsSmallBottlesAddedToOrder)
			{
				return;
			}

			shape = PointMarkerShape.custom;

			if(order.IsCoolerAddedToOrder && order.IsSmallBottlesAddedToOrder)
			{
				type = PointMarkerType.order_info_many;
				return;
			}

			if(order.IsCoolerAddedToOrder && !order.IsSmallBottlesAddedToOrder)
			{
				type = PointMarkerType.order_info_cooler;
				return;
			}

			if(!order.IsCoolerAddedToOrder && order.IsSmallBottlesAddedToOrder)
			{
				type = PointMarkerType.order_info_small_bottles;
				return;
			}
		}

		private PointMarker FillBaseMarker(GeoGroup geoGroup)
		{
			var geoGroupVersion = geoGroup.GetActualVersionOrNull();
			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {geoGroup.Name}");
			}

			var addressMarker = new PointMarker(
				new PointLatLng(
					(double)geoGroupVersion.BaseLatitude,
					(double)geoGroupVersion.BaseLongitude
				),
				PointMarkerType.vodonos,
				PointMarkerShape.custom
			)
			{
				Tag = geoGroup
			};
			return addressMarker;
		}

		private PointMarker FillAddressMarker(OrderOnDayNode order, PointMarkerType type, PointMarkerShape shape, GMapOverlay overlay, RouteList route)
		{
			int maxCharsInRow = 60;
			string ttText = WordWrapText(order.DeliveryPointShortAddress, maxCharsInRow);
			if(order.Total19LBottlesToDeliver > 0)
			{
				ttText += WordWrapText($"Бутылей 19л: {order.Total19LBottlesToDeliver}", maxCharsInRow);
			}

			if(order.Total6LBottlesToDeliver > 0)
			{
				ttText += WordWrapText($"Бутылей 6л: {order.Total6LBottlesToDeliver}", maxCharsInRow);
			}

			if(order.Total1500mlBottlesToDeliver > 0)
			{
				ttText += WordWrapText($"Бутылей 1,5л: {order.Total1500mlBottlesToDeliver}", maxCharsInRow);
			}

			if(order.Total600mlBottlesToDeliver > 0)
			{
				ttText += WordWrapText($"Бутылей 0,6л: {order.Total600mlBottlesToDeliver}", maxCharsInRow);
			}

			if(order.Total500mlBottlesToDeliver > 0)
			{
				ttText += WordWrapText($"Бутылей 0,5л: {order.Total500mlBottlesToDeliver}", maxCharsInRow);
			}

			ttText += WordWrapText($"Забор бутылей: {order.BottlesReturn}", maxCharsInRow);

			var deliveryTime = order.DeliverySchedule?.Name ?? "Не назначено";
			ttText += WordWrapText($"Время доставки: {deliveryTime}", maxCharsInRow);

			var districtName = ViewModel.LogisticanDistricts?.FirstOrDefault(x => x.DistrictBorder.Contains(order.DeliveryPointNetTopologyPoint))?.DistrictName;
			ttText += WordWrapText($"Район: {districtName}", maxCharsInRow);

			var comment = GetMarkerCommentValue(order);
			ttText += WordWrapText($"Комментарий: {comment}", maxCharsInRow);

			var orderLat = (double)order.DeliveryPointLatitude;
			var orderLong = (double)order.DeliveryPointLongitude;

			FillTypeAndShapeLogisticsRequrementsMarker(order, out PointMarkerShape logisticsRequirementsShape, out PointMarkerType logisticsRequirementsType);

			FillTypeAndShapeOrderInfoMarker(order, out PointMarkerShape orderInfoShape, out PointMarkerType orderInfoType);

			var addressMarker = new PointMarker(new PointLatLng(orderLat, orderLong), type, shape)
			{
				Tag = order,
				ToolTipText = ttText,
				LogisticsRequirementsMarkerShape = logisticsRequirementsShape,
				LogisticsRequirementsMarkerType = logisticsRequirementsType,
				OrderInfoMarkerShape = orderInfoShape,
				OrderInfoMarkerType = orderInfoType
			};

			if(route != null)
			{
				addressMarker.ToolTipText += "\n";
				addressMarker.ToolTipText += string.Format(" Везёт: {0}", route.Driver.ShortName);
			}

			return addressMarker;
		}

		private string WordWrapText(string text, int maxCharsInRow)
		{
			var subRows = text.Split('\n');
			var sb = new StringBuilder();

			foreach(var subRow in subRows)
			{
				sb.AppendLine(WordWrap(subRow, maxCharsInRow));
			}

			return sb.ToString();
		}

		private string WordWrap(string text, int maxCharsInRow)
		{
			var source = new StringBuilder(text.Trim());

			var totalRows = (source.Length / maxCharsInRow) + 1;
			if(totalRows <= 1)
			{
				return source.ToString();
			}

			int lastRowNumber = 1;
			int lastSpaceIndex = 0;

			for(int i = 0; i < source.Length; i++)
			{
				char c = source[i];

				if(c == ' ')
				{
					lastSpaceIndex = i;
				}

				var currentRowNumber = (i / maxCharsInRow) + 1;
				if(currentRowNumber > totalRows)
				{
					break;
				}

				if(lastRowNumber < currentRowNumber)
				{
					source.Insert(lastSpaceIndex, '\n');
					i++;
				}

				lastRowNumber = currentRowNumber;
			}

			return source.ToString();
		}

		private string GetMarkerCommentValue(OrderOnDayNode order)
		{
			if(order.OrderComment?.Length > 0)
			{
				return order.OrderComment;
			}
			if(order.DeliveryPointComment?.Length > 0)
			{
				return order.DeliveryPointComment;
			}
			if(order.CommentManager?.Length > 0)
			{
				return order.CommentManager;
			}
			if(order.ODZComment?.Length > 0)
			{
				return order.ODZComment;
			}
			if(order.OPComment?.Length > 0)
			{
				return order.OPComment;
			}
			if(order.DriverMobileAppComment?.Length > 0)
			{
				return order.DriverMobileAppComment;
			}
			return "-";
		}

		private void Refresh()
		{
			FillDialogAtDay();
			FillFullOrdersInfo();
		}

		private void UpdateOrdersInfo()
		{
			textOrdersInfo.Buffer.Text = ViewModel.GetOrdersInfo(_addressesWithoutCoordinats, _addressesWithoutRoutes, _totalBottlesCountAtDay, _bottlesWithoutRL);

			if(progressOrders.Adjustment != null)
			{
				progressOrders.Adjustment.Upper = ViewModel.OrdersOnDay.Count;
				progressOrders.Adjustment.Value = ViewModel.OrdersOnDay.Count - _addressesWithoutRoutes;
			}
			if(!ViewModel.OrdersOnDay.Any())
			{
				progressOrders.Text = string.Empty;
			}
			else if(_addressesWithoutRoutes == 0)
			{
				progressOrders.Text = "Готово.";
			}
			else
			{
				progressOrders.Text = NumberToTextRus.FormatCase(_addressesWithoutRoutes, "Остался {0} заказ", "Осталось {0} заказа", "Осталось {0} заказов");
			}
		}

		private void UpdateRoutesPixBuf()
		{
			if(_pixbufMarkers != null && ViewModel.RoutesOnDay.Count == _pixbufMarkers.Length)
			{
				return;
			}

			_pixbufMarkers = new Pixbuf[ViewModel.RoutesOnDay.Count];
			for(int i = 0; i < ViewModel.RoutesOnDay.Count; i++)
			{
				PointMarkerShape shape = ViewModel.GetMarkerShapeFromBottleQuantity(ViewModel.RoutesOnDay[i].TotalFullBottlesToClient);
				_pixbufMarkers[i] = PointMarker.GetIconPixbuf(ViewModel.GetAddressMarker(i).ToString(), shape);
			}
		}

		private void RoutesWasUpdated()
		{
			ViewModel.HasNoChanges = false;
			ytreeRoutes.YTreeModel.EmitModelChanged();
		}

		private void UpdateRoutesButton()
		{
			var menu = new Menu();
			foreach(var route in ViewModel.RoutesOnDay)
			{
				var carrierInfo = string.Format("№{0} - {1}", route.Id, route.Driver.ShortName);
				if(route.GeographicGroups.Any())
				{
					carrierInfo = string.Concat(carrierInfo, " (", route.GeographicGroups.First().Name, ')');
				}

				carrierInfo = string.Concat(
					carrierInfo,
					string.Format("; {0} кг; {1} куб.м.", route.Car?.CarModel?.MaxWeight, route.Car?.CarModel?.MaxVolume)
				);
				var item = new MenuItemId<RouteList>(carrierInfo)
				{
					ID = route
				};
				item.Activated += AddToRLItem_Activated;
				menu.Append(item);
			}
			menu.ShowAll();
			menuAddToRL.Menu = menu;
		}

		private void AddToRLItem_Activated(object sender, EventArgs e)
		{
			bool ordersAdded = false;
			RouteList routeList = ((MenuItemId<RouteList>)sender).ID;
			try
			{
				ordersAdded = ViewModel.AddOrdersToRouteList(GetSelectedOrders(), routeList);
			}
			catch(Exception ex)
			{
				MessageDialogHelper.RunErrorDialog(
					"Возникла ошибка при добавлении адресов, возможно из-за одновременного добавления одного адреса несколькими пользователями.\n" +
					"Данные для формирования будут автоматически обновлены для продолжения работы.\n" +
					"Повторите попытку добавления адресов.\n" +
					$"Текст ошибки: {ex.Message}", "Ошибка при добавлении адресов"
				);
				Refresh();
				return;
			}
			if(ordersAdded)
			{
				UpdateAddressesOnMap();
				UpdateMarkersInDriverDistricts(routeList.Driver);
				ytreeRoutes.YTreeModel.EmitModelChanged();
				TurnOffCheckShowOnlyDriverOrders();
				ViewModel.RouteListProfitabilityController.ReCalculateRouteListProfitability(ViewModel.UoW, routeList);
			}
		}

		private void TurnOffCheckShowOnlyDriverOrders()
		{
			if(checkShowOnlyDriverOrders.Active)
			{
				checkShowOnlyDriverOrders.Active = false;
				_driverDistrictsOverlay.IsVisibile = false;
				_driverAddressesOverlay.IsVisibile = false;
				_addressesOverlay.IsVisibile = true;
			}
			_routeOverlay.Clear();
		}

		private IList<OrderOnDayNode> GetSelectedOrders()
		{
			var orders = new List<OrderOnDayNode>();
			//Добавление заказов из кликов по маркеру
			var selectedOrderMarkers = _selectedMarkers
				.Select(m => m.Tag).OfType<OrderOnDayNode>()
				.ToList();
			orders.AddRange(selectedOrderMarkers);
			//Добавление заказов из квадратного выделения
			var squareSelectionOrdersIds = _addressesOverlay.Markers
				.Where(m => gmapWidget.SelectedArea.Contains(m.Position))
				.Select(x => x.Tag).OfType<OrderOnDayNode>()
				.ToList();
			orders.AddRange(squareSelectionOrdersIds);
			//Добавление закзаов через непрямоугольную область
			GMapOverlay overlay = gmapWidget.Overlays.FirstOrDefault(o => o.Id.Contains(_selectionOverlay.Id));
			GMapPolygon polygons = overlay?.Polygons.FirstOrDefault(p => p.Name.ToLower().Contains("выделение"));
			if(polygons != null)
			{
				var rectangleSelectionOrdersIds = _addressesOverlay.Markers
					.Where(m => polygons.IsInside(m.Position))
					.Select(x => x.Tag).OfType<OrderOnDayNode>()
					.ToList();
				orders.AddRange(rectangleSelectionOrdersIds);
			}

			return orders;
		}

		protected void FillItems()
		{
			if(ViewModel.DateForRouting != default(DateTime))
			{
				FillDialogAtDay();
			}
		}

		private void LoadDistrictsGeometry()
		{
			logger.Info("Загружаем районы...");
			_districtsOverlay.Clear();
			ViewModel.LogisticanDistricts = ViewModel.ScheduleRestrictionRepository.GetDistrictsWithBorder(ViewModel.UoW);
			foreach(var district in ViewModel.LogisticanDistricts)
			{
				var poligon = new GMapPolygon(
					district.DistrictBorder.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(),
					district.DistrictName
				);
				_districtsOverlay.Polygons.Add(poligon);
			}
			logger.Info("Ок.");
		}

		private void LoadDriverDistrictsGeometry(Employee driver)
		{
			if(driver != ViewModel.DriverFromRouteList)
			{
				_driverDistrictsOverlay.Clear();

				var driverDistricts = driver.DriverDistrictPrioritySets
					.SingleOrDefault(x => x.IsActive)
					?.DriverDistrictPriorities.Select(x => x.District)
					.ToList();

				if(driverDistricts == null || !driverDistricts.Any())
				{
					return;
				}

				foreach(var district in driverDistricts)
				{
					var poligon = new GMapPolygon(
						district.DistrictBorder.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(),
						district.DistrictName
					);
					switch(driverDistricts.IndexOf(district) + 1)
					{
						case 1:
							poligon.Fill = new SolidBrush(System.Drawing.Color.FromArgb(155, System.Drawing.Color.LightGreen));
							break;
						case 2:
							poligon.Fill = new SolidBrush(System.Drawing.Color.FromArgb(155, System.Drawing.Color.Yellow));
							break;
						case 3:
							poligon.Fill = new SolidBrush(System.Drawing.Color.FromArgb(155, System.Drawing.Color.Orange));
							break;
					}
					_driverDistrictsOverlay.Polygons.Add(poligon);
				}
			}
		}

		private void ShowOrdersInDriverDistricts(Employee driver)
		{
			if(driver != ViewModel.DriverFromRouteList)
			{
				UpdateMarkersInDriverDistricts(driver);
				ViewModel.DriverFromRouteList = driver;
			}

			if(ViewModel.ShowOnlyDriverOrders)
			{
				_driverDistrictsOverlay.IsVisibile = true;
				_driverAddressesOverlay.IsVisibile = true;
				_addressesOverlay.IsVisibile = false;
			}
			else
			{
				_driverDistrictsOverlay.IsVisibile = false;
				_driverAddressesOverlay.IsVisibile = false;
				_addressesOverlay.IsVisibile = true;
			}
		}

		private void UpdateMarkersInDriverDistricts(Employee driver)
		{
			_driverAddressesOverlay.Clear();

			foreach(var b in ViewModel.GeographicGroupsExceptEast)
			{
				_driverAddressesOverlay.Markers.Add(FillBaseMarker(b));
			}

			var driverAddresses = ViewModel.RoutesOnDay
				.Where(r => r.Driver.Id == driver.Id)
				.SelectMany(x => x.Addresses)
				.Select(a => new
				{
					Order = new OrderOnDayNode
					{
						OrderId = a.Order.Id,
						OrderStatus = a.Order.OrderStatus,
						DeliveryPointLatitude = a.Order.DeliveryPoint.Latitude,
						DeliveryPointLongitude = a.Order.DeliveryPoint.Longitude,
						DeliveryPointShortAddress = a.Order.DeliveryPoint.ShortAddress,
						DeliveryPointCompiledAddress = a.Order.DeliveryPoint.CompiledAddress,
						DeliveryPointNetTopologyPoint = a.Order.DeliveryPoint.NetTopologyPoint,
						DeliveryPointDistrictId = a.Order.DeliveryPoint.District.Id,
						LogisticsRequirements = a.Order.LogisticsRequirements,
						OrderAddressType = a.Order.OrderAddressType,
						DeliverySchedule = a.Order.DeliverySchedule,
						Total19LBottlesToDeliver = a.Order.Total19LBottlesToDeliver,
						Total6LBottlesToDeliver = a.Order.Total6LBottlesToDeliver,
						Total600mlBottlesToDeliver = a.Order.Total600mlBottlesToDeliver,
						BottlesReturn = a.Order.BottlesReturn,
						OrderComment = a.Order.Comment,
						DeliveryPointComment = a.Order.DeliveryPoint.Comment,
						CommentManager = a.Order.CommentManager,
						ODZComment = a.Order.ODZComment,
						OPComment = a.Order.OPComment,
						DriverMobileAppComment = a.Order.DriverMobileAppComment
					},
					RouteList = a.RouteList,
					Total19LBottlesToDeliver = a.Order.Total19LBottlesToDeliver
				})
				.ToList();

			// добавляем маркеры заказов из маршрутников водителя
			if(driverAddresses.Any())
			{
				foreach(var address in driverAddresses)
				{
					var addressMarker = FillAddressMarker(address.Order,
						ViewModel.GetAddressMarker(ViewModel.RoutesOnDay.IndexOf(address.RouteList)),
						ViewModel.GetMarkerShapeFromBottleQuantity(address.Order.Total19LBottlesToDeliver),
						_driverAddressesOverlay,
						address.RouteList);

					_driverAddressesOverlay.Markers.Add(addressMarker);
				}
			}

			var driverDistricts = driver.DriverDistrictPrioritySets
				.SingleOrDefault(x => x.IsActive)
				?.DriverDistrictPriorities.Select(x => x.District.Id)
				.ToList();

			if(driverDistricts == null || !driverDistricts.Any())
			{
				return;
			}

			var ordersOnDay = ViewModel.OrdersOnDay.Select(x => x)
				.Where(x => x.OrderAddressType != OrderAddressType.Service).ToList();
			var ordersRouteLists = ViewModel.OrderRepository.GetAllRouteListsForOrders(ViewModel.UoW, ordersOnDay.Select(o => o.OrderId));

			//добавляем маркеры нераспределенных заказов из районов водителя
			foreach(var order in ordersOnDay)
			{
				var route = ViewModel.RoutesOnDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.OrderId));

				if(order.DeliveryPointLatitude.HasValue && order.DeliveryPointLongitude.HasValue)
				{
					if(!ordersRouteLists.TryGetValue(order.OrderId, out var orderRls))
					{
						orderRls = new List<int>();
					}

					if(driverDistricts.Contains(order.DeliveryPointDistrictId) && route == null)
					{
						FillTypeAndShapeMarker(order, null, orderRls, out PointMarkerShape shape, out PointMarkerType type);
						var addressMarker = FillAddressMarker(order, type, shape, _driverAddressesOverlay, null);
						_driverAddressesOverlay.Markers.Add(addressMarker);
					}
				}
			}
		}

		protected void OnButtonAutoCreateClicked(object sender, EventArgs e)
		{
			if(ViewModel.DateForRouting < DateTime.Today.AddDays(-1) && !ViewModel.CanСreateRoutelistInPastPeriod)
			{
				ViewModel.CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Нельзя создавать маршруты на дату ранее вчерашнего дня!");
				return;
			}

			UpdateWarningButton();

			if(_creatingInProgress)
			{
				buttonAutoCreate.Label = "Создать маршруты";
				return;
			}

			_creatingInProgress = true;
			buttonAutoCreate.Label = "Остановить";

			if(ViewModel.CreateRoutesAutomatically(txt =>
				{
					Gtk.Application.Invoke((s, args) =>
					{
						textOrdersInfo.Buffer.Text = txt;
					});
				}))
			{
				UpdateRoutesPixBuf();
				UpdateRoutesButton();
				UpdateAddressesOnMap();
				RoutesWasUpdated();
				ViewModel.IsAutoroutingModeActive = true;
			}
			UpdateWarningButton();
			_creatingInProgress = false;
			buttonAutoCreate.Label = "Создать маршруты";
		}

		protected void OnButtonDriverSelectAutoClicked(object sender, EventArgs e)
		{
			var driver = ytreeviewOnDayDrivers.GetSelectedObjects<AtWorkDriver>().FirstOrDefault();

			if(driver == null)
			{
				ViewModel.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран водитель!");
				return;
			}

			var page = (ViewModel.NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<CarJournalViewModel, Action<CarJournalFilterViewModel>>(
				Tab,
				filter =>
				{
					filter.Archive = false;
					filter.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company };
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += (o, args) =>
					{
						var car = ViewModel.UoW.GetById<Car>(args.GetSelectedObjects<CarJournalNode>().First().Id);
						ViewModel.SelectCarForDriver(driver, car);
					};
				});
		}

		private void OnLoadTimeEdited(object o, EditedArgs args)
		{
			var routeList = (RouteList)ytreeRoutes.YTreeModel.NodeAtPath(new TreePath(args.Path));
			bool NeedRecalculate = false;

			if(string.IsNullOrWhiteSpace(args.NewText))
			{
				NeedRecalculate = routeList.OnloadTimeFixed;
				routeList.OnloadTimeFixed = false;
			}
			else if(TimeSpan.TryParse(args.NewText, out TimeSpan fixedTime))
			{
				if(fixedTime != routeList.OnLoadTimeStart)
				{
					NeedRecalculate = true;
				}

				routeList.OnloadTimeFixed = true;
				routeList.OnLoadTimeStart = fixedTime;
				routeList.OnLoadTimeEnd = fixedTime.Add(TimeSpan.FromMinutes(routeList.TimeOnLoadMinuts));
			}

			if(NeedRecalculate)
			{
				ViewModel.RecalculateOnLoadTime();
			}
		}

		private void UpdateWarningButton()
		{
			buttonWarnings.Visible = ViewModel.Optimizer.WarningMessages.Any();
			buttonWarnings.Label = ViewModel.Optimizer.WarningMessages.Count.ToString();
		}

		protected void OnFilterWidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress)
			{
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter))
				{
					FillItems();
				}
			}
		}

		public override void Destroy()
		{
			ViewModel.Dispose();
			gmapWidget.Destroy();
			ytreeRoutes.Destroy();
			ytreeviewOnDayDrivers.Destroy();
			ytreeviewOnDayForwarders.Destroy();
			ytreeviewShift.Destroy();
			ytreeviewGeographicGroup.Destroy();
			ytreeviewAddressesTypes.Destroy();
			viewDeliverySummary.Destroy();
			yenumcomboMapType.Destroy();
			enumCmbDeliveryType.Destroy();

			base.Destroy();
		}
	}
}
