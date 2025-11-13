using Gamma.GtkWidgets;
using Gamma.Widgets;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Vodovoz.Additions.Logistic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Dialogs.Counterparties;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class DeliveryPointView : TabViewBase<DeliveryPointViewModel>
	{
		private readonly bool _showMapByDefault = false;

		private bool _addressIsMoving;
		private VBox _vboxMap;
		private HBox _hboxMap;
		private yEnumComboBox _comboMapType;
		private yLabel _districtOnMapLabel;
		private yCheckButton _showDistrictsBordersCheck;
		private GMapControl _mapWidget;
		private GMapMarker _addressMarker;		
		private readonly GMapOverlay _addressOverlay = new GMapOverlay();
		private readonly GMapOverlay _districtsBordersOverlay = new GMapOverlay("district_borders");
		private readonly Clipboard _clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		public DeliveryPointView(DeliveryPointViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			notebook.Binding
				.AddBinding(ViewModel, vm => vm.CurrentPage, w => w.CurrentPage)
				.InitializeFromSource();

			notebook.SwitchPage += (o, args) =>
			{
				if(args.PageNum == 1)
				{
					radioFixedPrices.Active = true;
				}
			};
			notebook.ShowTabs = false;
			buttonSave.Clicked += (sender, args) =>
			{
				ViewModel.Save(true);
			};
			buttonSave.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsInProcess && vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			buttonCancel.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsInProcess, w => w.Sensitive)
				.InitializeFromSource();

			buttonInsertFromBuffer.Clicked += (s, a) => ViewModel.SetCoordinatesFromBuffer(_clipboard.WaitForText());
			buttonInsertFromBuffer.Sensitive = ViewModel.CanEdit;
			buttonApplyLimitsToAllDeliveryPointsOfCounterparty.Clicked +=
				(s, a) => ViewModel.ApplyOrderSumLimitsToAllDeliveryPointsOfClient();
			buttonApplyLimitsToAllDeliveryPointsOfCounterparty.Sensitive = ViewModel.CanEdit;
			
			radioInformation.Binding
				.AddBinding(ViewModel, vm => vm.IsInformationActive, w => w.Active)
				.InitializeFromSource();
			
			radioFixedPrices.Toggled += RadioFixedPricesOnToggled;
			radioFixedPrices.Binding
				.AddBinding(ViewModel, vm => vm.IsFixedPricesActive, w => w.Active)
				.InitializeFromSource();
			
			radioSitesAndApps.Binding
				.AddBinding(ViewModel, vm => vm.IsSitesAndAppsActive, w => w.Active)
				.InitializeFromSource();

			ybuttonOpenOnMap.Binding
				.AddBinding(ViewModel.Entity, e => e.CoordinatesExist, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonOpenOnMap.Clicked += (s, a) => ViewModel.OpenOnMapCommand.Execute();

			#region Address entries

			entryCity.CitiesDataLoader = ViewModel.CitiesDataLoader;
			entryStreet.StreetsDataLoader = ViewModel.StreetsDataLoader;
			entryBuilding.HousesDataLoader = ViewModel.HousesDataLoader;
			entryBuilding.WidthRequest = 200;
			entryCity.CitySelected += EntryCityOnCitySelected;
			entryStreet.StreetSelected += EntryStreetOnStreetSelected;
			entryStreet.FocusOutEvent += EntryStreetOnFocusOutEvent;
			entryBuilding.FocusOutEvent += EntryBuildingOnFocusOutEvent;
			entryCity.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.City, w => w.CityName)
				.AddBinding(e => e.CityFiasGuid, w => w.FiasGuid)
				.AddBinding(e => e.LocalityType, w => w.CityTypeName)
				.AddBinding(e => e.LocalityTypeShort, w => w.CityTypeNameShort)
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryCity.FireCityChange();

			entryStreet.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.Street, w => w.StreetName)
				.AddBinding(e => e.StreetDistrict, w => w.StreetDistrict)
				.AddBinding(e => e.StreetType, w => w.StreetTypeName)
				.AddBinding(e => e.StreetTypeShort, w => w.StreetTypeNameShort)
				.AddBinding(e => e.StreetFiasGuid, w => w.FiasGuid)
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryStreet.FireStreetChange();

			entryBuilding.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.BuildingFiasGuid, w => w.FiasGuid)
				.AddBinding(e => e.Building, w => w.BuildingName)
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ViewModel.CityBeforeChange = entryCity.CityName;
			ViewModel.StreetBeforeChange = entryStreet.StreetName;
			ViewModel.BuildingBeforeChange = entryBuilding.BuildingName;
			ViewModel.EntranceBeforeChange = entryEntrance.Text;

			#endregion

			phonesview1.ViewModel = ViewModel.PhonesViewModel;

			ySpecCmbCategory.ItemsList = ViewModel.DeliveryPointCategories;
			ySpecCmbCategory.Binding
				.AddBinding(ViewModel.Entity, e => e.Category, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ycheckHaveResidue.Binding.AddSource(ViewModel.Entity)
				.AddFuncBinding(e => e.HaveResidue.HasValue, w => w.Visible)
				.AddFuncBinding(e => e.HaveResidue.HasValue && e.HaveResidue.Value, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			comboRoomType.ItemsEnum = typeof(RoomType);
			comboRoomType.Binding
				.AddBinding(ViewModel.Entity, e => e.RoomType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yenumEntranceType.ItemsEnum = typeof(EntranceType);
			yenumEntranceType.Binding
				.AddBinding(ViewModel.Entity, e => e.EntranceType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryDefaultDeliverySchedule.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryScheduleSelectorFactory);
			entryDefaultDeliverySchedule.Binding
				.AddBinding(ViewModel.Entity, e => e.DeliverySchedule, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			checkIsActive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsActive, w => w.Active)
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.CanArchiveDeliveryPoint, w => w.Sensitive)
				.InitializeFromSource();

			textComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			labelCompiledAddress.Binding
				.AddBinding(ViewModel.Entity, e => e.CompiledAddress, w => w.LabelProp)
				.InitializeFromSource();
			entryRoom.Binding
				.AddBinding(ViewModel.Entity, e => e.Room, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entryFloor.Binding
				.AddBinding(ViewModel.Entity, e => e.Floor, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entryEntrance.Binding
				.AddBinding(ViewModel.Entity, e => e.Entrance, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entryEntrance.FocusOutEvent += EntryEntranceOnFocusOutEvent;
			spinMinutesToUnload.Binding
				.AddBinding(ViewModel.Entity, e => e.MinutesToUnload, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			hboxOrganisation.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Counterparty != null && e.Counterparty.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();
			ylabelOrganisation.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Counterparty != null && e.Counterparty.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();
			yentryOrganisation.Binding
				.AddBinding(ViewModel.Entity, e => e.Organization, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yentryKPP.Binding
				.AddBinding(ViewModel.Entity, e => e.KPP, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryDefaultWaterNomenclature.ViewModel = ViewModel.DefaultWaterNomenclatureViewModel;
			entryDefaultWaterNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			#region Оставлено для корректного отображения старых заказов

			yentryAddress1c.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.Address1c, w => w.Text)
				.AddBinding(e => e.Address1c, w => w.TooltipText)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			labelAddress1c.Visible = yentryAddress1c.Visible = !string.IsNullOrWhiteSpace(ViewModel.Entity.Address1c);
			yentryCode1c.Binding
				.AddBinding(ViewModel.Entity, e => e.Code1c, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			codeLabel.Visible = hboxCode.Visible = !string.IsNullOrWhiteSpace(ViewModel.Entity.Code1c);

			#endregion

			spinBottlesReserv.Binding
				.AddBinding(ViewModel.Entity, e => e.BottleReserv, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ychkAlwaysFreeDelivery.Binding
				.AddBinding(ViewModel.Entity, e => e.AlwaysFreeDelivery, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ychkAlwaysFreeDelivery.Visible = ViewModel.CanSetFreeDelivery;
			lblCounterparty.LabelProp = ViewModel.Entity.Counterparty.FullName;
			lblId.LabelProp = ViewModel.Entity.Id.ToString();

			var successTextColor = GdkColors.SuccessText.ToHtmlColor();
			var infoTextColor = GdkColors.InfoText.ToHtmlColor();
			var dangerTextColor = GdkColors.DangerText.ToHtmlColor();

			ylabelFoundOnOsm.Binding.AddFuncBinding(ViewModel.Entity,
				e => e.CoordinatesExist
					? $"<span foreground='{(e.FoundOnOsm ? successTextColor : infoTextColor)}'>{e.CoordinatesText}</span>"
					: $"<span foreground='{dangerTextColor}'>Не найден на карте.</span>",
				w => w.LabelProp).InitializeFromSource();
			ylabelChangedUser.Binding.AddFuncBinding(ViewModel,
				vm => vm.CoordsWasChanged
					? $"Изменено: {vm.CoordsLastChangeUserName}"
					: "Никем не изменялись",
				w => w.LabelProp).InitializeFromSource();
			ycheckOsmFixed.Binding
				.AddBinding(ViewModel.Entity, e => e.IsFixedInOsm, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ycheckOsmFixed.Visible = ViewModel.CurrentUserIsAdmin;

			chkAddCertificatesAlways.Binding
				.AddBinding(ViewModel.Entity, e => e.AddCertificatesAlways, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryLunchTimeFrom.Binding
				.AddBinding(ViewModel.Entity, e => e.LunchTimeFrom, w => w.Time)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entryLunchTimeTo.Binding
				.AddBinding(ViewModel.Entity, e => e.LunchTimeTo, w => w.Time)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			#region Вкладка Сайты и приложения
			
			textViewOnlineComment.Binding
				.AddBinding(ViewModel.Entity, e => e.OnlineComment, w => w.Buffer.Text)
				.InitializeFromSource();

			entryIntercom.Binding
				.AddBinding(ViewModel.Entity, e => e.Intercom, w => w.Text)
				.InitializeFromSource();

			#endregion

			//make actions menu
			var menu = new Menu();
			var openClientItem = new MenuItem("Открыть контрагента");
			openClientItem.Activated += (s, a) => ViewModel.OpenCounterpartyCommand.Execute();
			menu.Add(openClientItem);
			menuActions.Menu = menu;
			menu.ShowAll();

			//Configure map
			_mapWidget = new GMapControl
			{
				MapProvider = GMapProviders.GoogleMap,
				Position = new PointLatLng(59.93900, 30.31646),
				MinZoom = 0,
				MaxZoom = 24,
				Zoom = 9,
				WidthRequest = 500,
				HasFrame = true
			};

			_districtsBordersOverlay.IsVisibile = false;
			_mapWidget.Overlays.Add(_districtsBordersOverlay);
			_mapWidget.Overlays.Add(_addressOverlay);			
			_mapWidget.ButtonPressEvent += MapWidgetOnButtonPressEvent;
			_mapWidget.ButtonReleaseEvent += MapWidgetOnButtonReleaseEvent;
			_mapWidget.MotionNotifyEvent += MapWidgetOnMotionNotifyEvent;

			_mapWidget.OnPolygonEnter += OnMapWidgetPolygonEnter;
			_mapWidget.OnPolygonLeave += OnMapWidgetPolygonLeave;
			_mapWidget.Sensitive = ViewModel.CanEdit;

			_vboxMap = new VBox();
			_hboxMap = new HBox();
	
			_vboxMap.Add(_hboxMap);

			_comboMapType = new yEnumComboBox();
			_comboMapType.ItemsEnum = typeof(MapProviders);
			_comboMapType.SelectedItem = MapProviders.GoogleMap;

			_comboMapType.EnumItemSelected += (sender, args) =>
			{
				_mapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
				RefreshDistrictsBorders();
			};

			_hboxMap.Add(_comboMapType);
			_hboxMap.SetChildPacking(_comboMapType, false, false, 0, PackType.Start);

			_showDistrictsBordersCheck = new yCheckButton();
			_showDistrictsBordersCheck.Label = "Показывать логистические районы";
			_showDistrictsBordersCheck.Binding
				.AddBinding(ViewModel, vm => vm.ShowDistrictBorders, w => w.Active)
				.InitializeFromSource();
			_showDistrictsBordersCheck.Toggled += OnShowDistrictsBordersToggled;

			_hboxMap.Add(_showDistrictsBordersCheck);
			_vboxMap.SetChildPacking(_hboxMap, false, false, 0, PackType.Start);	
			_vboxMap.Add(_mapWidget);

			_districtOnMapLabel = new yLabel();
			_districtOnMapLabel.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DistrictOnMapText, w => w.Text)
				.AddBinding(vm => vm.ShowDistrictBorders, w => w.Visible)
				.InitializeFromSource();
			_vboxMap.Add(_districtOnMapLabel);
			_vboxMap.SetChildPacking(_districtOnMapLabel, false, false, 0, PackType.Start);

			_vboxMap.ShowAll();

			sidePanelMap.Panel = _vboxMap;
			sidePanelMap.IsHided = !_showMapByDefault;
			ViewModel.Entity.PropertyChanged += ViewModelOnPropertyChanged;
			UpdateAddressOnMap();

			if(ViewModel.DeliveryPoint.Counterparty.IsForRetail)
			{
				ySpinLimitMin.ValueAsInt = int.MinValue;
				ySpinLimitMax.ValueAsInt = int.MaxValue;

				ySpinLimitMin.Sensitive = ViewModel.CanEditOrderLimits;
				ySpinLimitMax.Sensitive = ViewModel.CanEditOrderLimits;

				ySpinLimitMin.Binding.AddBinding(ViewModel.Entity, e => e.MinimalOrderSumLimit, w => w.ValueAsInt).InitializeFromSource();
				ySpinLimitMax.Binding.AddBinding(ViewModel.Entity, e => e.MaximalOrderSumLimit, w => w.ValueAsInt).InitializeFromSource();

				//FIXME этот виджет следовало бы переписать на VM + V
				deliverypointresponsiblepersonsview1.UoW = ViewModel.UoW;
				deliverypointresponsiblepersonsview1.DeliveryPoint = ViewModel.DeliveryPoint;
				deliverypointresponsiblepersonsview1.ResponsiblePersons = ViewModel.DeliveryPoint.ResponsiblePersons;
				deliverypointresponsiblepersonsview1.Sensitive = ViewModel.CanEdit;
			}
			else
			{
				labelLimit.Visible = false;
				hboxLimits.Visible = false;
				deliverypointresponsiblepersonsview1.Visible = false;
				labelResponsiblePersons.Visible = false;
			}

			logisticsRequirementsView.ViewModel = ViewModel.LogisticsRequirementsViewModel;

			RefreshDistrictsBorders();
		}

		private void RefreshDistrictsBorders()
		{
			_districtsBordersOverlay.Clear();
			District acurateDistrict = null;

			if(ViewModel.Entity.Latitude.HasValue && ViewModel.Entity.Longitude.HasValue)
			{
				acurateDistrict = ViewModel.GetAccurateDistrict();
			}

			foreach(var district in ViewModel.AllActiveDistrictsWithBorders)
			{
				if(district.DistrictBorder != null)
				{
					Color color;
					int alpha;
					Pen pen;
					int borderWidth;

					if(district == acurateDistrict)
					{
						color = Color.Red;
						alpha = 50;
						borderWidth = 2;
					}
					else
					{
						color = Color.Blue;
						alpha = 30;
						borderWidth = 1;
					}

					pen = new Pen(color, borderWidth);

					var coordinates = district.DistrictBorder.Coordinates.ToPointLatLng();

					var polygon = new GMapPolygon(coordinates, district.DistrictName)
					{
						Fill = new SolidBrush(Color.FromArgb(alpha, color)),
						Stroke = pen,
						IsHitTestVisible = true
					};

					_districtsBordersOverlay.Polygons.Add(polygon);
				}
			}
		}

		private void OnShowDistrictsBordersToggled(object sender, EventArgs e)
		{
			_districtsBordersOverlay.IsVisibile = ViewModel.ShowDistrictBorders;
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.Entity.Latitude):
				case nameof(ViewModel.Entity.Longitude):
					UpdateMapPosition();
					UpdateAddressOnMap();
					RefreshDistrictsBorders();
					break;
				case nameof(ViewModel.Entity.Counterparty) when ViewModel.Entity?.Counterparty != null:
				{
					if(ViewModel.Entity.Counterparty.PersonType != PersonType.natural)
					{
						yentryOrganisation.Text = null;
					}

					break;
				}
			}
		}

		#region MapWidgetEvents

		private void UpdateMapPosition()
		{
			if(ViewModel.Entity.Latitude.HasValue && ViewModel.Entity.Longitude.HasValue)
			{
				var position = new PointLatLng((double) ViewModel.Entity.Latitude.Value, (double) ViewModel.Entity.Longitude.Value);
				if(!_mapWidget.ViewArea.Contains(position))
				{
					_mapWidget.Position = position;
					_mapWidget.Zoom = 15;
				}
			}
			else
			{
				_mapWidget.Position = new PointLatLng(59.93900, 30.31646);
				_mapWidget.Zoom = 9;
			}
		}

		private void UpdateAddressOnMap()
		{
			if(_addressMarker != null)
			{
				_addressOverlay.Markers.Clear();
				_addressMarker = null;
			}

			if(ViewModel.Entity.Latitude.HasValue && ViewModel.Entity.Longitude.HasValue)
			{
				_addressMarker = new GMarkerGoogle(
					new PointLatLng((double) ViewModel.Entity.Latitude.Value, (double) ViewModel.Entity.Longitude.Value),
					GMarkerGoogleType.arrow)
				{
					ToolTipText = ViewModel.Entity.ShortAddress
				};

				_addressOverlay.Markers.Add(_addressMarker);
			}
		}

		private void MapWidgetOnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
		{
			if(_addressIsMoving)
			{
				_addressMarker.Position = _mapWidget.FromLocalToLatLng((int) args.Event.X, (int) args.Event.Y);
			}
		}

		private void MapWidgetOnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				_addressIsMoving = false;
				var newPoint = _mapWidget.FromLocalToLatLng((int) args.Event.X, (int) args.Event.Y);

				string message = string.Empty;

				if(!ViewModel.DeliveryPoint.ManualCoordinates && ViewModel.DeliveryPoint.FoundOnOsm)
				{
					message = "Координаты точки установлены по адресу. Вы уверены что хотите установить новые координаты?";
				}
				else if(!ViewModel.UoWGeneric.IsNew && ViewModel.Entity.CoordinatesExist)
				{
					message = "Координаты точки доставки уже были установлены. Вы уверены что хотите установить новые координаты?";
				}

				if(!string.IsNullOrEmpty(message) && !MessageDialogHelper.RunQuestionDialog(message))
				{
					UpdateAddressOnMap();
					return;
				}

				ViewModel.WriteCoordinates((decimal) newPoint.Lat, (decimal) newPoint.Lng, true);
			}
		}

		private void MapWidgetOnButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				var newPoint = _mapWidget.FromLocalToLatLng((int) args.Event.X, (int) args.Event.Y);

				if(_addressMarker == null)
				{
					_addressMarker = new GMarkerGoogle(newPoint, GMarkerGoogleType.arrow)
						{ToolTipText = ViewModel.DeliveryPoint.ShortAddress};
					_addressOverlay.Markers.Add(_addressMarker);
				}
				else
				{
					_addressMarker.Position = newPoint;
				}

				_addressIsMoving = true;
			}
		}

		private void OnMapWidgetPolygonLeave(GMapPolygon item)
		{
			if(ViewModel.DistrictOnMapText == item.Name)
			{
				ViewModel.DistrictOnMapText = string.Empty;
			}
		}

		private void OnMapWidgetPolygonEnter(GMapPolygon item)
		{
			ViewModel.DistrictOnMapText = item.Name;
		}		

		#endregion

		#region AddressEntriesEvents

		private async void EntryBuildingOnFocusOutEvent(object sender, EventArgs e)
		{
			await UpdateCoordinates();
		}

		private async void EntryEntranceOnFocusOutEvent(object sender, EventArgs e)
		{
			await UpdateCoordinates(true);
		}

		private async Task UpdateCoordinates(bool updatedEntrance = false)
		{
			if(!ViewModel.IsAddressChanged)
			{
				return;
			}

			if(ViewModel.Entity.CoordinatesExist
				&& updatedEntrance
				&& !MessageDialogHelper.RunQuestionDialog(
					"В точке доставке установлены координаты\n" +
						"Вы уверены, что хотите обновить координаты, т.к. адрес может быть не найден и они слетят?"))
			{
				ViewModel.ResetAddressChanges();
				return;
			}

			ViewModel.ResetAddressChanges();
			ViewModel.Entity.FoundOnOsm = entryBuilding.FiasCompletion != null && entryBuilding.FiasCompletion.Value;
			entryBuilding.GetCoordinates(out var longitude, out var latitude);
			DeliveryPointViewModel.Coordinate coordinate = new DeliveryPointViewModel.Coordinate
			{
				Latitude = latitude,
				Longitude = longitude
			};

			if(!string.IsNullOrWhiteSpace(entryBuilding.BuildingName)
				&& (!string.IsNullOrWhiteSpace(ViewModel.Entity.Entrance)
					|| longitude == null
					|| latitude == null))
			{
				coordinate = await ViewModel.UpdateCoordinatesFromGeoCoderAsync(entryBuilding.HousesDataLoader);
			}
			
			Gtk.Application.Invoke((o, args) =>
			{
				if(!ViewModel.IsDisposed)
				{
					ViewModel.WriteCoordinates(coordinate.Latitude, coordinate.Longitude, false);
				}
			});
		}

		private void EntryStreetOnStreetSelected(object sender, EventArgs e)
		{
			entryBuilding.StreetGuid = entryStreet.FiasGuid;
			entryBuilding.BuildingName = string.Empty;
		}

		private void EntryStreetOnFocusOutEvent(object sender, EventArgs e)
		{
			if(!ViewModel.IsAddressChanged)
			{
				return;
			}

			ViewModel.ResetAddressChanges();

			entryBuilding.StreetGuid = entryStreet.FiasGuid;

			if(string.IsNullOrWhiteSpace(entryStreet.StreetName))
			{
				entryBuilding.BuildingName = string.Empty;
			}

			if(entryBuilding.StreetGuid == null)
			{
				entryBuilding.FiasGuid = null;
			}

			ViewModel.WriteCoordinates(null, null, false);
		}

		private void EntryCityOnCitySelected(object sender, EventArgs e)
		{
			entryStreet.CityGuid = entryCity.FiasGuid;
			entryStreet.StreetTypeName = string.Empty;
			entryStreet.StreetTypeNameShort = string.Empty;
			entryStreet.StreetName = string.Empty;
			entryStreet.StreetDistrict = string.Empty;
			entryStreet.FireStreetChange();
			entryBuilding.StreetGuid = null;
			entryBuilding.CityGuid = entryCity.FiasGuid;
			entryBuilding.BuildingName = string.Empty;
		}

		#endregion

		#region Toggles

		private void RadioFixedPricesOnToggled(object sender, EventArgs e)
		{
			if(radioFixedPrices.Active)
			{
				if(fixedpricesview.ViewModel == null)
				{
					fixedpricesview.ViewModel = ViewModel.FixedPricesViewModel;
					fixedpricesview.Sensitive = ViewModel.CanEditNomenclatureFixedPrice && ViewModel.CanEdit;
				}
			}
		}

		public override void Destroy()
		{
			_mapWidget.Destroy();
			_showDistrictsBordersCheck.Toggled -= OnShowDistrictsBordersToggled;
			_showDistrictsBordersCheck.Destroy();
			_districtOnMapLabel.Destroy();
			_comboMapType.Destroy();
			_hboxMap.Destroy();
			_vboxMap.Destroy();

			base.Destroy();
		}

		#endregion
	}
}
