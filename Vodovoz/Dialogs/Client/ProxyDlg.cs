using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NLog;
using QSContacts;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProxyDlg : OrmGtkDialogBase<Proxy>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public ProxyDlg (Counterparty counterparty)
		{
			this.Build ();
			UoWGeneric = Proxy.Create (counterparty);
			ConfigureDlg ();
		}

		public ProxyDlg (Proxy sub) : this(sub.Id) {}

		public ProxyDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Proxy> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			entryNumber.IsEditable = true;
			entryNumber.Binding.AddBinding (Entity, e => e.Number, w => w.Text).InitializeFromSource ();

			personsView.Session = UoW.Session;
			if (UoWGeneric.Root.Persons == null)
				UoWGeneric.Root.Persons = new List<Person> ();
			personsView.Persons = UoWGeneric.Root.Persons;

			datepickerIssue.Binding.AddBinding (Entity, e => e.IssueDate, w => w.Date).InitializeFromSource ();
			datepickerIssue.DateChanged += OnIssueDateChanged;
			datepickerStart.Binding.AddBinding (Entity, e => e.StartDate, w => w.Date).InitializeFromSource ();
			datepickerExpiration.Binding.AddBinding (Entity, e => e.ExpirationDate, w => w.Date).InitializeFromSource ();

			buttonDeleteDeliveryPoint.Sensitive = false;

			ytreeDeliveryPoints.ColumnsConfig = FluentColumnsConfig<DeliveryPoint>.Create()
				.AddColumn("Точки доставки").AddTextRenderer(x => x.CompiledAddress).Finish();
			ytreeDeliveryPoints.Selection.Mode 		= Gtk.SelectionMode.Multiple;
			ytreeDeliveryPoints.ItemsDataSource 	= Entity.ObservableDeliveryPoints;
			ytreeDeliveryPoints.Selection.Changed  += YtreeDeliveryPoints_Selection_Changed;
		}

		void YtreeDeliveryPoints_Selection_Changed (object sender, EventArgs e)
		{
			buttonDeleteDeliveryPoint.Sensitive = ytreeDeliveryPoints.GetSelectedObjects().Length > 0;
		}

		private void OnIssueDateChanged (object sender, EventArgs e)
		{
			if (datepickerIssue.Date != default(DateTime) &&
				UoWGeneric.Root.StartDate == default(DateTime) || datepickerStart.Date < datepickerIssue.Date)
				datepickerStart.Date = datepickerIssue.Date;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Proxy> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доверенность...");
			personsView.SaveChanges ();
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}
			
		void Dlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var points = UoW.GetById<DeliveryPoint>(e.GetSelectedIds()).ToList();
			points.ForEach(Entity.AddDeliveryPoint);
		}
			
		protected void OnButtonAddDeliveryPointsClicked (object sender, EventArgs e)
		{
			var dlg = new ReferenceRepresentation(new ViewModel.ClientDeliveryPointsVM (UoW, Entity.Counterparty));
			dlg.Mode = OrmReferenceMode.MultiSelect;
			dlg.ObjectSelected += Dlg_ObjectSelected;
			TabParent.AddSlaveTab (this, dlg);
		}

		protected void OnButtonDeleteDekiveryPointClicked (object sender, EventArgs e)
		{
			var selected = ytreeDeliveryPoints.GetSelectedObjects<DeliveryPoint>();
			foreach (var toDelete in selected)
			{
				Entity.ObservableDeliveryPoints.Remove(toDelete);
			}
		}
	}
}

