using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NLog;
using QSContacts;
using QSOrmProject;
using Vodovoz.Domain;
using QSValidation;

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
			referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, Entity.Counterparty);
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
	}
}

