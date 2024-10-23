using Autofac;
using QS.Commands;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.DocumentsJournal;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Cash.Transfer
{
	public class IncomeCashTransferDocumentViewModel : EntityTabViewModelBase<IncomeCashTransferDocument>
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IReportInfoFactory _reportInfoFactory;
		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;

		private Employee _cashier;
		private bool _incomesSelected;
		private bool _expensesSelected;

		public IncomeCashTransferDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IGtkTabsOpener gtkTabsOpener,
			IReportViewOpener reportViewOpener,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IReportInfoFactory reportInfoFactory
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(financialCategoriesGroupsSettings is null)
			{
				throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			}

			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			EmployeeAutocompleteSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			CarEntryViewModel = BuildCarEntryViewModel();

			if(uowBuilder.IsNewEntity)
			{
				Entity.CreationDate = DateTime.Now;
				Entity.Author = Cashier;
				Entity.IncomeCategoryId = financialCategoriesGroupsSettings.TransferDefaultFinancialIncomeCategoryId;
				Entity.ExpenseCategoryId = financialCategoriesGroupsSettings.TransferDefaultFinancialExpenseCategoryId;
			}

			CreateCommands();
			UpdateCashSubdivisions();

			FinancialExpenseCategoryViewModel = BuildFinancialExpenseCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			FinancialIncomeCategoryViewModel = BuildFinancialIncomeCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.IncomeCategoryId,
				() => FinancialIncomeCategory);

			ConfigEntityUpdateSubscribes();
			ConfigureEntityPropertyChanges();
		}

		#region Id Ref Propeties

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		public FinancialIncomeCategory FinancialIncomeCategory
		{
			get => this.GetIdRefField(ref _financialIncomeCategory, Entity.IncomeCategoryId);
			set => this.SetIdRefField(SetField, ref _financialIncomeCategory, () => Entity.IncomeCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region EntityEntry ViewModels

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		private IEntityEntryViewModel BuildFinancialIncomeCategoryViewModel()
		{
			var financialIncomeCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<IncomeCashTransferDocumentViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialIncomeCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialIncomeCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Income;
						filter.TargetDocument = TargetDocument.Transfer;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialIncomeCategory));
					})
				.Finish();

			viewModel.IsEditable = CanEdit;

			return viewModel;
		}

		public IEntityEntryViewModel FinancialIncomeCategoryViewModel { get; }

		private IEntityEntryViewModel BuildFinancialExpenseCategoryViewModel()
		{
			var financialExpenseCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<IncomeCashTransferDocumentViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialExpenseCategoryEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.TargetDocument = TargetDocument.Transfer;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();

			viewModel.IsEditable = CanEdit;

			return viewModel;
		}

		public IEntityEntryViewModel CarEntryViewModel { get; }

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<IncomeCashTransferDocument>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var viewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		#endregion EntityEntry ViewModels

		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelectorFactory { get; }

		public Employee Cashier
		{
			get
			{
				if(_cashier == null)
				{
					_cashier = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				}
				return _cashier;
			}
		}

		public virtual bool IncomesSelected
		{
			get => _incomesSelected;
			set => SetField(ref _incomesSelected, value);
		}

		public virtual bool ExpensesSelected
		{
			get => _expensesSelected;
			set => SetField(ref _expensesSelected, value);
		}

		public bool CanEdit => Entity.Status == CashTransferDocumentStatuses.New;

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEdit);

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.CashSubdivisionFrom))
			{
				UpdateSubdivisionsTo();
				Entity.ObservableCashTransferDocumentIncomeItems.Clear();
				Entity.ObservableCashTransferDocumentExpenseItems.Clear();
			}

			if(e.PropertyName == nameof(Entity.CashSubdivisionTo))
			{
				UpdateSubdivisionsFrom();
			}
		}

		#region Подписки на внешние измнения сущностей

		private void ConfigEntityUpdateSubscribes()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<RouteList>(RoutelistEntityConfig_EntityUpdated);
		}

		private void RoutelistEntityConfig_EntityUpdated(EntityChangeEvent[] changeEvents)
		{
			foreach(var updatedItem in changeEvents.Select(x => x.Entity))
			{
				if(updatedItem is RouteList updatedRouteList)
				{
					var foundRouteList = Entity.CashTransferDocumentIncomeItems
						.Where(x => x.Income != null)
						.Where(x => x.Income.RouteListClosing != null)
						.Select(x => x.Income.RouteListClosing)
						.FirstOrDefault(x => x.Id == updatedRouteList.Id);

					if(foundRouteList != null)
					{
						UoW.Session.Refresh(foundRouteList);
					}
				}
			}
		}

		#endregion Подписки на внешние измнения сущностей

		#region Commands

		public DelegateCommand<IEnumerable<IncomeCashTransferedItem>> DeleteIncomesCommand { get; private set; }
		public DelegateCommand<IEnumerable<ExpenseCashTransferedItem>> DeleteExpensesCommand { get; private set; }
		public DelegateCommand<Income> OpenRouteListCommand { get; private set; }
		public DelegateCommand SendCommand { get; private set; }
		public DelegateCommand ReceiveCommand { get; private set; }
		public DelegateCommand AddIncomesCommand { get; private set; }
		public DelegateCommand AddExpensesCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }

		private void CreateCommands()
		{
			DeleteIncomesCommand = new DelegateCommand<IEnumerable<IncomeCashTransferedItem>>(
				Entity.DeleteTransferedIncomes,
				(parameter) =>
				{
					return parameter != null
						&& parameter.Any()
						&& CanEdit;
				}
			);

			DeleteIncomesCommand.CanExecuteChangedWith(this, x => x.IncomesSelected);
			DeleteIncomesCommand.CanExecuteChangedWith(Entity, x => x.Status);

			DeleteExpensesCommand = new DelegateCommand<IEnumerable<ExpenseCashTransferedItem>>(
				Entity.DeleteTransferedExpenses,
				(parameter) =>
				{
					return parameter != null
						&& parameter.Any()
						&& CanEdit;
				}
			);

			DeleteExpensesCommand.CanExecuteChangedWith(this, x => x.ExpensesSelected);
			DeleteExpensesCommand.CanExecuteChangedWith(Entity, x => x.Status);

			OpenRouteListCommand = new DelegateCommand<Income>(
				(parameter) =>
				{
					if(parameter.RouteListClosing == null)
					{
						return;
					}

					_gtkTabsOpener.OpenRouteListClosingDlg(TabParent, parameter.RouteListClosing.Id);
				},
				(parameter) => { return parameter != null && parameter.RouteListClosing != null; }
			);

			SendCommand = new DelegateCommand(
				() =>
				{
					if(!Validate())
					{
						return;
					}
					Entity.Send(Cashier, Entity.Comment);
				},
				() =>
				{
					return Cashier != null
						&& Entity.Status == CashTransferDocumentStatuses.New
						&& Entity.Driver != null
						&& Entity.Car != null
						&& Entity.CashSubdivisionFrom != null
						&& Entity.CashSubdivisionTo != null
						&& Entity.ExpenseCategoryId != null
						&& Entity.IncomeCategoryId != null
						&& Entity.TransferedSum > 0;
				}
			);

			SendCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Driver,
				x => x.Car,
				x => x.CashSubdivisionFrom,
				x => x.CashSubdivisionTo,
				x => x.TransferedSum,
				x => x.ExpenseCategoryId,
				x => x.IncomeCategoryId
			);

			ReceiveCommand = new DelegateCommand(
				() =>
				{
					Entity.Receive(Cashier, Entity.Comment);
				},
				() =>
				{
					return Cashier != null
						&& Entity.Status == CashTransferDocumentStatuses.Sent
						&& _availableSubdivisionsForUser.Contains(Entity.CashSubdivisionTo)
						&& Entity.Id != 0;
				}
			);

			ReceiveCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.Id
			);

			AddIncomesCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<DocumentsJournalViewModel, Action<DocumentsFilterViewModel>>(
						this,
						filter =>
						{
							filter.RestrictDocument = typeof(Income);

							//скрываем уже выбранные приходники и отображаем расходники только выбранного подразделения
							filter.HiddenIncomes.AddRange(Entity.CashTransferDocumentIncomeItems.Select(x => x.Income.Id));

							filter.RestrictRelatedToSubdivisionId = Entity.CashSubdivisionFrom?.Id;

							//скрываем приходники выбранные в других документах перемещения
							filter.RestrictNotTransfered = true;
						});

					page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
					page.ViewModel.OnEntitySelectedResult += IncomesSelectDlg_ObjectSelected;
				},
				() =>
				{
					return Entity.Status == CashTransferDocumentStatuses.New
						&& Entity.CashSubdivisionTo != null
						&& Entity.CashSubdivisionFrom != null;
				}
			);

			AddIncomesCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.CashSubdivisionTo,
				x => x.CashSubdivisionFrom
			);

			AddExpensesCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<DocumentsJournalViewModel, Action<DocumentsFilterViewModel>>(
						this,
						filter =>
						{
							filter.RestrictDocument = typeof(Expense);

							//скрываем уже выбранные расходники и отображаем расходники только выбранного подразделения
							filter.HiddenExpenses.AddRange(Entity.CashTransferDocumentExpenseItems.Select(x => x.Expense.Id));

							filter.RestrictRelatedToSubdivisionId = Entity.CashSubdivisionFrom?.Id;

							//скрываем расходники выбранные в других документах перемещения
							filter.RestrictNotTransfered = true;
						});

					page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
					page.ViewModel.OnEntitySelectedResult += ExpensesSelectDlg_ObjectSelected;
				},
				() =>
				{
					return Entity.Status == CashTransferDocumentStatuses.New
						&& Entity.CashSubdivisionTo != null
						&& Entity.CashSubdivisionFrom != null;
				}
			);

			AddExpensesCommand.CanExecuteChangedWith(Entity,
				x => x.Status,
				x => x.CashSubdivisionTo,
				x => x.CashSubdivisionFrom
			);

			PrintCommand = new DelegateCommand(
			() =>
			{
				var reportInfo = _reportInfoFactory.Create();
				reportInfo.Title = $"Документ перемещения №{Entity.Id} от {Entity.CreationDate:d}";
				reportInfo.Identifier = "Documents.IncomeCashTransfer";
				reportInfo.Parameters = new Dictionary<string, object> { { "transfer_document_id", Entity.Id } };

				_reportViewOpener.OpenReport(TabParent, reportInfo);
			},
				() => Entity.Id != 0
			);
		}

		private void IncomesSelectDlg_ObjectSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			if(!e.SelectedNodes.Any())
			{
				return;
			}

			var ids = e.SelectedNodes.Select(x => x.Id);

			var incomesToAdd = UoW.Session.Query<Income>().Where(x => ids.Contains(x.Id));

			foreach(var item in incomesToAdd)
			{
				Entity.AddIncomeItem(item);
			}
		}

		private void ExpensesSelectDlg_ObjectSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			if(!e.SelectedNodes.Any())
			{
				return;
			}

			var ids = e.SelectedNodes.Select(x => x.Id);

			var incomesToAdd = UoW.Session.Query<Expense>().Where(x => ids.Contains(x.Id));

			foreach(var item in incomesToAdd)
			{
				Entity.AddExpenseItem(item);
			}
		}

		#endregion Commands

		#region Настройка списков доступных подразделений кассы

		private IEnumerable<Subdivision> _cashSubdivisions;

		private IList<Subdivision> _availableSubdivisionsForUser;

		private IEnumerable<Subdivision> _subdivisionsFrom;

		public virtual IEnumerable<Subdivision> SubdivisionsFrom
		{
			get => _subdivisionsFrom;
			set => SetField(ref _subdivisionsFrom, value);
		}

		private IEnumerable<Subdivision> _subdivisionsTo;

		public virtual IEnumerable<Subdivision> SubdivisionsTo
		{
			get => _subdivisionsTo;
			set => SetField(ref _subdivisionsTo, value);
		}

		private void UpdateCashSubdivisions()
		{
			Type[] cashDocumentTypes = { typeof(Income), typeof(Expense), typeof(AdvanceReport) };
			_availableSubdivisionsForUser = _subdivisionRepository.GetAvailableSubdivionsForUser(UoW, cashDocumentTypes).ToList();

			if(Entity.Id != 0
			   && !CanEdit
			   && Entity.CashSubdivisionFrom != null
			   && !_availableSubdivisionsForUser.Contains(Entity.CashSubdivisionFrom))
			{
				_availableSubdivisionsForUser.Add(Entity.CashSubdivisionFrom);
			}

			_cashSubdivisions = _subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, cashDocumentTypes).Distinct();
			SubdivisionsFrom = _availableSubdivisionsForUser;
			SubdivisionsTo = _cashSubdivisions;
		}

		private bool _isUpdatingSubdivisions = false;

		private void UpdateSubdivisionsFrom()
		{
			if(_isUpdatingSubdivisions)
			{
				return;
			}

			_isUpdatingSubdivisions = true;
			var currentSubdivisonFrom = Entity.CashSubdivisionFrom;
			SubdivisionsFrom = _availableSubdivisionsForUser.Where(x => x != Entity.CashSubdivisionTo);

			if(SubdivisionsTo.Contains(currentSubdivisonFrom))
			{
				Entity.CashSubdivisionFrom = currentSubdivisonFrom;
			}

			_isUpdatingSubdivisions = false;
		}

		private void UpdateSubdivisionsTo()
		{
			if(_isUpdatingSubdivisions)
			{
				return;
			}

			_isUpdatingSubdivisions = true;
			var currentSubdivisonTo = Entity.CashSubdivisionTo;
			SubdivisionsTo = _cashSubdivisions.Where(x => x != Entity.CashSubdivisionFrom);

			if(SubdivisionsTo.Contains(currentSubdivisonTo))
			{
				Entity.CashSubdivisionTo = currentSubdivisonTo;
			}

			_isUpdatingSubdivisions = false;
		}

		#endregion Настройка списков доступных подразделений кассы

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Dispose();
		}
	}
}
