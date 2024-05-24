using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarJournalViewModel : EntityJournalViewModelBase<Car, CarViewModel, CarJournalNode>
	{
		private readonly CarJournalFilterViewModel _filterViewModel;
		private readonly IGeneralSettings _generalSettings;
		private readonly IFileDialogService _fileDialogService;
		private readonly ICarRepository _carRepository;

		public CarJournalViewModel(
			CarJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			IGeneralSettings generalSettings,
			IFileDialogService fileDialogService,
			ICarRepository carRepository,
			Action<CarJournalFilterViewModel> filterConfiguration = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel
				?? throw new ArgumentNullException(nameof(filterViewModel));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			filterViewModel.Journal = this;

			JournalFilter = filterViewModel;

			if(filterConfiguration != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfiguration);
			}

			UseSlider = true;
			TabName = "Журнал автомобилей";

			UpdateOnChanges(
				typeof(Car),
				typeof(CarModel),
				typeof(Employee),
				typeof(CarVersion)
				);

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<Car> ItemsQuery(IUnitOfWork uow)
		{
			var currentDateTime = DateTime.Now;

			CarJournalNode carJournalNodeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			CarVersion currentCarVersion = null;
			Employee driverAlias = null;
			CarManufacturer carManufacturerAlias = null;

			var query = uow.Session.QueryOver<Car>(() => carAlias)
				.Inner.JoinAlias(c => c.CarModel, () => carModelAlias)
				.Inner.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias)
				.JoinEntityAlias(() => currentCarVersion,
					() => currentCarVersion.Car.Id == carAlias.Id
						&& currentCarVersion.StartDate <= currentDateTime
						&& (currentCarVersion.EndDate == null || currentCarVersion.EndDate >= currentDateTime))
				.Left.JoinAlias(c => c.Driver, () => driverAlias);

			#region Проверка приближающегося ТО

			var isCompanyCarRestriction = Restrictions.Eq(Projections.Property(() => currentCarVersion.CarOwnType), CarOwnType.Company);
			var isTechInspectForOurCarsUpcomingRestriction = Restrictions.Le(Projections.Property(() => carAlias.LeftUntilTechInspect), _generalSettings.UpcomingTechInspectForOurCars);
			var isUpcomingOurCarTechInspectAndIsCompanyCarRestriction = Restrictions.Conjunction()
				.Add(isCompanyCarRestriction)
				.Add(isTechInspectForOurCarsUpcomingRestriction);

			var isRaskatCarRestriction = Restrictions.Eq(Projections.Property(() => currentCarVersion.CarOwnType), CarOwnType.Raskat);
			var isTechInspectForRaskatCarsUpcomingRestriction = Restrictions.Le(Projections.Property(() => carAlias.LeftUntilTechInspect), _generalSettings.UpcomingTechInspectForRaskatCars);
			var isUpcomingRaskatCarTechInspectAndIsRaskatCarRestriction = Restrictions.Conjunction()
				.Add(isRaskatCarRestriction)
				.Add(isTechInspectForRaskatCarsUpcomingRestriction);

			var upcomingTechInspectProjection = Projections.Conditional(
				Restrictions.Disjunction()
					.Add(isUpcomingOurCarTechInspectAndIsCompanyCarRestriction)
					.Add(isUpcomingRaskatCarTechInspectAndIsRaskatCarRestriction),
				Projections.Constant(true),
				Projections.Constant(false)
				);

			#endregion

			if(_filterViewModel.Archive != null)
			{
				query.Where(c => c.IsArchive == _filterViewModel.Archive);
			}

			if(_filterViewModel.VisitingMasters != null)
			{
				if(_filterViewModel.VisitingMasters.Value)
				{
					query.Where(() => driverAlias.VisitingMaster);
				}
				else
				{
					query.Where(Restrictions.Disjunction()
						.Add(Restrictions.IsNull(Projections.Property(() => driverAlias.Id)))
						.Add(() => !driverAlias.VisitingMaster));
				}
			}

			if(_filterViewModel.RestrictedCarTypesOfUse != null)
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(_filterViewModel.RestrictedCarTypesOfUse.ToArray());
			}

			if(_filterViewModel.ExcludedCarTypesOfUse != null && _filterViewModel.ExcludedCarTypesOfUse.Any())
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).Not.IsIn(_filterViewModel.ExcludedCarTypesOfUse.ToArray());
			}

			if(_filterViewModel.RestrictedCarOwnTypes != null)
			{
				query.WhereRestrictionOn(() => currentCarVersion.CarOwnType).IsIn(_filterViewModel.RestrictedCarOwnTypes.ToArray());
			}

			if(_filterViewModel.CarModel != null)
			{
				query.Where(() => carModelAlias.Id == _filterViewModel.CarModel.Id);
			}

			query.Where(GetSearchCriterion(
				() => carAlias.Id,
				() => carManufacturerAlias.Name,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber,
				() => driverAlias.Name,
				() => driverAlias.LastName,
				() => driverAlias.Patronymic));

			var result = query
				.SelectList(list => list
					.Select(c => c.Id).WithAlias(() => carJournalNodeAlias.Id)
					.Select(() => carManufacturerAlias.Name).WithAlias(() => carJournalNodeAlias.ManufacturerName)
					.Select(() => carModelAlias.Name).WithAlias(() => carJournalNodeAlias.ModelName)
					.Select(c => c.RegistrationNumber).WithAlias(() => carJournalNodeAlias.RegistrationNumber)
					.Select(c => c.IsArchive).WithAlias(() => carJournalNodeAlias.IsArchive)
					.Select(upcomingTechInspectProjection).WithAlias(() => carJournalNodeAlias.IsUpcomingTechInspect)
					.Select(CustomProjections.Concat_WS(" ",
						Projections.Property(() => driverAlias.LastName),
						Projections.Property(() => driverAlias.Name),
						Projections.Property(() => driverAlias.Patronymic)))
					.WithAlias(() => carJournalNodeAlias.DriverName))
				.OrderByAlias(() => carJournalNodeAlias.IsUpcomingTechInspect).Desc
				.OrderBy(() => carAlias.Id).Asc
				.TransformUsing(Transformers.AliasToBean<CarJournalNode>());

			return result;
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			CreateCarInsurancesReportAction();
		}

		private void CreateCarInsurancesReportAction()
		{
			var selectAction = new JournalAction("Отчёт по страховкам",
				(selected) => true,
				(selected) => true,
				(selected) => CreateCarInsurancesReport()
			);
			NodeActionsList.Add(selectAction);
		}

		private void CreateCarInsurancesReport()
		{
			var ins = _carRepository.GetActualCarInsuranceData(UoW).ToList();
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
