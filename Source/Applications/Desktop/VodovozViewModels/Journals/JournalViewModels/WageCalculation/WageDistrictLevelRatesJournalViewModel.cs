using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Journals.JournalViewModels.WageCalculation
{
	public class WageDistrictLevelRatesJournalViewModel : SingleEntityJournalViewModelBase<WageDistrictLevelRates, WageDistrictLevelRatesViewModel, WageDistrictLevelRatesJournalNode>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly INavigationManager _navigationManager;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly bool _canUpdate;
		private readonly bool _canCreate;
		private readonly bool _canAssignWageDistrictLevelRates;

		public WageDistrictLevelRatesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IWageCalculationRepository wageCalculationRepository)
			: base(unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_interactiveService = commonServices.InteractiveService;

			TabName = "Журнал ставок по уровням";

			var threadLoader = DataLoader as ThreadDataLoader<WageDistrictLevelRatesJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Name, false);

			UpdateOnChanges(typeof(WageDistrictLevelRates));

			var permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(WageDistrictLevelRates));
			_canCreate = permissionResult.CanCreate;
			_canUpdate = permissionResult.CanUpdate;

			var canEditEmployee =
				commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Employee)).CanUpdate;
			var canEditWage =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.EmployeePermissions.CanEditWage);

			_canAssignWageDistrictLevelRates = canEditEmployee && canEditWage;
		}

		protected override Func<IUnitOfWork, IQueryOver<WageDistrictLevelRates>> ItemsSourceQueryFunction => (uow) => {
			WageDistrictLevelRatesJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<WageDistrictLevelRates>();
			query.Where(
				GetSearchCriterion<WageDistrictLevelRates>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
									.Select(x => x.IsDefaultLevel).WithAlias(() => resultAlias.IsDefaultLevel)
									.Select(x => x.IsDefaultLevelForOurCars).WithAlias(() => resultAlias.IsDefaultLevelOurCars)
									.Select(x => x.IsDefaultLevelForRaskatCars).WithAlias(() => resultAlias.IsDefaultLevelRaskatCars)
								)
								.TransformUsing(Transformers.AliasToBean<WageDistrictLevelRatesJournalNode>())
								.OrderBy(x => x.Name).Asc
								.ThenBy(x => x.IsArchive).Asc
								;

			return result;
		};

		protected override Func<WageDistrictLevelRatesViewModel> CreateDialogFunction => () => new WageDistrictLevelRatesViewModel(
			this,
			EntityUoWBuilder.ForCreate(),
			_unitOfWorkFactory,
			commonServices,
			UoW,
			_wageCalculationRepository
	   );

		protected override Func<WageDistrictLevelRatesJournalNode, WageDistrictLevelRatesViewModel> OpenDialogFunction => n => new WageDistrictLevelRatesViewModel(
			this,
			EntityUoWBuilder.ForOpen(n.Id),
			_unitOfWorkFactory,
			commonServices,
			UoW,
			_wageCalculationRepository
	   );

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();
			CreateWageDistrictLevelRatesAssigningAction();
		}

		protected override void CreatePopupActions()
		{
			CreateCopyAction();
		}

		private void CreateWageDistrictLevelRatesAssigningAction()
		{
			var createExportToExcelAction = new JournalAction(
				"Привязка ставок",
				(selected) => _canAssignWageDistrictLevelRates,
				(selected) => _canAssignWageDistrictLevelRates,
				(selected) =>
				{
					_navigationManager.OpenViewModel<WageDistrictLevelRatesAssigningViewModel>(null);
				}
			);
			NodeActionsList.Add(createExportToExcelAction);
		}

		private void CreateCopyAction()
		{
			var copyAction = new JournalAction("Копировать",
				selectedItems => _canCreate && _canUpdate && selectedItems.OfType<WageDistrictLevelRatesJournalNode>().FirstOrDefault() != null,
				selected => true,
				selected =>
				{
					var selectedNode = selected.OfType<WageDistrictLevelRatesJournalNode>().FirstOrDefault();

					if(selectedNode == null)
					{
						return;
					}

					var wageLevelRateToCopy = UoW.GetById<WageDistrictLevelRates>(selectedNode.Id);

					if(!_interactiveService.Question($"Скопировать ставку \"{selectedNode.Name}\"?"))
					{
						return;
					}

					var copy = CreateCopy(wageLevelRateToCopy);

					if(copy.IsDefaultLevel)
					{
						ResetExistinDefaultLevelsForNewEmployees(UoW);
					}

					if(copy.IsDefaultLevelForOurCars)
					{
						ResetExistinDefaultLevelsForNewEmployeesOnOurCars(UoW);
					}

					if(copy.IsDefaultLevelForRaskatCars)
					{
						ResetExistinDefaultLevelsForNewEmployeesOnRaskatCars(UoW);
					}

					UoW.Save(copy);
					UoW.Commit();

					_interactiveService.ShowMessage(ImportanceLevel.Info, "Копирование завершено");

					Refresh();
				}
			);
			PopupActionsList.Add(copyAction);
		}

		public void ResetExistinDefaultLevelsForNewEmployees(IUnitOfWork uow)
		{
			var defaultLevelForNewEmployees = _wageCalculationRepository.AllDefaultLevelForNewEmployees(uow);

			foreach(var defaultLevel in defaultLevelForNewEmployees)
			{
				defaultLevel.IsDefaultLevel = false;
				uow.Save(defaultLevel);
			}
		}

		public void ResetExistinDefaultLevelsForNewEmployeesOnOurCars(IUnitOfWork uow)
		{
			var defaultLevelForOurCars = _wageCalculationRepository.AllDefaultLevelForNewEmployeesOnOurCars(uow);

			foreach(var defaultLevel in defaultLevelForOurCars)
			{
				defaultLevel.IsDefaultLevelForOurCars = false;
				uow.Save(defaultLevel);
			}
		}

		public void ResetExistinDefaultLevelsForNewEmployeesOnRaskatCars(IUnitOfWork uow)
		{
			var defaultLevelForRaskatCars = _wageCalculationRepository.AllDefaultLevelForNewEmployeesOnRaskatCars(uow);

			foreach(var defaultLevel in defaultLevelForRaskatCars)
			{
				defaultLevel.IsDefaultLevelForRaskatCars = false;
				uow.Save(defaultLevel);
			}
		}

		private WageDistrictLevelRates CreateCopy(WageDistrictLevelRates source)
		{
			var copy = (WageDistrictLevelRates)source.Clone();
			copy.Name += " - копия";

			if(copy.Name.Length > WageDistrictLevelRates.NameMaxLength)
			{
				copy.Name = copy.Name.Remove(WageDistrictLevelRates.NameMaxLength);
			}

			return copy;
		}
	}
}
