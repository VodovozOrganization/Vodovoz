using System;
using System.Collections.Generic;
using NLog;
using QSContacts;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;
using Gamma.ColumnConfig;
using QSTDI;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

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
			datatable1.DataSource = subjectAdaptor;
			personsView.Session = Session;
			if (UoWGeneric.Root.Persons == null)
				UoWGeneric.Root.Persons = new List<Person> ();
			personsView.Persons = UoWGeneric.Root.Persons;
			datepickerIssue.DateChanged += OnIssueDateChanged;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.ClientDeliveryPointsVM (UoW, Entity.Counterparty);

			ytreeDeliveryPoints.ColumnsConfig = FluentColumnsConfig<DeliveryPoint>.Create()
				.AddColumn("Точки доставки").AddTextRenderer(x => x.CompiledAddress).Finish();

			ytreeDeliveryPoints.ItemsDataSource = Entity.ObservableDeliveryPoints;
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

		protected void OnButtonAddDeliveryPointsClicked (object sender, EventArgs e)
		{
			var dlg = new ReferenceRepresentation(new ViewModel.ClientDeliveryPointsVM (UoW, Entity.Counterparty));
			dlg.Mode = OrmReferenceMode.MultiSelect;
			dlg.ObjectSelected += Dlg_ObjectSelected;
			TabParent.AddSlaveTab (this, dlg);
		}

		void Dlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var points = UoW.GetById<DeliveryPoint>(e.GetSelectedIds()).ToList();
			points.ForEach(Entity.AddDeliveryPoint);
		}
			
	}
}

