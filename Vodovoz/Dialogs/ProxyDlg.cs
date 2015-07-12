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
			referenceDeliveryPoint.ParentReference = new OrmParentReference (UoWGeneric, UoWGeneric.Root.Counterparty, "DeliveryPoints");

			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in UoWGeneric.Root.Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
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

