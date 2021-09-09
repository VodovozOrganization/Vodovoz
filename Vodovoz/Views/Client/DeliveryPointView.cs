using System;
using System.ComponentModel;
using Gamma.Widgets;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Osm.DTO;
using QS.Tdi;
using QS.Views.GtkUI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class DeliveryPointView : TabViewBase<DeliveryPointViewModel>
	{
		private bool _addressIsMoving;
		private VBox _vboxMap;
		private yEnumComboBox _comboMapType;
		private GMapControl _mapWidget;
		private GMapMarker _addressMarker;
		private string _cityBeforeChange;
		private string _streetBeforeChange;
		private string _buildingBeforeChange;
		private readonly GMapOverlay _addressOverlay = new GMapOverlay();
		private readonly Clipboard _clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		public DeliveryPointView(DeliveryPointViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			notebook1.Binding.AddBinding(ViewModel, vm => vm.CurrentPage, w => w.CurrentPage).InitializeFromSource();
			notebook1.SwitchPage += (o, args) =>
			{
				if(args.PageNum == 1)
				{
					radioFixedPrices.Active = true;
				}
			};
			notebook1.ShowTabs = false;
			buttonSave.Clicked += (sender, args) =>
			{
				deliverypointresponsiblepersonsview1.RemoveEmpty();
				ViewModel.Save(true);
			};
			buttonSave.Binding.AddBinding(ViewModel, vm => vm.IsNotSaving, w => w.Sensitive).InitializeFromSource();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			buttonCancel.Binding.AddBinding(ViewModel, vm => vm.IsNotSaving, w => w.Sensitive).InitializeFromSource();
			buttonInsertFromBuffer.Clicked += (s, a) => ViewModel.SetCoordinatesFromBuffer(_clipboard.WaitForText());
			buttonApplyLimitsToAllDeliveryPointsOfCounterparty.Clicked +=
				(s, a) => ViewModel.ApplyOrderSumLimitsToAllDeliveryPointsOfClient();
			radioInformation.Toggled += RadioInformationOnToggled;
			radioFixedPrices.Toggled += RadioFixedPricesOnToggled;

			#region Address entries

			entryCity.CitiesDataLoader = ViewModel.CitiesDataLoader;
			entryStreet.StreetsDataLoader = ViewModel.StreetsDataLoader;
			entryBuilding.HousesDataLoader = ViewModel.HousesDataLoader;
			entryCity.CitySelected += EntryCityOnCitySelected;
			entryStreet.StreetSelected += EntryStreetOnStreetSelected;
			entryBuilding.FocusOutEvent += EntryBuildingOnFocusOutEvent;
			entryCity.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.CityDistrict, w => w.CityDistrict)
				.AddBinding(e => e.City, w => w.City)
				.AddBinding(e => e.LocalityType, w => w.Locality).InitializeFromSource();
			entryStreet.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.StreetDistrict, w => w.StreetDistrict)
				.AddBinding(e => e.Street, w => w.Street).InitializeFromSource();
			entryBuilding.Binding.AddBinding(ViewModel.Entity, e => e.Building, w => w.House).InitializeFromSource();

			_cityBeforeChange = entryCity.City;
			_streetBeforeChange = entryStreet.Street;
			_buildingBeforeChange = entryBuilding.House;

			#endregion

			phonesview1.ViewModel = ViewModel.PhonesViewModel;

			ySpecCmbCategory.ItemsList = ViewModel.DeliveryPointCategories;
			ySpecCmbCategory.Binding.AddBinding(ViewModel.Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			ycheckHaveResidue.Binding.AddSource(ViewModel.Entity)
				.AddFuncBinding(e => e.HaveResidue.HasValue, w => w.Visible)
				.AddFuncBinding(e => e.HaveResidue.HasValue && e.HaveResidue.Value, w => w.Active)
				.InitializeFromSource();

			comboRoomType.ItemsEnum = typeof(RoomType);
			comboRoomType.Binding.AddBinding(ViewModel.Entity, e => e.RoomType, w => w.SelectedItem).InitializeFromSource();

			yenumEntranceType.ItemsEnum = typeof(EntranceType);
			yenumEntranceType.Binding.AddBinding(ViewModel.Entity, e => e.EntranceType, w => w.SelectedItem).InitializeFromSource();

			entryDefaultDeliverySchedule.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryScheduleSelectorFactory);
			entryDefaultDeliverySchedule.Binding.AddBinding(ViewModel.Entity, e => e.DeliverySchedule, w => w.Subject).InitializeFromSource();

			checkIsActive.Binding.AddBinding(ViewModel.Entity, e => e.IsActive, w => w.Active).InitializeFromSource();
			checkIsActive.Binding.AddFuncBinding(ViewModel, vm => vm.CanArchiveDeliveryPoint, w => w.Sensitive).InitializeFromSource();

			textComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelCompiledAddress.Binding.AddBinding(ViewModel.Entity, e => e.CompiledAddress, w => w.LabelProp).InitializeFromSource();
			entryRoom.Binding.AddBinding(ViewModel.Entity, e => e.Room, w => w.Text).InitializeFromSource();
			entryFloor.Binding.AddBinding(ViewModel.Entity, e => e.Floor, w => w.Text).InitializeFromSource();
			entryEntrance.Binding.AddBinding(ViewModel.Entity, e => e.Entrance, w => w.Text).InitializeFromSource();
			spinMinutesToUnload.Binding.AddBinding(ViewModel.Entity, e => e.MinutesToUnload, w => w.ValueAsInt).InitializeFromSource();

			hboxOrganisation.Binding.AddFuncBinding(ViewModel.Entity,
					e => e.Counterparty != null && e.Counterparty.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();
			ylabelOrganisation.Binding.AddFuncBinding(ViewModel.Entity,
					e => e.Counterparty != null && e.Counterparty.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();
			yentryOrganisation.Binding.AddBinding(ViewModel.Entity, e => e.Organization, w => w.Text).InitializeFromSource();
			yentryKPP.Binding.AddBinding(ViewModel.Entity, e => e.KPP, w => w.Text).InitializeFromSource();
			textAddressAddition.Binding.AddBinding(ViewModel.Entity, e => e.АddressAddition, w => w.Buffer.Text).InitializeFromSource();

			entryDefaultWater.SetEntityAutocompleteSelectorFactory(ViewModel.NomenclatureSelectorFactory.GetDefaultWaterSelectorFactory());
			entryDefaultWater.Binding.AddBinding(ViewModel.Entity, e => e.DefaultWaterNomenclature, w => w.Subject).InitializeFromSource();

			#region Оставлено для корректного отображения старых заказов

			yentryAddress1c.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.Address1c, w => w.Text)
				.AddBinding(e => e.Address1c, w => w.TooltipText)
				.InitializeFromSource();
			labelAddress1c.Visible = yentryAddress1c.Visible = !string.IsNullOrWhiteSpace(ViewModel.Entity.Address1c);
			yentryCode1c.Binding.AddBinding(ViewModel.Entity, e => e.Code1c, w => w.Text).InitializeFromSource();
			codeLabel.Visible = hboxCode.Visible = !string.IsNullOrWhiteSpace(ViewModel.Entity.Code1c);

			#endregion

			spinBottlesReserv.Binding.AddBinding(ViewModel.Entity, e => e.BottleReserv, w => w.ValueAsInt).InitializeFromSource();
			ychkAlwaysFreeDelivery.Binding.AddBinding(ViewModel.Entity, e => e.AlwaysFreeDelivery, w => w.Active).InitializeFromSource();
			ychkAlwaysFreeDelivery.Visible = ViewModel.CanSetFreeDelivery;
			lblCounterparty.LabelProp = ViewModel.Entity.Counterparty.FullName;
			lblId.LabelProp = ViewModel.Entity.Id.ToString();

			ylabelFoundOnOsm.Binding.AddFuncBinding(ViewModel.Entity,
				e => e.CoordinatesExist
					? string.Format("<span foreground='{1}'>{0}</span>", e.CoordinatesText, e.FoundOnOsm ? "green" : "blue")
					: "<span foreground='red'>Не найден на карте.</span>",
				w => w.LabelProp).InitializeFromSource();
			ylabelChangedUser.Binding.AddFuncBinding(ViewModel,
				vm => vm.CoordsWasChanged
					? $"Изменено: {vm.CoordsLastChangeUserName}"
					: "Никем не изменялись",
				w => w.LabelProp).InitializeFromSource();
			ycheckOsmFixed.Binding.AddBinding(ViewModel.Entity, e => e.IsFixedInOsm, w => w.Active).InitializeFromSource();
			ycheckOsmFixed.Visible = ViewModel.CurrentUserIsAdmin;

			chkAddCertificatesAlways.Binding.AddBinding(ViewModel.Entity, e => e.AddCertificatesAlways, w => w.Active)
				.InitializeFromSource();

			entryLunchTimeFrom.Binding.AddBinding(ViewModel.Entity, e => e.LunchTimeFrom, w => w.Time).InitializeFromSource();
			entryLunchTimeTo.Binding.AddBinding(ViewModel.Entity, e => e.LunchTimeTo, w => w.Time).InitializeFromSource();

			chkBeforeIntervalDelivery.RenderMode = QS.Widgets.RenderMode.Icon;
			chkBeforeIntervalDelivery.Binding.AddBinding(ViewModel.Entity, e => e.IsBeforeIntervalDelivery, w => w.Active).InitializeFromSource();

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
			_mapWidget.Overlays.Add(_addressOverlay);
			_mapWidget.ButtonPressEvent += MapWidgetOnButtonPressEvent;
			_mapWidget.ButtonReleaseEvent += MapWidgetOnButtonReleaseEvent;
			_mapWidget.MotionNotifyEvent += MapWidgetOnMotionNotifyEvent;

			_vboxMap = new VBox();
			_comboMapType = new yEnumComboBox();
			_comboMapType.ItemsEnum = typeof(MapProviders);
			_comboMapType.SelectedItem = MapProviders.GoogleMap;
			_comboMapType.EnumItemSelected += (sender, args) =>
			{
				_mapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)args.SelectedItem);
			};
			_vboxMap.Add(_comboMapType);
			_vboxMap.SetChildPacking(_comboMapType, false, false, 0, PackType.Start);
			_vboxMap.Add(_mapWidget);
			_vboxMap.ShowAll();

			sidePanelMap.Panel = _vboxMap;
			sidePanelMap.IsHided = false;
			ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
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
				deliverypointresponsiblepersonsview1.ResponsiblePersons = ViewModel.ResponsiblePersons;
			}
			else
			{
				labelLimit.Visible = false;
				hboxLimits.Visible = false;
				deliverypointresponsiblepersonsview1.Visible = false;
				labelResponsiblePersons.Visible = false;
			}
		}

		private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.Entity.Latitude):
				case nameof(ViewModel.Entity.Longitude):
					UpdateMapPosition();
					UpdateAddressOnMap();
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
				if(!ViewModel.DeliveryPoint.ManualCoordinates && ViewModel.DeliveryPoint.FoundOnOsm)
				{
					if(!MessageDialogHelper.RunQuestionDialog(
						"Координаты точки установлены по адресу. Вы уверены что хотите установить новые координаты?"))
					{
						UpdateAddressOnMap();
						return;
					}
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

		#endregion

		#region AddressEntriesEvents

		private void EntryBuildingOnFocusOutEvent(object o, FocusOutEventArgs args)
		{
			var addressChanged = entryCity.City != _cityBeforeChange
			                     || entryStreet.Street != _streetBeforeChange
			                     || entryBuilding.House != _buildingBeforeChange;
			if(!addressChanged || !entryBuilding.OsmCompletion.HasValue)
			{
				return;
			}

			ViewModel.Entity.FoundOnOsm = entryBuilding.OsmCompletion.Value;

			entryBuilding.GetCoordinates(out var longitude, out var latitude);

			_cityBeforeChange = entryCity.City;
			_streetBeforeChange = entryStreet.Street;
			_buildingBeforeChange = entryBuilding.House;

			ViewModel.WriteCoordinates(latitude, longitude, false);

			if(entryBuilding.OsmHouse != null && !string.IsNullOrWhiteSpace(entryBuilding.OsmHouse.Name))
			{
				labelHouseName.Visible = true;
				labelHouseName.LabelProp = entryBuilding.OsmHouse.Name;
			}
			else
			{
				labelHouseName.Visible = false;
			}
		}

		private void EntryStreetOnStreetSelected(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(entryStreet.Street))
			{
				return;
			}

			entryBuilding.Street = new OsmStreet(-1, entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			entryBuilding.House = string.Empty;
		}

		private void EntryCityOnCitySelected(object sender, EventArgs e)
		{
			entryStreet.CityId = entryCity.OsmId;
			entryStreet.Street = string.Empty;
			entryStreet.StreetDistrict = string.Empty;
			entryBuilding.House = string.Empty;
		}

		#endregion

		#region Toggles

		private void RadioInformationOnToggled(object sender, EventArgs e)
		{
			if(radioInformation.Active)
			{
				notebook1.CurrentPage = 0;
			}
		}

		private void RadioFixedPricesOnToggled(object sender, EventArgs e)
		{
			if(radioFixedPrices.Active)
			{
				if(fixedpricesview.ViewModel == null)
				{
					fixedpricesview.ViewModel = ViewModel.FixedPricesViewModel;
				}

				notebook1.CurrentPage = 1;
			}
		}

		#endregion
	}
}
