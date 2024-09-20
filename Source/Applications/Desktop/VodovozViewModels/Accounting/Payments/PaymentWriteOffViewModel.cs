using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Windows.Input;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.Accounting.Payments
{
	public class PaymentWriteOffViewModel : EntityTabViewModelBase<PaymentWriteOff>
	{
		private readonly IPermissionResult _permissionResult;
		private readonly IGeneralSettings _generalSettings;
		private Counterparty _counterparty;
		private Organization _organization;

		public PaymentWriteOffViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ICurrentPermissionService currentPermissionService,
			IGeneralSettings generalSettings,
			ViewModelEEVMBuilder<FinancialExpenseCategory> financialExpenseCategoryViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));

			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			_permissionResult = currentPermissionService.ValidateEntityPermission(typeof(PaymentWriteOff));

			if(!_permissionResult.CanRead)
			{
				throw new AbortCreatingPageException("У вас нет доступа к этому документу", "Доступ запрещён");
			}

			if(financialExpenseCategoryViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(financialExpenseCategoryViewModelEEVMBuilder));
			}

			FinancialExpenseCategoryViewModel = financialExpenseCategoryViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(filter =>
				{
					filter.RestrictFinancialSubtype = FinancialSubType.Expense;
					filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));

					foreach(var includedId in _generalSettings.PaymentWriteOffAllowedFinancialExpenseCategories)
					{
						filter.IncludeExpenseCategoryIds.Add(includedId);
					}
				})
				.UseViewModelDialog<FinancialExpenseCategoryViewModel>()
				.Finish();

			SaveCommand = new DelegateCommand(SaveHandler, () => CanSave);
			CancelCommand = new DelegateCommand(CancelHandler);
		}

		public bool CanSave => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;

		public Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public IEntityEntryViewModel OrganizationViewModel { get; set; }
		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }
		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }

		private void SaveHandler()
		{
			SaveAndClose();
		}

		private void CancelHandler()
		{
			Close(true, CloseSource.Cancel);
		}
	}
}
