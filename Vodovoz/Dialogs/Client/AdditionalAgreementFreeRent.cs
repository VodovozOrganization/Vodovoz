using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	public partial class AdditionalAgreementFreeRent : OrmGtkDialogBase<FreeRentAgreement>, IAgreementSaved, IEditableDialog
	{
		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = 
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
			freerentpackagesview1.IsEditable = true;
			referenceDeliveryPoint.Sensitive = false;
			dateIssue.Sensitive = dateStart.Sensitive = false;
			referenceDeliveryPoint.RepresentationModel = new ViewModel.DeliveryPointsVM (UoW, Entity.Contract.Counterparty);
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();
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

