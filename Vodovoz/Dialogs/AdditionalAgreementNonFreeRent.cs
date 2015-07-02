using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AdditionalAgreementNonFreeRent : OrmGtkDialogBase<NonfreeRentAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = entryAgreementNumber.Sensitive = 
					dateStart.Sensitive = paidrentpackagesview1.IsEditable = value;
			} 
		}

		public AdditionalAgreementNonFreeRent (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = NonfreeRentAgreement.Create (contract);
			ConfigureDlg ();
		}

		public AdditionalAgreementNonFreeRent (CounterpartyContract contract, DeliveryPoint point) : this (contract)
		{
			UoWGeneric.Root.DeliveryPoint = point;
		}

		public AdditionalAgreementNonFreeRent (NonfreeRentAgreement sub) : this (sub.Id)
		{
		}

		public AdditionalAgreementNonFreeRent (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<NonfreeRentAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			entryAgreementNumber.IsEditable = true;
			referenceDeliveryPoint.Sensitive = false;
			dateIssue.Sensitive = false;
			referenceDeliveryPoint.SubjectType = typeof(DeliveryPoint);
			referenceDeliveryPoint.ItemsCriteria = DeliveryPointRepository
				.DeliveryPointsForCounterpartyQuery (UoWGeneric.Root.Contract.Counterparty)
				.GetExecutableQueryOver (UoWGeneric.Session).RootCriteria;
			dataAgreementType.Text = UoWGeneric.Root.Contract.Number + " - А";

			paidrentpackagesview1.AgreementUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<NonfreeRentAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			logger.Info ("Ok");
			AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			return true;
		}
	}
}

