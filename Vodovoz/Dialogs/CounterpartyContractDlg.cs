using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyContractDlg : OrmGtkDialogBase<CounterpartyContract>, IEditableDialog, IContractSaved
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public event EventHandler<ContractSavedEventArgs> ContractSaved;

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value; 
				buttonSave.Sensitive = entryNumber.Sensitive = dateIssue.Sensitive = 
					referenceOrganization.Sensitive = checkArchive.Sensitive = 
						checkOnCancellation.Sensitive = spinDelay.Sensitive = 
							additionalagreementsview1.IsEditable = value;
			}
		}

		public CounterpartyContractDlg (Counterparty counterparty)
		{
			this.Build ();
			UoWGeneric = CounterpartyContract.Create (counterparty);
			TabName = "Новый договор";
			ConfigureDlg ();
		}

		public CounterpartyContractDlg (Counterparty counterparty, Organization organization) : this (counterparty)
		{
			UoWGeneric.Root.Organization = organization;
			referenceOrganization.Sensitive = false;
		}

		public CounterpartyContractDlg(Counterparty counterparty, Organization organizetion, DateTime date):this(counterparty,organizetion){
			UoWGeneric.Root.IssueDate = date;
		}

		public CounterpartyContractDlg (CounterpartyContract sub) : this (sub.Id)
		{
		}

		public CounterpartyContractDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CounterpartyContract> (id);
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			datatable5.DataSource = subjectAdaptor;
			referenceOrganization.SubjectType = typeof(Organization);
			additionalagreementsview1.AgreementUoW = UoWGeneric;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<CounterpartyContract> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			if (ContractSaved != null)
				ContractSaved (this, new ContractSavedEventArgs (UoWGeneric.Root));
			return true;
		}
	}
}

