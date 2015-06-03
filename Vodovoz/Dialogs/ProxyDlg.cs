using System;
using System.Collections.Generic;
using Gtk;
using NHibernate.Criterion;
using NLog;
using QSContacts;
using QSOrmProject;
using Vodovoz.Domain;

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
			datepickerStart.DateChanged += OnStartDateChanged;
			datepickerExpiration.DateChanged += OnExpirationDateChanged;
			referenceDeliveryPoint.ParentReference = new OrmParentReference (Session, UoWGeneric.Root.Counterparty, "DeliveryPoints");

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
				UoWGeneric.Root.StartDate == default(DateTime) && datepickerStart.Date < datepickerIssue.Date)
				datepickerStart.Date = datepickerIssue.Date;
		}

		private void OnStartDateChanged (object sender, EventArgs e)
		{
			if (datepickerStart.Date < datepickerIssue.Date) {
				datepickerStart.Date = datepickerIssue.Date;
				string Message = "Нельзя установить дату начала действия доверенности раньше даты ее выдачи.";
				MessageDialog md = new MessageDialog ((Window)this.Toplevel, DialogFlags.Modal,
					                   MessageType.Warning, 
					                   ButtonsType.Close,
					                   Message);
				md.Run ();
				md.Destroy ();
			}
			if (datepickerStart.Date != default(DateTime) && datepickerExpiration.Date < datepickerStart.Date)
				datepickerExpiration.Date = datepickerStart.Date;
		}

		private void OnExpirationDateChanged (object sender, EventArgs e)
		{
			if (datepickerExpiration.Date < datepickerStart.Date) {
				datepickerExpiration.Date = datepickerStart.Date;
				string Message = "Нельзя установить дату окончания действия доверенности раньше даты начала ее действия.";
				MessageDialog md = new MessageDialog ((Window)this.Toplevel, DialogFlags.Modal,
					                   MessageType.Warning, 
					                   ButtonsType.Close,
					                   Message);
				md.Run ();
				md.Destroy ();
			}
		}

		public override bool Save ()
		{
			logger.Info ("Сохраняем доверенность...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}
	}
}

