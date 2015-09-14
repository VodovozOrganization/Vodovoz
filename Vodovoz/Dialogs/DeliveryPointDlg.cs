using System;
using System.Collections.Generic;
using System.Data.Bindings;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
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
			referenceLogisticsArea.SubjectType = typeof(LogisticsArea);
			referenceLogisticsArea.Sensitive = QSMain.User.Permissions ["logistican"];
			referenceDeliverySchedule.SubjectType = typeof(DeliverySchedule);
			entryPhone.ValidationMode = QSWidgetLib.ValidationType.phone;
			referenceContact.RepresentationModel = new ViewModel.ContactsVM (UoWGeneric, Entity.Counterparty);
			entryCity.FocusOutEvent += FocusOut;
			entryStreet.FocusOutEvent += FocusOut;
			entryRegion.FocusOutEvent += FocusOut;
			entryBuilding.FocusOutEvent += FocusOut;
			entryCity.Binding.AddBinding (Entity, entity => entity.City, widget => widget.Text).InitializeFromSource ();  
		}

		void FocusOut (object o, Gtk.FocusOutEventArgs args)
		{
			SetLogisticsArea ();
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

