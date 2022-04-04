using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelWriteoffDocumentViewModel : EntityTabViewModelBase<FuelWriteoffDocument>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IEmployeeService employeeService;
		private readonly IFuelRepository fuelRepository;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly ICommonServices commonServices;
		private readonly IReportViewOpener reportViewOpener;
		public IExpenseCategorySelectorFactory ExpenseSelectorFactory { get; }

		public FuelWriteoffDocumentViewModel(
			IEntityUoWBuilder uoWBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IFuelRepository fuelRepository,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IReportViewOpener reportViewOpener,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IExpenseCategorySelectorFactory expenseCategorySelectorFactory
		) 
		: base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			this.reportViewOpener = reportViewOpener ?? throw new ArgumentNullException(nameof(reportViewOpener));
			SubdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			ExpenseSelectorFactory =
				expenseCategorySelectorFactory ?? throw new ArgumentNullException(nameof(expenseCategorySelectorFactory));

			CreateCommands();
			UpdateCashSubdivisions();

			TabName = "Акт выдачи топлива";
			if(CurrentEmployee == null) {
				AbortOpening("К вашему пользователю не привязан сотрудник, невозможно открыть документ");
			}

			if(UoW.IsNew) {
				Entity.Date = DateTime.Now;
				Entity.Cashier = CurrentEmployee;
			}

			ValidationContext.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);
			ConfigureEntries();
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

		private FuelBalanceViewModel fuelBalanceViewModel;
		public FuelBalanceViewModel FuelBalanceViewModel {
			get {
				if(fuelBalanceViewModel == null) {
					fuelBalanceViewModel = new FuelBalanceViewModel(subdivisionRepository, fuelRepository);
				}
				return fuelBalanceViewModel;
			}
		}

		#region Entries

		private void ConfigureEntries()
		{
			EmployeeAutocompleteSelectorFactory = EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
		}
		
		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelectorFactory { get; private set; }
		
		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }

		#endregion Entries

		protected override void BeforeSave()
		{
			Entity.UpdateOperations();
			base.BeforeSave();
		}

		public bool CanEdit => true;
		public bool CanEditDate => CanEdit && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_fuelwriteoff_document_date");

		public decimal GetAvailableLiters(FuelType fuelType)
		{
			if(Entity.CashSubdivision == null || fuelType == null) {
				return 0;
			}
			decimal existedLiters = 0;
			if(!UoW.IsNew) {
				using(var localUow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var doc = localUow.GetById<FuelWriteoffDocument>(Entity.Id);
					var item = doc?.FuelWriteoffDocumentItems?.FirstOrDefault(x => x.FuelType.Id == fuelType.Id);
					if(item != null) {
						existedLiters = item.Liters;
					}
				}
			}
			return fuelRepository.GetFuelBalanceForSubdivision(UoW, Entity.CashSubdivision, fuelType) + existedLiters;
		}

		#region Настройка списков доступных подразделений кассы

		public IEnumerable<Subdivision> AvailableSubdivisions { get; private set; }

		private void UpdateCashSubdivisions()
		{
			AvailableSubdivisions = subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser);
			if(AvailableSubdivisions.Contains(CurrentEmployee.Subdivision)) {
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
				() => {
					var fuelTypeJournalViewModel = new SimpleEntityJournalViewModel<FuelType, FuelTypeViewModel>(x => x.Name,
						() => new FuelTypeViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, commonServices),
						(node) => new FuelTypeViewModel(EntityUoWBuilder.ForOpen(node.Id), unitOfWorkFactory, commonServices),
						QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						commonServices
					);
					fuelTypeJournalViewModel.SetRestriction(() => {
						return Restrictions.Not(Restrictions.In(Projections.Id(), Entity.ObservableFuelWriteoffDocumentItems.Select(x => x.FuelType.Id).ToArray()));
					});
					fuelTypeJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					fuelTypeJournalViewModel.OnEntitySelectedResult += (sender, e) => {
						var node = e.SelectedNodes.FirstOrDefault();
						if(node == null) {
							return;
						}
						Entity.AddNewWriteoffItem(UoW.GetById<FuelType>(node.Id));
					};

					var fuelTypePermissionSet = commonServices.PermissionService.ValidateUserPermission(typeof(FuelType), commonServices.UserService.CurrentUserId);
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
				() => {
					var reportInfo = new QS.Report.ReportInfo {
						Title = String.Format($"Акт выдачи топлива №{Entity.Id} от {Entity.Date:d}"),
						Identifier = "Documents.FuelWriteoffDocument",
						Parameters = new Dictionary<string, object> { { "document_id", Entity.Id } }
					};

					reportViewOpener.OpenReport(this, reportInfo);
				},
				() => Entity.Id != 0
			);
		}

		#endregion
	}
}
