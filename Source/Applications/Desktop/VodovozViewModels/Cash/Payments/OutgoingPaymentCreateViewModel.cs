using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Organizations;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.Cash.Payments
{
	public class OutgoingPaymentCreateViewModel : EntityTabViewModelBase<OutgoingPayment>
	{
		private FinancialExpenseCategory _financialExpenseCategory;
		private Organization _ourOrganization;
		private Counterparty _counterparty;

		public OutgoingPaymentCreateViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ViewModelEEVMBuilder<FinancialExpenseCategory> financialExpenseCategoryViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Organization> ourOrganizationViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			FinancialExpenseCategoryViewModel = financialExpenseCategoryViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(filter =>
				{
					filter.RestrictFinancialSubtype = FinancialSubType.Expense;
					filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
				})
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.Finish();

			OurOrganizationViewModel = ourOrganizationViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.OurOrganization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			SaveCommand = new DelegateCommand(SaveAndClose, () => PermissionResult.CanCreate);
			CancelCommand = new DelegateCommand(() => Close(PermissionResult.CanCreate && HasChanges, CloseSource.Cancel));
		}

		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }
		public IEntityEntryViewModel OurOrganizationViewModel { get; }

		public Counterparty Counterparty
		{
			get => this.GetIdRefField(ref _counterparty, Entity.CounterpartyId);
			set => this.SetIdRefField(SetField, ref _counterparty, () => Entity.CounterpartyId, value);
		}

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.FinancialExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.FinancialExpenseCategoryId, value);
		}

		public Organization OurOrganization
		{
			get => this.GetIdRefField(ref _ourOrganization, Entity.OrganizationId);
			set => this.SetIdRefField(SetField, ref _ourOrganization, () => Entity.OrganizationId, value);
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public override bool Save(bool close)
		{
			Entity.CreatedAt = DateTime.Now;
			return base.Save(close);
		}
	}
}
