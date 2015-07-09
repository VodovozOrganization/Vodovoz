using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementFreeRent : OrmGtkDialogBase<FreeRentAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = entryAgreementNumber.Sensitive = 
					dateStart.Sensitive = freerentpackagesview1.IsEditable = value;
			} 
		}

		public AdditionalAgreementFreeRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = FreeRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementFreeRent (CounterpartyContract contract, DeliveryPoint point, DateTime IssueDate) : this (contract)
		{
			UoWGeneric.Root.DeliveryPoint = point;
			UoWGeneric.Root.IssueDate = UoWGeneric.Root.StartDate = IssueDate;
		}

		public AdditionalAgreementFreeRent (FreeRentAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementFreeRent (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FreeRentAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			entryAgreementNumber.IsEditable = true;
			freerentpackagesview1.IsEditable = true;
			referenceDeliveryPoint.Sensitive = false;
			dateIssue.Sensitive = dateStart.Sensitive = false;
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = DeliveryPointRepository
				.DeliveryPointsForCounterpartyQuery (UoWGeneric.Root.Contract.Counterparty)
				.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria;
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - Б";
			freerentpackagesview1.AgreementUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<FreeRentAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			return true;
		}
	}
}

