using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.PermissionExtensions;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Cash
{
	public class ExpenseSelfDeliveryViewModel : EntityTabViewModelBase<Expense>
	{
		private readonly IPermissionResult _entityPermissionResult;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICashRepository _cashRepository;
		private readonly IEntityExtendedPermissionValidator _entityExtendedPermissionValidator;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IReportInfoFactory _reportInfoFactory;
		private IEntityEntryViewModel _orderViewModel;
		private FinancialExpenseCategory _financialExpenseCategory;

		public ExpenseSelfDeliveryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope lifetimeScope,
			IUserService userService,
			IEmployeeRepository employeeRepository,
			ICashRepository cashRepository,
			IEntityExtendedPermissionValidator entityExtendedPermissionValidator,
			IReportViewOpener reportViewOpener,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IFinancialExpenseCategoriesRepository financialExpenseCategoriesRepository,
			IReportInfoFactory reportInfoFactory
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = IsNew ? "Расходный кассовый ордер самовывоза" : $"Расходный кассовый ордер самовывоза {Entity.Id}";

			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			if(userService is null)
			{
				throw new ArgumentNullException(nameof(userService));
			}

			_lifetimeScope = lifetimeScope
				?? throw new ArgumentNullException(nameof(lifetimeScope));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_cashRepository = cashRepository
				?? throw new ArgumentNullException(nameof(cashRepository));
			_entityExtendedPermissionValidator = entityExtendedPermissionValidator
				?? throw new ArgumentNullException(nameof(entityExtendedPermissionValidator));
			_reportViewOpener = reportViewOpener
				?? throw new ArgumentNullException(nameof(reportViewOpener));
			_financialCategoriesGroupsSettings = financialCategoriesGroupsSettings
				?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_entityPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Expense));
			CanEditRectroactively =
				_entityExtendedPermissionValidator.Validate(
					typeof(Expense), userService.CurrentUserId, nameof(RetroactivelyClosePermission));

			if(IsNew)
			{
				Entity.Casher = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				if(Entity.Casher is null)
				{
					InitializationFailed("Ошибка",
						  "Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
					return;
				}

				if(!CanCreate)
				{
					InitializationFailed("Ошибка",
						 "Отсутствуют права на создание расходного ордера");
					return;
				}

				Entity.TypeDocument = ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery;
				Entity.TypeOperation = ExpenseType.ExpenseSelfDelivery;

				Entity.ExpenseCategoryId = _financialCategoriesGroupsSettings.SelfDeliveryDefaultFinancialExpenseCategoryId;

				Entity.Date = DateTime.Now;
			}

			CashierViewModel = BuildCashierEntryViewModel();

			FinancialExpenseCategoryViewModel = BuildFinancialExpenseCategoryViewModelEntryViewModel();

			Entity.PropertyChanged += OnEntityPropertyChanged;

			SetPropertyChangeRelation(
				e => e.Id,
				() => IsNew);

			SetPropertyChangeRelation(
				e => e.Date,
				() => CanEdit);

			SetPropertyChangeRelation(
				e => e.Money,
				() => Money);

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			ValidationContext.Items.Add("IsSelfDelivery", true);
			ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWork), UoW);
			ValidationContext.ServiceContainer.AddService(typeof(IFinancialExpenseCategoriesRepository), financialExpenseCategoriesRepository);

			PrintCommand = new DelegateCommand(Print);
			SaveCommand = new DelegateCommand(SaveAndClose, () => CanEdit);
			CloseCommand = new DelegateCommand(() => Close(true, CloseSource.Self));
		}

		#region Commands

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CloseCommand { get; }

		public DelegateCommand PrintCommand { get; }

		#endregion Commands

		#region Id Ref Propeties

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region EntityEntry ViewModels

		public IEntityEntryViewModel CashierViewModel { get; }

		public IEntityEntryViewModel BuildCashierEntryViewModel()
		{
			var cashierEntryViewModelBuilder = new CommonEEVMBuilderFactory<Expense>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var viewModel = cashierEntryViewModelBuilder
				.ForProperty(x => x.Casher)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
						filter.Status = EmployeeStatus.IsWorking;
					})
				.Finish();

			viewModel.IsEditable = false;

			return viewModel;
		}

		public IEntityEntryViewModel OrderViewModel
		{
			get => _orderViewModel;
			set // при обновлении версии языка - заменить на init или при смене countarparty на mvvm - заменить билдер
			{
				if(_orderViewModel is null)
				{
					_orderViewModel = value;
				}
			}
		}

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		public IEntityEntryViewModel BuildFinancialExpenseCategoryViewModelEntryViewModel()
		{
			var financialExpenseCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<ExpenseSelfDeliveryViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			return financialExpenseCategoryViewModelEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.TargetDocument = Entity.TypeDocument.ToTargetDocument();
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();
		}

		#endregion EntityEntry ViewModels

		public string CurrencySymbol => NumberFormatInfo.CurrentInfo.CurrencySymbol;

		public bool IsNew => UoWGeneric.IsNew;

		public bool CanCreate => _entityPermissionResult.CanCreate;

		public bool CanEdit => (IsNew && CanCreate)
			|| (_entityPermissionResult.CanUpdate && Entity.Date.Date == DateTime.Now.Date)
			|| CanEditRectroactively;

		public bool CanEditRectroactively { get; }

		public decimal Money
		{
			get => Entity.Money;
			set => Entity.Money = value;
		}

		public ILifetimeScope Scope => _lifetimeScope;

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Order)
				&& Entity.Order != null)
			{
				Entity.FillFromOrder(UoW, _cashRepository);
			}
		}

		public void SetOrderById(int orderId)
		{
			Entity.Order = UoW.GetById<Order>(orderId);
		}

		public void InitializationFailed(
			string title = "",
			string message = "")
		{
			if(!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(message))
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, message, title);
			}

			FailInitialize = true;
		}

		private void Print()
		{
			if(UoWGeneric.HasChanges
				&& (!AskQuestion("Сохранить изменения перед печатью?") || !Save()))
			{
				return;
			}

			var reportInfo = _reportInfoFactory.Create();
			reportInfo.Title = $"Квитанция №{Entity.Id} от {Entity.Date:d}";
			reportInfo.Identifier = "Cash.Expense";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "id",  Entity.Id }
			};

			_reportViewOpener.OpenReport(this, reportInfo);
		}
	}
}
