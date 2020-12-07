using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using NLog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.DTO;
using QS.Osm.Loaders;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.Dialogs.Phones;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.JournalFilters;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModel;
using QS.Tdi;
using QSOsm;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories;
using QS.Project.Services;
using Vodovoz.Domain;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Models;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	public partial class DeliveryPointDlg : EntityDialogBase<DeliveryPoint>, IDeliveryPointInfoProvider, ITDICloseControlTab
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger();
		private Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

		IPhoneRepository phoneRepository = new PhoneRepository();

		GMapControl MapWidget;
		readonly GMapOverlay addressOverlay = new GMapOverlay();
		GMapMarker addressMarker;
		public DeliveryPoint DeliveryPoint => Entity;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		string cityBeforeChange = null;
		string streetBeforeChange = null;
		string buildingBeforeChange = null;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.DeliveryPricePanelView };
		public override bool HasChanges {
			get {
				phonesview1.ViewModel.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		public DeliveryPointDlg(Counterparty counterparty)
		{
			this.Build();
			UoWGeneric = DeliveryPoint.CreateUowForNew(counterparty);
			TabName = "Новая точка доставки";
			ConfigureDlg();
		}

		public DeliveryPointDlg(Counterparty counterparty, string address1c, string code1c)
		{
			this.Build();
			UoWGeneric = DeliveryPoint.CreateUowForNew(counterparty);
			TabName = "Новая точка доставки";
			Entity.Address1c = address1c;
			Entity.Code1c = code1c;
			ConfigureDlg();
		}

		public DeliveryPointDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(id);
			ConfigureDlg();
		}

		public DeliveryPointDlg(DeliveryPoint sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;

			entryCity.CitiesDataLoader = new CitiesDataLoader(OsmWorker.GetOsmService());
			entryStreet.StreetsDataLoader = new StreetsDataLoader(OsmWorker.GetOsmService());
			entryBuilding.HousesDataLoader = new HousesDataLoader(OsmWorker.GetOsmService());

			buttonDeleteResponsiblePerson.Sensitive = false;
			ytreeviewResponsiblePersons.ColumnsConfig = FluentColumnsConfig<Contact>.Create()
				.AddColumn("Ответственные лица").AddTextRenderer(x => x.FullName)
				.AddColumn("Телефоны").AddTextRenderer(x => String.Join("\n", x.Phones))
				.Finish();
			ytreeviewResponsiblePersons.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewResponsiblePersons.ItemsDataSource = Entity.ObservableContacts;
			ytreeviewResponsiblePersons.Selection.Changed += YtreeviewResponsiblePersons_Selection_Changed;

			phonesview1.ViewModel = new PhonesViewModel(phoneRepository, UoW, ContactParametersProvider.Instance);
			phonesview1.ViewModel.PhonesList = Entity.ObservablePhones;

			ShowResidue();

			ySpecCmbCategory.ItemsList = UoW.Session.QueryOver<DeliveryPointCategory>().Where(c => !c.IsArchive).List().OrderBy(c => c.Name);
			ySpecCmbCategory.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			comboRoomType.ItemsEnum = typeof(RoomType);
			comboRoomType.Binding.AddBinding(Entity, entity => entity.RoomType, widget => widget.SelectedItem)
				.InitializeFromSource();
			yenumEntranceType.ItemsEnum = typeof(EntranceType);
			yenumEntranceType.Binding.AddBinding(Entity, entity => entity.EntranceType, widget => widget.SelectedItem)
				.InitializeFromSource();
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			referenceDeliverySchedule.Binding.AddBinding(Entity, e => e.DeliverySchedule, w => w.Subject).InitializeFromSource();

			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelCompiledAddress.Binding.AddBinding(Entity, e => e.CompiledAddress, w => w.LabelProp).InitializeFromSource();
			checkIsActive.Binding.AddBinding(Entity, e => e.IsActive, w => w.Active).InitializeFromSource();
			checkIsActive.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_arc_counterparty_and_deliverypoint");
			entryRoom.Binding.AddBinding(Entity, e => e.Room, w => w.Text).InitializeFromSource();
			entryFloor.Binding.AddBinding(Entity, e => e.Floor, w => w.Text).InitializeFromSource();
			entryEntrance.Binding.AddBinding(Entity, e => e.Entrance, w => w.Text).InitializeFromSource();
			spinMinutesToUnload.Binding.AddBinding(Entity, e => e.MinutesToUnload, w => w.ValueAsInt).InitializeFromSource();

			yentryOrganization.Binding.AddBinding(Entity, e => e.Organization, w => w.Text).InitializeFromSource();
			hboxOrganization.Visible = Entity?.Counterparty?.PersonType == PersonType.natural;

			yentryKPP.Binding.AddBinding(Entity, e => e.KPP, w => w.Text).InitializeFromSource();

			yentryAddition.Binding.AddBinding(Entity, e => e.АddressAddition, w => w.Text).InitializeFromSource();

			var filter = new NomenclatureRepFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.AvailableCategories = new NomenclatureCategory[] { NomenclatureCategory.water });
			yEntryRefDefWater.RepresentationModel = new NomenclatureDependsFromVM(filter);
			yEntryRefDefWater.Binding.AddBinding(Entity, e => e.DefaultWaterNomenclature, w => w.Subject).InitializeFromSource();

			#region Оставлено для корректного отображения старых заказов.
			yentryAddress1c.Binding.AddBinding(Entity, e => e.Address1c, w => w.Text).InitializeFromSource();
			yentryAddress1c.Binding.AddBinding(Entity, e => e.Address1c, w => w.TooltipText).InitializeFromSource();
			labelAddress1c.Visible = yentryAddress1c.Visible = !String.IsNullOrWhiteSpace(Entity.Address1c);
			yentryCode1c.Binding.AddBinding(Entity, e => e.Code1c, w => w.Text).InitializeFromSource();
			codeLabel.Visible = hboxCode.Visible = !String.IsNullOrWhiteSpace(Entity.Code1c);
			#endregion
			spinBottlesReserv.Binding.AddBinding(Entity, e => e.BottleReserv, w => w.ValueAsInt).InitializeFromSource();
			ychkAlwaysFreeDelivery.Binding.AddBinding(Entity, e => e.AlwaysFreeDelivery, w => w.Active).InitializeFromSource();
			ychkAlwaysFreeDelivery.Visible = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_free_delivery");
			lblCounterparty.LabelProp = Entity.Counterparty.FullName;
			lblId.LabelProp = Entity.Id.ToString();

			radioFixedPrices.Toggled += OnRadioFixedPricesToggled;
			var nomenclatureParametersProvider = new NomenclatureParametersProvider();
			var nomenclatureRepository = new NomenclatureRepository(nomenclatureParametersProvider);
			var waterFixedPricesGenerator = new WaterFixedPricesGenerator(nomenclatureRepository);
			var nomenclatureFixedPriceFactory = new NomenclatureFixedPriceFactory();
			var fixedPriceController = new NomenclatureFixedPriceController(nomenclatureFixedPriceFactory, waterFixedPricesGenerator);
			var fixedPricesModel = new DeliveryPointFixedPricesModel(UoW, Entity, fixedPriceController);
			var nomSelectorFactory = new NomenclatureSelectorFactory();
			FixedPricesViewModel fixedPricesViewModel = new FixedPricesViewModel(UoW, fixedPricesModel, nomSelectorFactory, this);
			fixedpricesview.ViewModel = fixedPricesViewModel;

			ylabelFoundOnOsm.Binding.AddFuncBinding(Entity,
				entity => entity.CoordinatesExist
				? String.Format("<span foreground='{1}'>{0}</span>", entity.CoordinatesText,
					(entity.FoundOnOsm ? "green" : "blue"))
				: "<span foreground='red'>Не найден на карте.</span>",
				widget => widget.LabelProp)
				.InitializeFromSource();
			ylabelChangedUser.Binding.AddFuncBinding(Entity,
				entity => entity.СoordsLastChangeUser != null ? String.Format("Изменено: {0}", entity.СoordsLastChangeUser.Name) : "Никем не изменялись",
				widget => widget.LabelProp)
				.InitializeFromSource();
			ycheckOsmFixed.Binding.AddBinding(Entity, e => e.IsFixedInOsm, w => w.Active).InitializeFromSource();
			ycheckOsmFixed.Visible = QSMain.User.Admin;

			entryCity.CitySelected += (sender, e) => {
				entryStreet.CityId = entryCity.OsmId;
				entryStreet.Street = string.Empty;
				entryStreet.StreetDistrict = string.Empty;
				entryBuilding.House = string.Empty;
			};

			entryStreet.StreetSelected += (sender, e) => {
				if(string.IsNullOrWhiteSpace(entryStreet.Street)) {
					return;
				}
				entryBuilding.Street = new OsmStreet(-1, entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			};

			entryBuilding.FocusOutEvent += EntryBuilding_FocusOutEvent;

			entryCity.Binding
				.AddSource(Entity)
				.AddBinding(entity => entity.CityDistrict, widget => widget.CityDistrict)
				.AddBinding(entity => entity.City, widget => widget.City)
				.AddBinding(entity => entity.LocalityType, widget => widget.Locality)
				.InitializeFromSource();
			entryStreet.Binding
				.AddSource(Entity)
				.AddBinding(entity => entity.StreetDistrict, widget => widget.StreetDistrict)
				.AddBinding(entity => entity.Street, widget => widget.Street)
				.InitializeFromSource();
			entryBuilding.Binding
				.AddSource(Entity)
				.AddBinding(entity => entity.Building, widget => widget.House)
				.InitializeFromSource();

			chkAddCertificatesAlways.Binding.AddBinding(Entity, e => e.AddCertificatesAlways, w => w.Active).InitializeFromSource();

			cityBeforeChange = entryCity.City;
			streetBeforeChange = entryStreet.Street;
			buildingBeforeChange = entryBuilding.House;

			//make actions menu
			var menu = new Gtk.Menu();
			var menuItem = new Gtk.MenuItem("Открыть контрагента");
			menuItem.Activated += OpenCounterparty;
			menu.Add(menuItem);
			menuActions.Menu = menu;
			menu.ShowAll();

			//Configure map
			MapWidget = new GMapControl {
				MapProvider = GMapProviders.GoogleMap,
				Position = new PointLatLng(59.93900, 30.31646),
				MinZoom = 0,
				MaxZoom = 24,
				Zoom = 9,
				WidthRequest = 450,
				HasFrame = true
			};
			MapWidget.Overlays.Add(addressOverlay);
			MapWidget.ButtonPressEvent += MapWidget_ButtonPressEvent;
			MapWidget.ButtonReleaseEvent += MapWidget_ButtonReleaseEvent;
			MapWidget.MotionNotifyEvent += MapWidget_MotionNotifyEvent;
			rightsidepanel1.Panel = MapWidget;
			rightsidepanel1.PanelOpened += Rightsidepanel1_PanelOpened;
			rightsidepanel1.PanelHided += Rightsidepanel1_PanelHided;
			Entity.PropertyChanged += Entity_PropertyChanged;
			UpdateAddressOnMap();
		}

		void YtreeviewResponsiblePersons_Selection_Changed(object sender, EventArgs e)
		{
			buttonDeleteResponsiblePerson.Sensitive = ytreeviewResponsiblePersons.GetSelectedObjects().Length > 0;
		}

		void MapWidget_MotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
		{
			if(addressMoving)
				addressMarker.Position = MapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
		}

		void MapWidget_ButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 1) {
				addressMoving = false;
				var newPoint = MapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				if(!Entity.ManualCoordinates && Entity.FoundOnOsm) {
					if(!MessageDialogHelper.RunQuestionDialog("Координаты точки установлены по адресу. Вы уверены что хотите установить новые координаты?")) {
						UpdateAddressOnMap();
						return;
					}
				}

				Entity.ManualCoordinates = true;
				WriteCoordinates((decimal)newPoint.Lat, (decimal)newPoint.Lng);
			}
		}

		private bool addressMoving;

		void MapWidget_ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1) {
				var newPoint = MapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				if(addressMarker == null) {
					addressMarker = new GMarkerGoogle(newPoint, GMarkerGoogleType.arrow) {
						ToolTipText = Entity.ShortAddress
					};
					addressOverlay.Markers.Add(addressMarker);
				} else
					addressMarker.Position = newPoint;
				addressMoving = true;
			}
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Entity.GetPropertyName(x => x.Latitude)
				|| e.PropertyName == Entity.GetPropertyName(x => x.Longitude)) {
				UpdateMapPosition();
				UpdateAddressOnMap();
			}
			if(e.PropertyName == Entity.GetPropertyName(x => x.Counterparty) && Entity?.Counterparty != null) {
				if(Entity.Counterparty.PersonType != PersonType.natural)
					yentryOrganization.Text = null;
				hboxOrganization.Visible = Entity.Counterparty.PersonType == PersonType.natural;
			}
			//Необходимо разобраться в каких случаях нужно вызывать событие CurrentObjectChanged т.к оно сильно тормозит диалог
			if(e.PropertyName != Entity.GetPropertyName(x => x.Organization))
				CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));

			if(e.PropertyName == Entity.GetPropertyName(x => x.HaveResidue))
				ShowResidue();
		}

		void ShowResidue()
		{
			ycheckHaveResidue.Visible = Entity.HaveResidue.HasValue;
			ycheckHaveResidue.Active = Entity.HaveResidue.HasValue && Entity.HaveResidue.Value;
		}

		void Rightsidepanel1_PanelHided(object sender, EventArgs e)
		{
			if(TabParent is TdiSliderTab slider)
				slider.IsHideJournal = false;
		}

		void Rightsidepanel1_PanelOpened(object sender, EventArgs e)
		{
			if(TabParent is TdiSliderTab slider)
				slider.IsHideJournal = true;
		}

		void OpenCounterparty(object sender, EventArgs e)
		{
			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Counterparty>(Entity.Counterparty.Id),
				() => new CounterpartyDlg(Entity.Counterparty.Id)
			);
		}

		void EntryBuilding_FocusOutEvent(object o, Gtk.FocusOutEventArgs args)
		{
			bool addressChanged = entryCity.City != cityBeforeChange
							   || entryStreet.Street != streetBeforeChange
							   || entryBuilding.House != buildingBeforeChange;
			if(entryBuilding.OsmCompletion.HasValue) {
				Entity.FoundOnOsm = entryBuilding.OsmCompletion.Value;

				entryBuilding.GetCoordinates(out decimal? longitude, out decimal? latitude);

				if(longitude == null || latitude == null)
					return;
				if(!addressChanged)
					return;

				cityBeforeChange = entryCity.City;
				streetBeforeChange = entryStreet.Street;
				buildingBeforeChange = entryBuilding.House;

				if(!Entity.ManualCoordinates || (Entity.ManualCoordinates && MessageDialogHelper.RunQuestionDialog("Координаты были установлены вручную, заменить их на коордитаты адреса?"))) {
					WriteCoordinates(latitude, longitude);
					Entity.ManualCoordinates = false;
				}

				if(entryBuilding.OsmHouse != null && !String.IsNullOrWhiteSpace(entryBuilding.OsmHouse.Name)) {
					labelHouseName.Visible = true;
					labelHouseName.LabelProp = entryBuilding.OsmHouse.Name;
				} else {
					labelHouseName.Visible = false;
				}
			}
		}

		private void UpdateMapPosition()
		{
			if(Entity.Latitude.HasValue && Entity.Longitude.HasValue) {
				var position = new PointLatLng((double)Entity.Latitude.Value, (double)Entity.Longitude.Value);
				if(!MapWidget.ViewArea.Contains(position)) {
					MapWidget.Position = position;
					MapWidget.Zoom = 15;
				}
			} else {
				MapWidget.Position = new PointLatLng(59.93900, 30.31646);
				MapWidget.Zoom = 9;
			}
		}

		private void UpdateAddressOnMap()
		{
			if(addressMarker != null) {
				addressOverlay.Markers.Clear();
				addressMarker = null;
			}

			if(Entity.Latitude.HasValue && Entity.Longitude.HasValue) {
				addressMarker = new GMarkerGoogle(
									new PointLatLng(
										(double)Entity.Latitude.Value,
										(double)Entity.Longitude.Value
									),
									GMarkerGoogleType.arrow
								) {
					ToolTipText = Entity.ShortAddress
				};
				addressOverlay.Markers.Add(addressMarker);
			}
		}

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения сохранения точки доставки и повторите", "Сохранение...");
			return canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}

		public override bool Save()
		{
			try {
				SetSensetivity(false);
				phonesview1.ViewModel.RemoveEmpty();

				if(!Entity.CoordinatesExist && !MessageDialogHelper.RunQuestionDialog("Адрес точки доставки не найден на карте, вы точно хотите сохранить точку доставки?"))
					return false;

				var valid = new QSValidator<DeliveryPoint>(UoWGeneric.Root);
				if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
					return false;
				if(Entity.District == null && !MessageDialogHelper.RunWarningDialog(
									"Проверьте координаты!",
									"Район доставки не найден. Это приведёт к невозможности отображения заказа на эту точку доставки у логистов при составлении маршрутного листа. Укажите правильные координаты.\nПродолжить сохранение точки доставки?",
									Gtk.ButtonsType.YesNo
								))
					return false;

				UoWGeneric.Save();
				return true;
			} finally {
				SetSensetivity(true);
			}
		}

		protected void OnRadoiInformationToggled(object sender, EventArgs e)
		{
			if(radioInformation.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadioCommentsToggled(object sender, EventArgs e)
		{
			if(radioComments.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadioContactsToggled(object sender, EventArgs e)
		{
			if(radioContacts.Active)
				notebook1.CurrentPage = 2;
		}

		private void OnRadioFixedPricesToggled(object sender, EventArgs e)
		{
			if(radioFixedPrices.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnButtonAddResponsiblePersonClicked(object sender, EventArgs e)
		{
			var dlg = new PermissionControlledRepresentationJournal(new ContactsVM(UoW, Entity.Counterparty)) {
				Mode = JournalSelectMode.Multiple
			};
			dlg.ObjectSelected += Dlg_ObjectSelected;
			TabParent.AddSlaveTab(this, dlg);
		}

		void Dlg_ObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedIds = e.GetSelectedIds();
			if(!selectedIds.Any()) {
				return;
			}
			var contacts = UoW.GetById<Contact>(selectedIds).ToList();
			contacts.ForEach(Entity.AddContact);
		}

		protected void OnButtonDeleteResponsiblePersonClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewResponsiblePersons.GetSelectedObjects<Contact>();
			foreach(var toDelete in selected)
				Entity.ObservableContacts.Remove(toDelete);
		}

		protected void OnButtonInsertFromBufferClicked(object sender, EventArgs e)
		{
			bool error = true;

			string booferCoordinates = clipboard.WaitForText();

			string[] coordinates = booferCoordinates?.Split(',');
			if(coordinates?.Length == 2) {
				coordinates[0] = coordinates[0].Replace('.', ',');
				coordinates[1] = coordinates[1].Replace('.', ',');

				bool goodLat = decimal.TryParse(coordinates[0].Trim(), out decimal latitude);
				bool goodLon = decimal.TryParse(coordinates[1].Trim(), out decimal longitude);

				if(goodLat && goodLon) {
					WriteCoordinates(latitude, longitude);
					error = false;
					Entity.ManualCoordinates = true;
				}
			}
			if(error)
				MessageDialogHelper.RunErrorDialog(
					"Буфер обмена не содержит координат или содержит неправильные координаты"
				);
		}

		private void WriteCoordinates(decimal? latitude, decimal? longitude)
		{
			if(EqualCoords(Entity.Latitude, latitude) && EqualCoords(Entity.Longitude, longitude))
				return;

			Entity.SetСoordinates(latitude, longitude, UoW);
			Entity.СoordsLastChangeUser = Repositories.HumanResources.UserRepository.GetCurrentUser(UoW);
		}

		/// <summary>
		/// Сравнивает координаты с точностью 6 знаков после запятой
		/// </summary>
		/// <returns><c>true</c>, Если координаты равны, <c>false</c> иначе.</returns>
		private bool EqualCoords(decimal? coord1, decimal? coord2)
		{
			if(coord1.HasValue && coord2.HasValue) {
				decimal CoordDiff = Math.Abs(coord1.Value - coord2.Value);
				return Math.Round(CoordDiff, 6) == decimal.Zero;
			}

			return false;
		}
	}
}

