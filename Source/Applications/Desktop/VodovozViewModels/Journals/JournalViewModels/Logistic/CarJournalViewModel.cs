using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
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
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Car;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Logistics;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic.Cars;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.Cars;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarJournalViewModel : EntityJournalViewModelBase<Car, CarViewModel, CarJournalNode>
	{
		private readonly CarJournalFilterViewModel _filterViewModel;
		private readonly IGeneralSettings _generalSettings;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IFileDialogService _fileDialogService;
		private readonly ICarRepository _carRepository;
		private readonly ICarInsuranceSettings _carInsuranceSettings;

		public CarJournalViewModel(
			CarJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			IGeneralSettings generalSettings,
			ICarEventSettings carEventSettings,
			IFileDialogService fileDialogService,
			ICarRepository carRepository,
			ICarInsuranceSettings carInsuranceSettings,
			Action<CarJournalFilterViewModel> filterConfiguration = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel
				?? throw new ArgumentNullException(nameof(filterViewModel));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_carInsuranceSettings = carInsuranceSettings ?? throw new ArgumentNullException(nameof(carInsuranceSettings));
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

		public ILifetimeScope LifetimeScope { get; }

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
			Organization organizationAlias = null;

			var query = uow.Session.QueryOver<Car>(() => carAlias)
				.Inner.JoinAlias(c => c.CarModel, () => carModelAlias)
				.Inner.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias)
				.JoinEntityAlias(() => currentCarVersion,
					() => currentCarVersion.Car.Id == carAlias.Id
						&& currentCarVersion.StartDate <= currentDateTime
						&& (currentCarVersion.EndDate == null || currentCarVersion.EndDate >= currentDateTime))
				.Left.JoinAlias(c => c.Driver, () => driverAlias)
				.Left.JoinAlias(() => currentCarVersion.CarOwnerOrganization, () => organizationAlias);

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

			#region Проверка наличия и окончания срока действия страховки

			var osagoMaxEndDateSubquery = QueryOver.Of<CarInsurance>()
				.Where(ins => ins.Car.Id == carAlias.Id && ins.InsuranceType == CarInsuranceType.Osago)
				.Select(Projections.Max<CarInsurance>(ins => ins.EndDate));

			var kaskoMaxEndDateSubquery = QueryOver.Of<CarInsurance>()
				.Where(ins => ins.Car.Id == carAlias.Id && ins.InsuranceType == CarInsuranceType.Kasko)
				.Select(Projections.Max<CarInsurance>(ins => ins.EndDate));

			var osagoMaxEndDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
				NHibernateUtil.DateTime,
				Projections.SubQuery(osagoMaxEndDateSubquery),
				Projections.Constant(DateTime.MinValue.ToShortDateString())
				);

			var kaskoMaxEndDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
				NHibernateUtil.DateTime,
				Projections.SubQuery(kaskoMaxEndDateSubquery),
				Projections.Constant(DateTime.MinValue.ToShortDateString())
				);

			var isOsagoExpiresRestriction = Restrictions.Conjunction()
				.Add(Restrictions.Disjunction()
					.Add(isCompanyCarRestriction)
					.Add(isRaskatCarRestriction))
				.Add(Restrictions.Le(
					osagoMaxEndDateProjection,
					DateTime.Today.AddDays(_carInsuranceSettings.OsagoEndingNotifyDaysBefore)));

			var isKaskoExpiresRestriction = Restrictions.Conjunction()
				.Add(Restrictions.Disjunction()
					.Add(isCompanyCarRestriction)
					.Add(isRaskatCarRestriction))
				.Add(Restrictions.Le(
					kaskoMaxEndDateProjection,
					DateTime.Today.AddDays(_carInsuranceSettings.KaskoEndingNotifyDaysBefore)))
				.Add(() => !carAlias.IsKaskoInsuranceNotRelevant);

			var isOsagoExpiresProjection = Projections.Conditional(
				isOsagoExpiresRestriction,
				Projections.Constant(true),
				Projections.Constant(false)
				);

			var isKaskoExpiresProjection = Projections.Conditional(
				isKaskoExpiresRestriction,
				Projections.Constant(true),
				Projections.Constant(false)
				);

			#endregion

			var isShowBackgroundColorNotificationProjection = Projections.Conditional(
				Restrictions.Disjunction()
					.Add(isUpcomingOurCarTechInspectAndIsCompanyCarRestriction)
					.Add(isUpcomingRaskatCarTechInspectAndIsRaskatCarRestriction)
					.Add(isOsagoExpiresRestriction)
					.Add(isKaskoExpiresRestriction),
				Projections.Constant(true),
				Projections.Constant(false)
				);

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
					.Select(() => organizationAlias.Name).WithAlias(() => carJournalNodeAlias.CarOwner)
					.Select(() => carManufacturerAlias.Name).WithAlias(() => carJournalNodeAlias.ManufacturerName)
					.Select(() => carModelAlias.Name).WithAlias(() => carJournalNodeAlias.ModelName)
					.Select(c => c.RegistrationNumber).WithAlias(() => carJournalNodeAlias.RegistrationNumber)
					.Select(c => c.IsArchive).WithAlias(() => carJournalNodeAlias.IsArchive)
					.Select(upcomingTechInspectProjection).WithAlias(() => carJournalNodeAlias.IsUpcomingTechInspect)
					.Select(isOsagoExpiresProjection).WithAlias(() => carJournalNodeAlias.IsOsagoInsuranceExpires)
					.Select(isKaskoExpiresProjection).WithAlias(() => carJournalNodeAlias.IsKaskoInsuranceExpires)
					.Select(isShowBackgroundColorNotificationProjection).WithAlias(() => carJournalNodeAlias.IsShowBackgroundColorNotification)
					.Select(CustomProjections.Concat_WS(" ",
						Projections.Property(() => driverAlias.LastName),
						Projections.Property(() => driverAlias.Name),
						Projections.Property(() => driverAlias.Patronymic)))
					.WithAlias(() => carJournalNodeAlias.DriverName)
					.Select(Projections.Constant("Тестовый страховщик")).WithAlias(() => carJournalNodeAlias.Insurer))
				.ThenByAlias(() => carJournalNodeAlias.IsShowBackgroundColorNotification).Desc
				.OrderBy(() => carAlias.Id).Asc
				.TransformUsing(Transformers.AliasToBean<CarJournalNode>());

			return result;
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			CreateCarInsurancesReportAction();
			CreateCarTechInspectReportAction();
			ExportJournalItemsToExcelAction();
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

		private void CreateCarTechInspectReportAction()
		{
			var selectAction = new JournalAction("Отчёт по ТО",
				(selected) => true,
				(selected) => true,
				(selected) => CreateCarTechInspectReport()
			);
			NodeActionsList.Add(selectAction);
		}

		private void ExportJournalItemsToExcelAction()
		{
			var selectAction = new JournalAction("Экспорт в Excel",
				(selected) => true,
				(selected) => true,
				(selected) => ExportJournalItemsToExcel()
			);
			NodeActionsList.Add(selectAction);
		}

		private void CreateCarInsurancesReport()
		{
			var osagoInsurances = _carRepository.GetActualCarInsurances(UoW, CarInsuranceType.Osago).ToList();
			var kaskoInsurances = _carRepository.GetActualCarInsurances(UoW, CarInsuranceType.Kasko).ToList();
			var insurances = osagoInsurances
				.Union(kaskoInsurances.Where(ins => !ins.IsKaskoNotRelevant))
				.OrderByDescending(ins => ins.LastInsurance is null)
				.ThenBy(ins => ins.DaysToExpire)
				.ToList();

			var dialogSettings = GetSaveExcelReportDialogSettings($"{CarInsurancesReport.ReportTitle}");

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			CarInsurancesReport.ExportToExcel(result.Path, insurances);
		}

		private void CreateCarTechInspectReport()
		{
			var techInspects =
				_carRepository
				.GetCarsTechInspectData(UoW, _carEventSettings.TechInspectCarEventTypeId)
				.OrderBy(ti => ti.LeftUntilTechInspectKm)
				.ToList();

			var dialogSettings = GetSaveExcelReportDialogSettings($"{CarTechInspectReport.ReportTitle}");

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			CarTechInspectReport.ExportToExcel(result.Path, techInspects);
		}

		private void ExportJournalItemsToExcel()
		{
			var journalItems = ItemsQuery(UoW).List<CarJournalNode>();

			var dialogSettings = GetSaveExcelReportDialogSettings($"{CarJournalItemsReport.ReportTitle}");

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			CarJournalItemsReport.ExportToExcel(result.Path, journalItems);
		}

		private DialogSettings GetSaveExcelReportDialogSettings(string fileName)
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{fileName}.xlsx";
			dialogSettings.FileFilters.Clear();
			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			return dialogSettings;
		}
		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
