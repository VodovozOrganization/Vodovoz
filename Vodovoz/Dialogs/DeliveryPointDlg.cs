using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSOsm.DTO;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
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

			ylabelFoundOnOsm.Binding.AddFuncBinding (Entity, 
				entity => entity.FoundOnOsm ? String.Empty : "<span foreground='red'>Не найден на карте.</span>",
				widget => widget.LabelProp)
				.InitializeFromSource ();

			entryCity.CitySelected += (sender, e) => {
				entryStreet.CityId = entryCity.OsmId;
			};

			entryStreet.StreetSelected += (sender, e) => {
				entryBuilding.Street = new OsmStreet (-1, entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			};

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
				Entity.FoundOnOsm = entryBuilding.OsmCompletion.Value;
		}

		public override bool Save ()
		{
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

