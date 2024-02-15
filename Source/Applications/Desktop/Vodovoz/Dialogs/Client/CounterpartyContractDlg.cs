﻿using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QSProjectsLib;
using System;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.JournalViewModels;
using Vodovoz.Models;
using Vodovoz.Parameters;

namespace Vodovoz
{
	public partial class CounterpartyContractDlg : QS.Dialog.Gtk.EntityDialogBase<CounterpartyContract>, IEditableDialog, IContractSaved
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private readonly IDocTemplateRepository _docTemplateRepository = new DocTemplateRepository();

		public event EventHandler<ContractSavedEventArgs> ContractSaved;

		public bool IsEditable { get; set; } = true;

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
			entityentryOrganization.Sensitive = false;
			Entity.UpdateContractTemplate(UoW, _docTemplateRepository);
		}

		public CounterpartyContractDlg(Counterparty counterparty, PaymentType paymentType, Organization organizetion, DateTime? date):this(counterparty,organizetion){
			var orderOrganizationProviderFactory = new OrderOrganizationProviderFactory();
			var orderOrganizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
			var parametersProvider = new ParametersProvider();
			var orderParametersProvider = new OrderParametersProvider(parametersProvider);
			var cashReceiptRepository = new CashReceiptRepository(UnitOfWorkFactory.GetDefaultFactory, orderParametersProvider);
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

			var organizationEntryViewModelBuilder = new LegacyEEVMBuilderFactory<CounterpartyContract>(
				this,
				Entity,
				UoW,
				Startup.MainWin.NavigationManager,
				_lifetimeScope);

			var organizationEntryViewModel = organizationEntryViewModelBuilder.ForProperty(x => x.Organization)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.Finish();

			organizationEntryViewModel.CanViewEntity = false;

			entityentryOrganization.ViewModel = organizationEntryViewModel;

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
			entityentryOrganization.Sensitive = false;
            ycomboContractType.Sensitive = false;
        }

		public override bool Save ()
		{
			if(Entity.IssueDate == DateTime.MinValue){
				MessageDialogWorks.RunErrorDialog("Введите дату заключения (дату доставки)");
				return false;
			}

			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save ();
			ContractSaved?.Invoke(this, new ContractSavedEventArgs (UoWGeneric.Root));
			return true;
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}

