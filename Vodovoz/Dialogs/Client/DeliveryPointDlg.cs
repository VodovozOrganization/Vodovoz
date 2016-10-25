using System;
using System.Collections.Generic;
using Gamma.Utilities;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSOsm.DTO;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class DeliveryPointDlg : OrmGtkDialogBase<DeliveryPoint>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		GMapControl MapWidget;
		readonly GMapOverlay addressOverlay = new GMapOverlay();
		GMapMarker addressMarker;

		public DeliveryPointDlg (Counterparty counterparty)
		{
			this.Build ();
			UoWGeneric = DeliveryPoint.Create (counterparty);
			TabName = "Новая точка доставки";
			ConfigureDlg ();
		}

		public DeliveryPointDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DeliveryPoint> (id);
			ConfigureDlg ();
		}

		public DeliveryPointDlg (DeliveryPoint sub) : this (sub.Id)
		{
		}

		private void ConfigureDlg ()
		{
			entryPhone.ValidationMode = QSWidgetLib.ValidationType.phone;
			entryPhone.Binding.AddBinding(Entity, e => e.Phone, w => w.Text).InitializeFromSource();
			comboRoomType.ItemsEnum = typeof(RoomType);
			comboRoomType.Binding.AddBinding (Entity, entity => entity.RoomType, widget => widget.SelectedItem)
				.InitializeFromSource ();
			referenceLogisticsArea.SubjectType = typeof(LogisticsArea);
			referenceLogisticsArea.Sensitive = QSMain.User.Permissions ["logistican"];
			referenceLogisticsArea.Binding.AddBinding(Entity, e => e.LogisticsArea, w => w.Subject).InitializeFromSource();
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			referenceDeliverySchedule.Binding.AddBinding(Entity, e => e.DeliverySchedule, w => w.Subject).InitializeFromSource();
			referenceContact.RepresentationModel = new ViewModel.ContactsVM (UoWGeneric, Entity.Counterparty);
			referenceContact.Binding.AddBinding(Entity, e => e.Contact, w => w.Subject).InitializeFromSource();
			entryCity.FocusOutEvent += FocusOut;
			entryStreet.FocusOutEvent += FocusOut;
			entryRegion.FocusOutEvent += FocusOut;
			entryBuilding.FocusOutEvent += FocusOut;

			textComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelCompiledAddress.Binding.AddBinding(Entity, e => e.CompiledAddress, w => w.LabelProp).InitializeFromSource();
			checkIsActive.Binding.AddBinding(Entity, e => e.IsActive, w => w.Active).InitializeFromSource();
			entryRegion.Binding.AddBinding(Entity, e => e.Region, w => w.Text).InitializeFromSource();
			entryRoom.Binding.AddBinding(Entity, e => e.Room, w => w.Text).InitializeFromSource();
			spinFloor.Binding.AddBinding(Entity, e => e.Floor, w => w.ValueAsInt).InitializeFromSource();
			spinMinutesToUnload.Binding.AddBinding(Entity, e => e.MinutesToUnload, w => w.ValueAsInt).InitializeFromSource();

			ylabelDistrictOfCity.Binding.AddBinding (Entity, entity => entity.StreetDistrict, widget => widget.LabelProp)
				.InitializeFromSource ();

			yentryAddition.Binding.AddBinding(Entity, e => e.АddressAddition, w => w.Text).InitializeFromSource();

			ylabelFoundOnOsm.Binding.AddFuncBinding (Entity, 
				entity => entity.FoundOnOsm 
				? String.Format("<span foreground='green'>{0}</span>", entity.СoordinatesText) 
				: "<span foreground='red'>Не найден на карте.</span>",
				widget => widget.LabelProp)
				.InitializeFromSource ();
			ycheckOsmFixed.Binding.AddBinding(Entity, e => e.IsFixedInOsm, w => w.Active).InitializeFromSource();
			ycheckOsmFixed.Visible = QSMain.User.Admin;

			entryCity.CitySelected += (sender, e) => {
				entryStreet.CityId = entryCity.OsmId;
				entryStreet.Street=string.Empty;
				entryStreet.StreetDistrict=string.Empty;
			};

			entryStreet.StreetSelected += (sender, e) => {
				entryBuilding.Street = new OsmStreet (-1, entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			};

			entryBuilding.CompletionLoaded += EntryBuilding_Changed;
			entryBuilding.Changed += EntryBuilding_Changed;

			entryCity.Binding
				.AddSource (Entity)
				.AddBinding (entity => entity.CityDistrict, widget => widget.CityDistrict)
				.AddBinding (entity => entity.City, widget => widget.City)
				.AddBinding (entity => entity.LocalityType, widget => widget.Locality) 
				.InitializeFromSource ();
			entryStreet.Binding
				.AddSource (Entity)
				.AddBinding (entity => entity.StreetDistrict, widget => widget.StreetDistrict)
				.AddBinding (entity => entity.Street, widget => widget.Street)
				.InitializeFromSource ();
			entryBuilding.Binding
				.AddSource (Entity)
				.AddBinding (entity => entity.Building, widget => widget.House)
				.InitializeFromSource ();

			//make actions menu
			var menu = new Gtk.Menu ();
			var menuItem = new Gtk.MenuItem ("Открыть контрагента");
			menuItem.Activated += OpenCounterparty;
			menu.Add (menuItem);
			menuActions.Menu = menu;
			menu.ShowAll ();

			//Configure map
			MapWidget = new GMap.NET.GtkSharp.GMapControl();
			MapWidget.MapProvider = GMapProviders.GoogleMap;
			MapWidget.Position = new PointLatLng(59.93900, 30.31646);
			MapWidget.MinZoom = 0;
			MapWidget.MaxZoom = 24;
			MapWidget.Zoom = 9;
			MapWidget.WidthRequest = 450;
			MapWidget.HasFrame = true;
			MapWidget.Overlays.Add(addressOverlay);
			rightsidepanel1.Panel = MapWidget;
			rightsidepanel1.PanelOpened += Rightsidepanel1_PanelOpened;
			rightsidepanel1.PanelHided += Rightsidepanel1_PanelHided;
			Entity.PropertyChanged += Entity_PropertyChanged;
			UpdateAddressOnMap();
		}

		void Entity_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Entity.GetPropertyName(x => x.Latitude)
				|| e.PropertyName == Entity.GetPropertyName(x => x.Longitude))
			{
				UpdateMapPosition();
				UpdateAddressOnMap();
			}
		}

		void Rightsidepanel1_PanelHided (object sender, EventArgs e)
		{
			var slider = TabParent as QSTDI.TdiSliderTab;
			if(slider != null)
			{
				slider.IsHideJournal = false;
			}
		}

		void Rightsidepanel1_PanelOpened (object sender, EventArgs e)
		{
			var slider = TabParent as QSTDI.TdiSliderTab;
			if(slider != null)
			{
				slider.IsHideJournal = true;
			}
		}

		void OpenCounterparty (object sender, EventArgs e)
		{
			TabParent.OpenTab(
				OrmMain.GenerateDialogHashName<Counterparty>(Entity.Counterparty.Id),
				() => new CounterpartyDlg(Entity.Counterparty.Id)
			);
		}

		void FocusOut (object o, Gtk.FocusOutEventArgs args)
		{
			SetLogisticsArea ();
		}

		void EntryBuilding_Changed (object sender, EventArgs e)
		{
			if (entryBuilding.OsmCompletion.HasValue)
			{
				Entity.FoundOnOsm = entryBuilding.OsmCompletion.Value;
				decimal? latitude, longitude;
				entryBuilding.GetCoordinates(out longitude, out latitude);
				Entity.Latitude = latitude;
				Entity.Longitude = longitude;
			}
			if(entryBuilding.OsmHouse != null && !String.IsNullOrWhiteSpace(entryBuilding.OsmHouse.Name))
			{
				labelHouseName.Visible = true;
				labelHouseName.LabelProp = entryBuilding.OsmHouse.Name;
			}
			else
			{
				labelHouseName.Visible = false;
			}
		}

		private void UpdateMapPosition()
		{
			if(Entity.Latitude.HasValue && Entity.Longitude.HasValue)
			{
				MapWidget.Position = new PointLatLng((double)Entity.Latitude.Value, (double)Entity.Longitude.Value);
				MapWidget.Zoom = 16;
			}
			else
			{
				MapWidget.Position = new PointLatLng(59.93900, 30.31646);
				MapWidget.Zoom = 9;
			}
		}

		private void UpdateAddressOnMap()
		{
			if(addressMarker != null)
			{
				addressOverlay.Markers.Clear();
				addressMarker = null;
			}

			if(Entity.Latitude.HasValue && Entity.Longitude.HasValue)
			{
				addressMarker = new GMarkerGoogle(new PointLatLng((double)Entity.Latitude.Value, (double)Entity.Longitude.Value),
					GMarkerGoogleType.arrow);
				addressMarker.ToolTipText = Entity.ShortAddress;
				addressOverlay.Markers.Add(addressMarker);
			}
		}

		public override bool Save ()
		{
			if(!Entity.FoundOnOsm)
			{
				if(!MessageDialogWorks.RunQuestionDialog ("Адрес точки доставки не найден на карте, вы точно хотите сохранить точку доставки?"))
					return false;
			}

			var valid = new QSValidator<DeliveryPoint> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			return true;
		}

		protected void SetLogisticsArea ()
		{
			IList <DeliveryPoint> sameAddress = UoWGeneric.Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.Eq ("Region", UoWGeneric.Root.Region))
				.Add (Restrictions.Eq ("City", UoWGeneric.Root.City))
				.Add (Restrictions.Eq ("Street", UoWGeneric.Root.Street))
				.Add (Restrictions.Eq ("Building", UoWGeneric.Root.Building))
				.Add (Restrictions.IsNotNull ("LogisticsArea"))
				.Add (Restrictions.Not (Restrictions.Eq ("Id", UoWGeneric.Root.Id)))
				.List<DeliveryPoint> ();
			if (sameAddress.Count > 0) {
				UoWGeneric.Root.LogisticsArea = sameAddress [0].LogisticsArea;
			}
		}
	}
}

