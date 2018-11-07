using System;
using QS.DomainModel.UoW;
using QS.Dialog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repository.Client;

namespace Vodovoz
{
	public partial class CounterpartyContractDlg : QS.Dialog.Gtk.EntityDialogBase<CounterpartyContract>, IEditableDialog, IContractSaved
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

		public CounterpartyContractDlg(Counterparty counterparty, PaymentType paymentType, Organization organizetion, DateTime? date):this(counterparty,organizetion){
			var contractType =  DocTemplateRepository.GetContractTypeForPaymentType(counterparty.PersonType, paymentType);
			Entity.ContractType = contractType;
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
			checkOnCancellation.Binding.AddBinding (Entity, e => e.OnCancellation, w => w.Active).InitializeFromSource ();
			checkArchive.Binding.AddBinding (Entity, e => e.IsArchive, w => w.Active).InitializeFromSource ();

			dateIssue.Binding.AddBinding (Entity, e => e.IssueDate, w => w.Date).InitializeFromSource ();
			entryNumber.Binding.AddBinding (Entity, e => e.ContractFullNumber, w => w.Text).InitializeFromSource ();
			spinDelay.Binding.AddBinding (Entity, e => e.MaxDelay, w => w.ValueAsInt).InitializeFromSource ();
			ycomboContractType.ItemsEnum = typeof(ContractType);
			ycomboContractType.Binding.AddBinding(Entity, e => e.ContractType, w => w.SelectedItem).InitializeFromSource();

			referenceOrganization.SubjectType = typeof(Organization);
			referenceOrganization.Binding.AddBinding (Entity, e => e.Organization, w => w.Subject).InitializeFromSource ();
			additionalagreementsview1.AgreementUoW = UoWGeneric;

			if (Entity.DocumentTemplate == null && Entity.Organization != null)
				Entity.UpdateContractTemplate(UoW);

			if (Entity.DocumentTemplate != null)
				(Entity.DocumentTemplate.DocParser as ContractParser).RootObject = Entity;

			templatewidget1.CanRevertCommon = QSMain.User.Permissions["can_set_common_additionalagreement"];
			templatewidget1.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
		}

		public override bool Save ()
		{
			if(Entity.IssueDate == DateTime.MinValue){
				MessageDialogWorks.RunErrorDialog("Введите дату заключения (дату доставки)");
				return false;
			}

			var valid = new QSValidator<CounterpartyContract> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save ();
			ContractSaved?.Invoke(this, new ContractSavedEventArgs (UoWGeneric.Root));
			return true;
		}
	}
}

