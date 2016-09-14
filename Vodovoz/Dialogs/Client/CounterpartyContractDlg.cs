using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.DocTemplates;

namespace Vodovoz
{
	public partial class CounterpartyContractDlg : OrmGtkDialogBase<CounterpartyContract>, IEditableDialog, IContractSaved
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public event EventHandler<ContractSavedEventArgs> ContractSaved;

		bool isEditable = true;

		public bool IsEditable { 
			get { return isEditable; } 
			set {
				isEditable = value; 
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

		/// <summary>
		/// Новый договор с заполненной организацией.
		/// </summary>
		public CounterpartyContractDlg (Counterparty counterparty, Organization organization) : this (counterparty)
		{
			UoWGeneric.Root.Organization = organization;
			referenceOrganization.Sensitive = false;
			Entity.UpdateContractTemplate(UoW);
		}

		public CounterpartyContractDlg(Counterparty counterparty, Organization organizetion, DateTime? date):this(counterparty,organizetion){
			if(date.HasValue)
				UoWGeneric.Root.IssueDate = date.Value;
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

			if (Entity.ContractTemplate == null && Entity.Organization != null)
				Entity.UpdateContractTemplate(UoW);

			if (Entity.ContractTemplate != null)
				(Entity.ContractTemplate.DocParser as ContractParser).RootObject = Entity;

			templatewidget1.Binding.AddBinding(Entity, e => e.ContractTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
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

