using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyContractDlg : OrmGtkDialogBase<CounterpartyContract>, IEditable
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		protected IContractOwner ContractOwner;

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
			additionalagreementsview1.ParentReference = new OrmParentReference (UoWGeneric.Session, (Subject as CounterpartyContract), "AdditionalAgreements");
		}

		public override bool Save ()
		{
			var valid = new QSValidator<CounterpartyContract> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			return true;
		}

	}
}

