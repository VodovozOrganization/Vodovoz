using System;
using NLog;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	public partial class RepairAgreementDlg : OrmGtkDialogBase<RepairAgreement>, IAgreementSaved, IEditableDialog
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public event EventHandler<AgreementSavedEventArgs> AgreementSaved;

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value;
				buttonSave.Sensitive = 
					dateIssue.Sensitive = dateStart.Sensitive = value;
			} 
		}

		public RepairAgreementDlg (CounterpartyContract contract)
		{
			this.Build ();
			UoWGeneric = RepairAgreement.Create (contract);
			ConfigureDlg ();
		}

		public RepairAgreementDlg (RepairAgreement sub) : this (sub.Id)
		{
		}

		public RepairAgreementDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RepairAgreement> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidator<RepairAgreement> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доп. соглашение...");
			UoWGeneric.Save ();
			if (AgreementSaved != null)
				AgreementSaved (this, new AgreementSavedEventArgs (UoWGeneric.Root));
			logger.Info ("Ok");
			return true;
		}
	}
}

