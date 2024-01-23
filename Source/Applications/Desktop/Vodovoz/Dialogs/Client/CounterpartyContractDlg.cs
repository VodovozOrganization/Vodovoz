using System;
using QS.DomainModel.UoW;
using QS.Dialog;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using QS.Project.Services;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz
{
	public partial class CounterpartyContractDlg : QS.Dialog.Gtk.EntityDialogBase<CounterpartyContract>, IEditableDialog, IContractSaved
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private readonly IDocTemplateRepository _docTemplateRepository = new DocTemplateRepository();

		public event EventHandler<ContractSavedEventArgs> ContractSaved;

		public bool IsEditable { get; set; } = true;

		public CounterpartyContractDlg (Counterparty counterparty)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<CounterpartyContract>();
			UoWGeneric.Root.Counterparty = counterparty;
			UoWGeneric.Root.GenerateSubNumber(counterparty);
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
			Entity.UpdateContractTemplate(UoW, _docTemplateRepository);
		}

		public CounterpartyContractDlg(Counterparty counterparty, PaymentType paymentType, Organization organizetion, DateTime? date):this(counterparty,organizetion){
			var orderOrganizationProviderFactory = new OrderOrganizationProviderFactory();
			var orderOrganizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
			var parametersProvider = new ParametersProvider();
			var orderParametersProvider = new OrderParametersProvider(parametersProvider);
			var cashReceiptRepository = new CashReceiptRepository(ServicesConfig.UnitOfWorkFactory, orderParametersProvider);
			var counterpartyContractRepository = new CounterpartyContractRepository(orderOrganizationProvider, cashReceiptRepository);
			var contractType =  counterpartyContractRepository.GetContractTypeForPaymentType(counterparty.PersonType, paymentType);
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
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<CounterpartyContract> (id);
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

			if (Entity.DocumentTemplate == null && Entity.Organization != null)
			{
				Entity.UpdateContractTemplate(UoW, _docTemplateRepository);
			}

			if (Entity.DocumentTemplate != null)
			{
				(Entity.DocumentTemplate.DocParser as ContractParser).RootObject = Entity;
			}

			templatewidget1.CanRevertCommon = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_common_additionalagreement");
			templatewidget1.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget1.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();

            entryNumber.Sensitive = false;
            dateIssue.Sensitive = false;
            referenceOrganization.Sensitive = false;
            ycomboContractType.Sensitive = false;
        }

		public override bool Save ()
		{
			if(Entity.IssueDate == DateTime.MinValue){
				MessageDialogWorks.RunErrorDialog("Введите дату заключения (дату доставки)");
				return false;
			}

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save ();
			ContractSaved?.Invoke(this, new ContractSavedEventArgs (UoWGeneric.Root));
			return true;
		}
	}
}

