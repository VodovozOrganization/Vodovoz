using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;

namespace Vodovoz.Dialogs.Cash.CashTransfer
{
	public class CommonCashTransferDocumentViewModel : ViewModel<CommonCashTransferDocument>
	{
		private readonly ICategoryRepository _categoryRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ILifetimeScope _lifetimeScope;
		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;

		private IEnumerable<Subdivision> _cashSubdivisions;
		private IList<Subdivision> _availableSubdivisionsForUser;
		private Employee _cashier;

		private bool _isUpdatingSubdivisions = false;

		public CommonCashTransferDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory factory,
			ICategoryRepository categoryRepository,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope) : base(entityUoWBuilder, factory)
		{
			_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			if(entityUoWBuilder.IsNewEntity)
			{
				Entity.CreationDate = DateTime.Now;
				Entity.Author = Cashier;
			}

			CreateCommands();
			UpdateCashSubdivisions();

			FinancialExpenseCategoryViewModel = BuildFinancialIncomeCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			FinancialIncomeCategoryViewModel = BuildFinancialExpenseCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.IncomeCategoryId,
				() => FinancialIncomeCategory);

			View = new CommonCashTransferDlg(this);

			Entity.PropertyChanged += Entity_PropertyChanged;

			ConfigureEntityChangingRelations();
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

		private IEntityEntryViewModel BuildFinancialExpenseCategoryViewModel()
		{
			var financialIncomeCategoryViewModelEntryViewModelBuilder = new LegacyEEVMBuilderFactory<CommonCashTransferDocumentViewModel>(View, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialIncomeCategoryViewModelEntryViewModelBuilder
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

		private IEntityEntryViewModel BuildFinancialIncomeCategoryViewModel()
		{
			var financialExpenseCategoryViewModelEntryViewModelBuilder = new LegacyEEVMBuilderFactory<CommonCashTransferDocumentViewModel>(View, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialExpenseCategoryViewModelEntryViewModelBuilder
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

		#endregion EntityEntry ViewModels

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

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

		public bool CanEdit => Entity.Status == CashTransferDocumentStatuses.New;

		public override bool Save()
		{
			var valid = new QSValidator<CommonCashTransferDocument>(Entity, new Dictionary<object, object>());

			if(valid.RunDlgIfNotValid())
			{
				return false;
			}

			return base.Save();
		}

		protected void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.Status,
				() => CanEdit
			);
		}

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.CashSubdivisionFrom))
			{
				UpdateSubdivisionsTo();
			}

			if(e.PropertyName == nameof(Entity.CashSubdivisionTo))
			{
				UpdateSubdivisionsFrom();
			}
		}

		#region Commands

		public DelegateCommand SendCommand { get; private set; }
		public DelegateCommand ReceiveCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }

		private void CreateCommands()
		{
			SendCommand = new DelegateCommand(
				() =>
				{
					var valid = new QSValidator<CommonCashTransferDocument>(Entity, new Dictionary<object, object>());
					if(valid.RunDlgIfNotValid())
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

			PrintCommand = new DelegateCommand(
				() =>
				{
					var reportInfo = new QS.Report.ReportInfo
					{
						Title = $"Документ перемещения №{Entity.Id} от {Entity.CreationDate:d}",
						Identifier = "Documents.CommonCashTransfer",
						Parameters = new Dictionary<string, object> { { "transfer_document_id", Entity.Id } }
					};

					var report = new QSReport.ReportViewDlg(reportInfo);
					View.TabParent.AddTab(report, View, false);
				},
				() => Entity.Id != 0
			);
		}

		#endregion Commands

		#region Настройка списков доступных подразделений кассы

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

		public INavigationManager NavigationManager { get; }

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
	}
}
