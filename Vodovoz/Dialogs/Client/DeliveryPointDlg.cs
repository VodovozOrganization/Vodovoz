using System;
using System.Collections.Generic;
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
			entryPhone.SetDefaultCityCode ("812");
			datatable1.DataSource = subjectAdaptor;
			comboRoomType.ItemsEnum = typeof(RoomType);
			referenceLogisticsArea.SubjectType = typeof(LogisticsArea);
			referenceLogisticsArea.Sensitive = QSMain.User.Permissions ["logistican"];
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			entryPhone.ValidationMode = QSWidgetLib.ValidationType.phone;
			referenceContact.RepresentationModel = new ViewModel.ContactsVM (UoWGeneric, Entity.Counterparty);
			entryCity.FocusOutEvent += FocusOut;
			entryStreet.FocusOutEvent += FocusOut;
			entryRegion.FocusOutEvent += FocusOut;
			entryBuilding.FocusOutEvent += FocusOut;

			comboRoomType.Binding.AddBinding (Entity, entity => entity.RoomType, widget => widget.SelectedItem)
				.InitializeFromSource ();

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

