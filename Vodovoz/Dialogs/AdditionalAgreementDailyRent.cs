using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementDailyRent : OrmGtkDialogBase<DailyRentAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = entryAgreementNumber.Sensitive = 
					dateEnd.Sensitive = dateStart.Sensitive = 
						dailyrentpackagesview1.IsEditable = value;
			} 
		}

		public AdditionalAgreementDailyRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = DailyRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementDailyRent (CounterpartyContract contract, DeliveryPoint point, DateTime IssueDate) : this (contract)
		{
			UoWGeneric.Root.DeliveryPoint = point;
			UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = IssueDate;
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
			dailyrentpackagesview1.IsEditable = true;
			spinRentDays.Sensitive = false;
			referenceDeliveryPoint.Sensitive = false;
			dateIssue.Sensitive = dateStart.Sensitive = false;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, Entity.Contract.Counterparty);
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
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
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

