using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
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
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Settings.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Cash.Transfer
{
	public class CommonCashTransferDocumentViewModel : EntityTabViewModelBase<CommonCashTransferDocument>
	{
		private readonly ICategoryRepository _categoryRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IReportInfoFactory _reportInfoFactory;
		private FinancialExpenseCategory _financialExpenseCategory;
		private FinancialIncomeCategory _financialIncomeCategory;

		private IEnumerable<Subdivision> _cashSubdivisions;
		private IList<Subdivision> _availableSubdivisionsForUser;
		private Employee _cashier;

		private bool _isUpdatingSubdivisions = false;

		public CommonCashTransferDocumentViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICategoryRepository categoryRepository,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			INavigationManager navigationManager,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			IReportViewOpener reportViewOpener,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IReportInfoFactory reportInfoFactory
			)
			: base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(financialCategoriesGroupsSettings is null)
			{
				throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			}

			_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			if(entityUoWBuilder.IsNewEntity)
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

			CarEntryViewModel = BuildCarEntryViewModel();

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

		private IEntityEntryViewModel BuildFinancialIncomeCategoryViewModel()
		{
			var financialIncomeCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<CommonCashTransferDocumentViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

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
			var financialExpenseCategoryEntryViewModelBuilder = new CommonEEVMBuilderFactory<CommonCashTransferDocumentViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

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
			var carViewModelBuilder = new CommonEEVMBuilderFactory<CommonCashTransferDocument>(this, Entity, UoW, NavigationManager, _lifetimeScope);

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

			PrintCommand = new DelegateCommand(
				() =>
				{
					var reportInfo = _reportInfoFactory.Create();
					reportInfo.Title = $"Документ перемещения №{Entity.Id} от {Entity.CreationDate:d}";
					reportInfo.Identifier = "Documents.CommonCashTransfer";
					reportInfo.Parameters = new Dictionary<string, object> { { "transfer_document_id", Entity.Id } };

					_reportViewOpener.OpenReport(TabParent, reportInfo);
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

		public ILifetimeScope LifetimeScope => _lifetimeScope;

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
