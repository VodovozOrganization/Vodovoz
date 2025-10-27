using System;
using QS.Banks.Domain;
using QS.Project.Filter;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.ViewModels.Journals.FilterViewModels.Banks;
using Vodovoz.ViewModels.Journals.JournalViewModels.Banks;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Filters.ViewModels
{
	public class BankAccountsMovementsJournalFilterViewModel  : FilterViewModelBase<BankAccountsMovementsJournalFilterViewModel>
	{
		private readonly ViewModelEEVMBuilder<Bank> _organizationBankViewModelBuilder;
		private readonly ViewModelEEVMBuilder<Account> _organizationAccountViewModelBuilder;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _onlyWithDiscrepancies;
		private Bank _organizationBank;
		private Account _organizationAccount;

		public BankAccountsMovementsJournalFilterViewModel(
			ViewModelEEVMBuilder<Bank> organizationBankViewModelBuilder,
			ViewModelEEVMBuilder<Account> organizationAccountViewModelBuilder)
		{
			_organizationBankViewModelBuilder =
				organizationBankViewModelBuilder ?? throw new ArgumentNullException(nameof(organizationBankViewModelBuilder));
			_organizationAccountViewModelBuilder =
				organizationAccountViewModelBuilder ?? throw new ArgumentNullException(nameof(organizationAccountViewModelBuilder));
		}
		
		public ITdiTab JournalTab { get; private set; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}
		
		public Bank OrganizationBank
		{
			get => _organizationBank;
			set => UpdateFilterField(ref _organizationBank, value);
		}
		
		public Account OrganizationAccount
		{
			get => _organizationAccount;
			set => UpdateFilterField(ref _organizationAccount, value);
		}

		public bool OnlyWithDiscrepancies
		{
			get => _onlyWithDiscrepancies;
			set => UpdateFilterField(ref _onlyWithDiscrepancies, value);
		}
		
		public IEntityEntryViewModel OrganizationBankEntryViewModel { get; private set; }
		public IEntityEntryViewModel OrganizationAccountEntryViewModel { get; private set; }

		public void SetParentTab(ITdiTab tab)
		{
			JournalTab = tab;
			ConfigureEntryViewModels();
		}
		
		private void ConfigureEntryViewModels()
		{
			var journal = JournalTab as DialogViewModelBase;
			
			var organizationBankViewModel = _organizationBankViewModelBuilder
				.SetViewModel(journal)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.OrganizationBank)
				.UseViewModelJournalAndAutocompleter<BanksJournalViewModel, BanksJournalFilterViewModel>(f => f.Account = OrganizationAccount)
				.UseViewModelDialog<AccountViewModel>()
				.Finish();

			organizationBankViewModel.CanViewEntity = false;
			OrganizationBankEntryViewModel = organizationBankViewModel;
			
			var organizationAccountViewModel = _organizationAccountViewModelBuilder
				.SetViewModel(journal)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.OrganizationAccount)
				.UseViewModelJournalAndAutocompleter<AccountJournalViewModel, AccountJournalFilterViewModel>(f => f.Bank = OrganizationBank)
				.UseViewModelDialog<AccountViewModel>()
				.Finish();

			organizationAccountViewModel.CanViewEntity = false;
			OrganizationAccountEntryViewModel = organizationAccountViewModel;
		}

		public override void Dispose()
		{
			base.Dispose();
			JournalTab = null;
		}
	}
}
