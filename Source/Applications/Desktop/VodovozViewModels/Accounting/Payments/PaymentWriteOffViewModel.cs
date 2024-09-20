using QS.Commands;
using QS.DomainModel.Entity;
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
using Vodovoz.ViewModels.Extensions;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ViewModels.Accounting.Payments
{
	public class PaymentWriteOffViewModel : EntityTabViewModelBase<PaymentWriteOff>
	{
		private readonly IPermissionResult _permissionResult;
		private readonly IGeneralSettings _generalSettings;
		private Counterparty _counterparty;
		private Organization _organization;
		private FinancialExpenseCategory _financialExpenseCategory;

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
				.ForProperty(this, x => x.FinancialExpenseCategory)
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

			SetPropertyChangeRelation(
				x => x.CounterpartyId,
				() => Counterparty);

			SetPropertyChangeRelation(
				x => x.OrganizationId,
				() => Organization);

			SetPropertyChangeRelation(
				x => x.FinancialExpenseCategoryId,
				() => FinancialExpenseCategory);

			if(Entity.Id == 0)
			{
				Entity.Date = DateTime.Now;
			}
		}

		public bool CanSave => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;

		public Counterparty Counterparty
		{
			get => this.GetIdRefField(ref _counterparty, Entity.CounterpartyId);
			set => this.SetIdRefField(SetField, ref _counterparty, () => Entity.CounterpartyId, value);
		}

		public Organization Organization
		{
			get => this.GetIdRefField(ref _organization, Entity.OrganizationId);
			set => this.SetIdRefField(SetField, ref _organization, () => Entity.OrganizationId, value);
		}

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.FinancialExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.FinancialExpenseCategoryId, value);
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
