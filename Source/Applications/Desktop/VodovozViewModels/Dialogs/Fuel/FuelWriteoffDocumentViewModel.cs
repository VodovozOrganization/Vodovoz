using Autofac;
using NHibernate.Criterion;
using QS.Commands;
using QS.DomainModel.Entity;
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
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Extensions;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelWriteoffDocumentViewModel : EntityTabViewModelBase<FuelWriteoffDocument>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeService _employeeService;
		private readonly IFuelRepository _fuelRepository;
		private readonly IGenericRepository<Car> _carRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IReportViewOpener _reportViewOpener;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IReportInfoFactory _reportInfoFactory;
		private Employee _currentEmployee;
		private FuelBalanceViewModel _fuelBalanceViewModel;
		private FinancialExpenseCategory _financialExpenseCategory;

		public FuelWriteoffDocumentViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IFuelRepository fuelRepository,
			IGenericRepository<Car> carRepository,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IReportViewOpener reportViewOpener,
			IRouteListProfitabilityController routeListProfitabilityController,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IReportInfoFactory reportInfoFactory
			)
			: base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			CreateCommands();
			UpdateCashSubdivisions();

			TabName = "Акт выдачи топлива";
			if(CurrentEmployee == null)
			{
				AbortOpening("К вашему пользователю не привязан сотрудник, невозможно открыть документ");
			}

			if(UoW.IsNew)
			{
				Entity.Date = DateTime.Now;
				Entity.Cashier = CurrentEmployee;
			}

			FinancialExpenseCategoryViewModel = BuildFinancialExpenseCategoryViewModel();

			SetPropertyChangeRelation(
				e => e.ExpenseCategoryId,
				() => FinancialExpenseCategory);

			ValidationContext.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);
			ConfigureEntries();
		}

		#region Id Ref Propeties

		public FinancialExpenseCategory FinancialExpenseCategory
		{
			get => this.GetIdRefField(ref _financialExpenseCategory, Entity.ExpenseCategoryId);
			set => this.SetIdRefField(SetField, ref _financialExpenseCategory, () => Entity.ExpenseCategoryId, value);
		}

		#endregion Id Ref Propeties

		#region EntityEntry ViewModels

		public IEntityEntryViewModel FinancialExpenseCategoryViewModel { get; }

		private IEntityEntryViewModel BuildFinancialExpenseCategoryViewModel()
		{
			var financialExpenseCategoryViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<FuelWriteoffDocumentViewModel>(this, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = financialExpenseCategoryViewModelEntryViewModelBuilder
				.ForProperty(x => x.FinancialExpenseCategory)
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
					})
				.Finish();

			viewModel.IsEditable = CanEdit;

			return viewModel;
		}

		#endregion EntityEntry ViewModels

		public Employee CurrentEmployee
		{
			get
			{
				if(_currentEmployee == null)
				{
					_currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return _currentEmployee;
			}
		}

		public FuelBalanceViewModel FuelBalanceViewModel
		{
			get
			{
				if(_fuelBalanceViewModel == null)
				{
					_fuelBalanceViewModel = new FuelBalanceViewModel(_unitOfWorkFactory, _subdivisionRepository, _fuelRepository);
				}
				return _fuelBalanceViewModel;
			}
		}

		#region Entries

		private void ConfigureEntries()
		{
			EmployeeAutocompleteSelectorFactory = EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
		}

		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelectorFactory { get; private set; }

		public IEmployeeJournalFactory EmployeeJournalFactory { get; }

		#endregion Entries

		protected override bool BeforeSave()
		{
			if(!IsEmployeeHasCarAndFuelCard())
			{
				CommonServices.InteractiveService.ShowMessage(
					QS.Dialog.ImportanceLevel.Error,
					$"У сотрудника отсутствуте авто, либо не выбрана топливная карта");

				return false;
			}

			Entity.UpdateOperations();
			return base.BeforeSave();
		}

		public bool CanEdit => true;
		public bool CanEditDate => CanEdit && CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_fuelwriteoff_document_date");

		public decimal GetAvailableLiters(FuelType fuelType)
		{
			if(Entity.CashSubdivision == null || fuelType == null)
			{
				return 0;
			}
			decimal existedLiters = 0;
			if(!UoW.IsNew)
			{
				using(var localUow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					var doc = localUow.GetById<FuelWriteoffDocument>(Entity.Id);
					var item = doc?.FuelWriteoffDocumentItems?.FirstOrDefault(x => x.FuelType.Id == fuelType.Id);
					if(item != null)
					{
						existedLiters = item.Liters;
					}
				}
			}
			return _fuelRepository.GetFuelBalanceForSubdivision(UoW, Entity.CashSubdivision, fuelType) + existedLiters;
		}

		private bool IsEmployeeHasCarAndFuelCard()
		{
			var employeeCar = _carRepository
				.Get(UoW, c => c.Driver.Id == Entity.Employee.Id)
				.FirstOrDefault();

			if(employeeCar == null)
			{
				return false;
			}

			var carFuelCard = employeeCar.FuelCardVersions
				.Where(v => v.StartDate <= Entity.Date
					&& (v.EndDate == null || v.EndDate >= Entity.Date))
				.FirstOrDefault();

			return carFuelCard != null;
		}

		#region Настройка списков доступных подразделений кассы

		public IEnumerable<Subdivision> AvailableSubdivisions { get; private set; }

		private void UpdateCashSubdivisions()
		{
			AvailableSubdivisions = _subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser);
			if(AvailableSubdivisions.Contains(CurrentEmployee.Subdivision))
			{
				Entity.CashSubdivision = CurrentEmployee.Subdivision;
			}
		}

		#endregion Настройка списков доступных подразделений кассы

		#region Commands

		public DelegateCommand AddWriteoffItemCommand { get; private set; }
		public DelegateCommand<FuelWriteoffDocumentItem> DeleteWriteoffItemCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }

		private void CreateCommands()
		{
			CreateAddWriteoffItemCommand();
			CreateDeleteWriteoffItemCommand();
			CreatePrintCommand();
		}

		private void CreateAddWriteoffItemCommand()
		{
			AddWriteoffItemCommand = new DelegateCommand(
				() =>
				{
					var fuelTypeJournalViewModel = new SimpleEntityJournalViewModel<FuelType, FuelTypeViewModel>(x => x.Name,
						() =>
							new FuelTypeViewModel(
								EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, CommonServices, _fuelRepository, _routeListProfitabilityController),
						(node) =>
							new FuelTypeViewModel(
								EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, CommonServices, _fuelRepository, _routeListProfitabilityController),
						_unitOfWorkFactory,
						CommonServices);
					fuelTypeJournalViewModel.SetRestriction(() =>
					{
						return Restrictions.Not(Restrictions.In(Projections.Id(), Entity.ObservableFuelWriteoffDocumentItems.Select(x => x.FuelType.Id).ToArray()));
					});
					fuelTypeJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					fuelTypeJournalViewModel.OnEntitySelectedResult += (sender, e) =>
					{
						var node = e.SelectedNodes.FirstOrDefault();
						if(node == null)
						{
							return;
						}
						Entity.AddNewWriteoffItem(UoW.GetById<FuelType>(node.Id));
					};

					var fuelTypePermissionSet = CommonServices.PermissionService.ValidateUserPermission(typeof(FuelType), UserService.CurrentUserId);
					if(fuelTypePermissionSet.CanRead && !fuelTypePermissionSet.CanUpdate)
					{
						var viewAction = new JournalAction("Просмотр",
							(selected) => selected.Any(),
							(selected) => true,
							(selected) =>
							{
								var tab = fuelTypeJournalViewModel.GetTabToOpen(typeof(FuelType), selected.First().GetId());
								fuelTypeJournalViewModel.TabParent.AddTab(tab, fuelTypeJournalViewModel);
							}
						);

						(fuelTypeJournalViewModel.NodeActions as IList<IJournalAction>)?.Add(viewAction);
					}

					TabParent.AddSlaveTab(this, fuelTypeJournalViewModel);
				},
				() => CanEdit
			);
			AddWriteoffItemCommand.CanExecuteChangedWith(this, x => CanEdit);
		}

		private void CreateDeleteWriteoffItemCommand()
		{
			DeleteWriteoffItemCommand = new DelegateCommand<FuelWriteoffDocumentItem>(
				Entity.RemoveWriteoffItem,
				(item) => CanEdit && item != null
			);
			AddWriteoffItemCommand.CanExecuteChangedWith(this, x => CanEdit);
		}

		private void CreatePrintCommand()
		{
			PrintCommand = new DelegateCommand(
				() =>
				{
					var reportInfo = _reportInfoFactory.Create();
					reportInfo.Title = String.Format($"Акт выдачи топлива №{Entity.Id} от {Entity.Date:d}");
					reportInfo.Identifier = "Documents.FuelWriteoffDocument";
					reportInfo.Parameters = new Dictionary<string, object> { { "document_id", Entity.Id } };

					_reportViewOpener.OpenReport(this, reportInfo);
				},
				() => Entity.Id != 0
			);
		}

		#endregion
	}
}
