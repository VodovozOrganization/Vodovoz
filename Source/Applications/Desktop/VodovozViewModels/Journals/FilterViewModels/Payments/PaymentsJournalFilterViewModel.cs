using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Tdi;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Banks.Domain;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Banks;
using Vodovoz.ViewModels.Journals.JournalViewModels.Banks;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Filters.ViewModels
{
	public partial class PaymentsJournalFilterViewModel : FilterViewModelBase<PaymentsJournalFilterViewModel>
	{
		private readonly IPaymentSettings _paymentSettings;
		private readonly ViewModelEEVMBuilder<Organization> _organizationViewModelBuilder;
		private readonly ViewModelEEVMBuilder<Bank> _organizationBankViewModelBuilder;
		private readonly ViewModelEEVMBuilder<Account> _organizationAccountViewModelBuilder;
		private DateTime? _startDate = DateTime.Today.AddDays(-14);
		private DateTime? _endDate;
		private PaymentState? _paymentState;
		private bool _hideCompleted;
		private bool _hideCancelledPayments;
		private bool? _isManuallyCreated;
		private bool _hidePaymentsWithoutCounterparty;
		private bool _hideAllocatedPayments;
		private bool _isSortingDescByUnAllocatedSum;
		private Counterparty _counterparty;
		private Organization _organization;
		private Bank _organizationBank;
		private Account _organizationAccount;		
		private PaymentJournalSortType _sortType;
		private Type _documentType;
		private bool _canChangeDocumentType = true;
		private bool _outgoingPaymentsWithoutCashlessRequestAssigned;

		public PaymentsJournalFilterViewModel(
			ILifetimeScope scope,
			INavigationManager navigationManager,
			ITdiTab journalTab,
			IPaymentSettings paymentSettings,
			ViewModelEEVMBuilder<Organization> organizationViewModelBuilder,
			ViewModelEEVMBuilder<Bank> organizationBankViewModelBuilder,
			ViewModelEEVMBuilder<Account> organizationAccountViewModelBuilder)
		{
			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
			_organizationViewModelBuilder = organizationViewModelBuilder ?? throw new ArgumentNullException(nameof(organizationViewModelBuilder));
			_organizationBankViewModelBuilder =
				organizationBankViewModelBuilder ?? throw new ArgumentNullException(nameof(organizationBankViewModelBuilder));
			_organizationAccountViewModelBuilder =
				organizationAccountViewModelBuilder ?? throw new ArgumentNullException(nameof(organizationAccountViewModelBuilder));
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));

			Initialize();
		}

		private void Initialize()
		{
			InitializeProfitCategories();
			ConfigureEntryViewModels();
		}

		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public ITdiTab JournalTab { get; private set; }

		public IObservableList<SelectableNode<ProfitCategory>> ProfitCategories { get; private set; }

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

		public PaymentState? PaymentState
		{
			get => _paymentState;
			set => UpdateFilterField(ref _paymentState, value);
		}
		
		public Counterparty Counterparty
		{
			get => _counterparty;
			set => UpdateFilterField(ref _counterparty, value);
		}
		
		public Organization Organization
		{
			get => _organization;
			set => UpdateFilterField(ref _organization, value);
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

		public bool HideCompleted
		{
			get => _hideCompleted;
			set => UpdateFilterField(ref _hideCompleted, value);
		}

		public bool HideCancelledPayments
		{
			get => _hideCancelledPayments;
			set => UpdateFilterField(ref _hideCancelledPayments, value);
		}

		public bool? IsManuallyCreated
		{
			get => _isManuallyCreated;
			set => UpdateFilterField(ref _isManuallyCreated, value);
		}
		
		public bool HidePaymentsWithoutCounterparty
		{
			get => _hidePaymentsWithoutCounterparty;
			set => UpdateFilterField(ref _hidePaymentsWithoutCounterparty, value);
		}
		
		public bool HideAllocatedPayments
		{
			get => _hideAllocatedPayments;
			set => UpdateFilterField(ref _hideAllocatedPayments, value);
		}

		public bool IsSortingDescByUnAllocatedSum
		{
			get => _isSortingDescByUnAllocatedSum;
			set => UpdateFilterField(ref _isSortingDescByUnAllocatedSum, value);
		}

		public PaymentJournalSortType SortType
		{
			get => _sortType;
			set => UpdateFilterField(ref _sortType, value);
		}

		public object DocumentTypeObject
		{
			get => DocumentType;
			set
			{
				if(value is Type type)
				{
					DocumentType = type;
				}
				else
				{
					DocumentType = null;
				}
			}
		}

		public Type DocumentType
		{
			get => _documentType;
			set => UpdateFilterField(ref _documentType, value);
		}

		public override bool IsShow { get; set; } = true;

		public bool CanChangeDocumentType
		{
			get => _canChangeDocumentType;
			set => SetField(ref _canChangeDocumentType, value);
		}

		public Type RestrictDocumentType
		{
			get => CanChangeDocumentType ? null : DocumentType;
			set
			{
				if(value is null)
				{
					CanChangeDocumentType = true;
				}
				else
				{
					CanChangeDocumentType = false;
					DocumentType = value;
				}
			}
		}

		public bool OutgoingPaymentsWithoutCashlessRequestAssigned
		{
			get => _outgoingPaymentsWithoutCashlessRequestAssigned;
			set => UpdateFilterField(ref _outgoingPaymentsWithoutCashlessRequestAssigned, value);
		}
		
		public IEntityEntryViewModel OrganizationEntryViewModel { get; private set; }
		public IEntityEntryViewModel OrganizationBankEntryViewModel { get; private set; }
		public IEntityEntryViewModel OrganizationAccountEntryViewModel { get; private set; }

		public IEnumerable<int> GetSelectedProfitCategoriesIds()
		{
			return ProfitCategories
				.Where(x => x.Selected)
				.Select(x => x.Value.Id)
				.ToList();
		}
		
		private void InitializeProfitCategories()
		{
			var list = new ObservableList<SelectableNode<ProfitCategory>>();
			var selectableNodes = UoW.GetAll<ProfitCategory>()
				.Where(x => !x.IsArchive)
				.ToList()
				.Select(SelectableNode<ProfitCategory>.Create);
					
			foreach(var node in selectableNodes)
			{
				if(node.Value.Id == _paymentSettings.DefaultProfitCategoryId
					|| node.Value.Id == _paymentSettings.RefundCancelOrderProfitCategoryId)
				{
					node.Selected = true;
				}

				node.SelectChanged += OnProfitCategorySelectChanged;

				list.Add(node);
			}

			ProfitCategories = list;
		}

		private void OnProfitCategorySelectChanged(object sender, SelectionChanged<ProfitCategory> e)
		{
			Update();
		}

		private void ConfigureEntryViewModels()
		{
			var journal = JournalTab as DialogViewModelBase;
			var organizationViewModel = _organizationViewModelBuilder
				.SetViewModel(journal)
				.SetUnitOfWork(UoW)
				.ForProperty(this, x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			organizationViewModel.CanViewEntity = false;
			OrganizationEntryViewModel = organizationViewModel;
			
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
			foreach(var profitCategory in ProfitCategories)
			{
				profitCategory.SelectChanged -= OnProfitCategorySelectChanged;
			}

			base.Dispose();
			JournalTab = null;
		}
	}
}
