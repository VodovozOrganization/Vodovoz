using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets.Cash;
using Vodovoz.ViewModels.Widgets.Organizations;

namespace Vodovoz.ViewModels.Organizations
{
	public class OrganizationViewModel
		: EntityTabViewModelBase<Organization>,
		IAskSaveOnCloseViewModel
	{
		private readonly ILogger<OrganizationViewModel> _logger;
		private readonly IOrganizationVersionsViewModelFactory _organizationVersionsViewModelFactory;
		private readonly IVatRateVersionViewModelFactory _vatRateVersionViewModelFactory;
		private readonly ViewModelEEVMBuilder<FinancialIncomeCategory> _financialIncomeCategoryViewModelEEVMBuilder;

		public OrganizationViewModel(
			ILogger<OrganizationViewModel> logger,
			IOrganizationVersionsViewModelFactory organizationVersionsViewModelFactory,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation, 
			IVatRateVersionViewModelFactory vatRateVersionViewModelFactory,
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			ViewModelEEVMBuilder<FinancialIncomeCategory> financialIncomeCategoryViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_organizationVersionsViewModelFactory = organizationVersionsViewModelFactory
				?? throw new ArgumentNullException(nameof(organizationVersionsViewModelFactory));
			_vatRateVersionViewModelFactory = vatRateVersionViewModelFactory
				?? throw new ArgumentNullException(nameof(vatRateVersionViewModelFactory));
			_financialIncomeCategoryViewModelEEVMBuilder = financialIncomeCategoryViewModelEEVMBuilder
				?? throw new ArgumentNullException(nameof(financialIncomeCategoryViewModelEEVMBuilder));
			
			OrganizationVersionsViewModel = _organizationVersionsViewModelFactory.CreateOrganizationVersionsViewModel(Entity, CanEdit);
			VatRateOrganizationVersionViewModel = _vatRateVersionViewModelFactory.CreateVatRateVersionViewModel(Entity,this, vatRateEevmBuilder, UoW, CanEdit);
			VatRateOrganizationVersionViewModel.IsWidgetVisible = !Entity.IsOsnoMode;

			FinancialIncomeCategoryEntryViewModel = BuildDefaultCashIncomeCategoryEntryViewModel();

			SaveCommand = new DelegateCommand(
				() => Save(true),
				() => CanEdit
			);

			CancelCommand = new DelegateCommand(
				() => Close(CanEdit, CloseSource.Cancel),
				() => CanEdit
			);
			
			RegexForEmailForMailing = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@vodovoz-spb\.ru\z";
		}

		public OrganizationVersionsViewModel OrganizationVersionsViewModel { get; }
		public VatRateOrganizationVersionViewModel VatRateOrganizationVersionViewModel { get; }
		public IEntityEntryViewModel FinancialIncomeCategoryEntryViewModel { get; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public string RegexForEmailForMailing { get; }

		public bool CanRead => PermissionResult.CanRead;

		public bool CanEdit =>
			PermissionResult.CanUpdate
			|| (Entity.Id == 0 && PermissionResult.CanCreate);

		public bool AskSaveOnClose => CanEdit;

		public override bool Save(bool close)
		{
			_logger.LogInformation("Сохраняем организацию...");

			try
			{
				return base.Save(close);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении организации.");

				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Организация не сохранилась...",
					"Ошибка при сохранении организации.");
				return false;
			}
		}

		private IEntityEntryViewModel BuildDefaultCashIncomeCategoryEntryViewModel()
		{
			var viewModel = _financialIncomeCategoryViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.DefaultCashIncomeCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
				filter =>
				{
					filter.RestrictFinancialSubtype = FinancialSubType.Income;
					filter.RestrictNodeSelectTypes.Add(typeof(FinancialIncomeCategory));
				})
				.UseViewModelDialog<FinancialIncomeCategoryViewModel>()
				.Finish();

			viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(FinancialIncomeCategory)).CanUpdate;

			return viewModel;
		}
	}
}
