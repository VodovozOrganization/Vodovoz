using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementDailyRent : OrmGtkDialogBase<DailyRentAgreement>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public AdditionalAgreementDailyRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = DailyRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementDailyRent (DailyRentAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementDailyRent (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<DailyRentAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			entryAgreementNumber.IsEditable = true;
			spinRentDays.Sensitive = false;
			referenceDeliveryPoint.Sensitive = false;
			dateIssue.Sensitive = false;

			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in UoWGeneric.Root.Contract.Counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = Session.CreateCriteria<DeliveryPoint> ()
				.Add (Restrictions.In ("Id", identifiers));
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - А";

			dailyrentpackagesview1.AgreementUoW = UoWGeneric;

			dateEnd.Date = UoWGeneric.Root.StartDate.AddDays (UoWGeneric.Root.RentDays);
		}

		public override bool Save ()
		{
			var valid = new QSValidator<DailyRentAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}

		protected void OnSpinRentDaysValueChanged (object sender, EventArgs e)
		{
			dailyrentpackagesview1.UpdateTotalLabels ();
		}

		protected void OnDateStartDateChanged (object sender, EventArgs e)
		{
			RecalcRentPeriod ();
		}

		protected void OnDateEndDateChanged (object sender, EventArgs e)
		{
			RecalcRentPeriod ();
		}

		protected void RecalcRentPeriod ()
		{
			spinRentDays.Value = (dateEnd.Date.Date - dateStart.Date.Date).Days;
			dailyrentpackagesview1.UpdateTotalLabels ();
		}
	}
}

