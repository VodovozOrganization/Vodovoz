using System;
using NLog;
using QS.Project.Dialogs;
using QSOrmProject;
using QSValidation;
using Vodovoz.DocTemplates;
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
			ylabelNumber.Binding.AddBinding(Entity, e => e.FullNumberText, w => w.LabelProp).InitializeFromSource();

			dateIssue.Binding.AddBinding (Entity, e => e.IssueDate, w => w.Date).InitializeFromSource ();
			dateStart.Binding.AddBinding (Entity, e => e.StartDate, w => w.Date).InitializeFromSource ();

			if (Entity.DocumentTemplate == null && Entity.Contract != null)
				Entity.UpdateContractTemplate(UoW);

			if (Entity.DocumentTemplate != null)
				(Entity.DocumentTemplate.DocParser as RepairAgreementParser).RootObject = Entity;

			templatewidget2.CanRevertCommon = QSProjectsLib.QSMain.User.Permissions["can_set_common_additionalagreement"];
			templatewidget2.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget2.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
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

