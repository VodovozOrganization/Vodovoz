using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
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
using System.IO;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
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
		private readonly IInteractiveService _interactiveService;
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
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
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
				typeof(CarVersion));

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
			CarInsurance carInsuranceAlias = null;
			CarInsurance osagoAlias = null;
			CarInsurance kaskoAlias = null;
			Counterparty osagoInsurerAlias = null;
			Counterparty kaskoInsurerAlias = null;
			CarEvent carEventALias = null;

			var lastOsagoInsuranceIdSubquery = QueryOver.Of(() => carInsuranceAlias)
				.Where(() => carInsuranceAlias.Car.Id == carAlias.Id && carInsuranceAlias.InsuranceType == CarInsuranceType.Osago)
				.Select(ins => ins.Id)
				.OrderBy(() => carInsuranceAlias.EndDate)
				.Desc
				.Take(1);

			var lastKaskoInsuranceIdSubquery = QueryOver.Of(() => carInsuranceAlias)
				.Where(() => !carAlias.IsKaskoInsuranceNotRelevant && carInsuranceAlias.Car.Id == carAlias.Id && carInsuranceAlias.InsuranceType == CarInsuranceType.Kasko)
				.Select(ins => ins.Id)
				.OrderBy(() => carInsuranceAlias.EndDate)
				.Desc
				.Take(1);

			var lastCarTechnicalCheckupEventSubquery = QueryOver.Of(() => carEventALias)
				.Where(ce => ce.Car.Id == carAlias.Id
					&& ce.CarEventType.Id == _carEventSettings.CarTechnicalCheckupEventTypeId
					&& ce.CarTechnicalCheckupEndingDate != null)
				.Select(ce => ce.CarTechnicalCheckupEndingDate)
				.OrderBy(ce => ce.CarTechnicalCheckupEndingDate)
				.Desc
				.Take(1);

			var query = uow.Session.QueryOver<Car>(() => carAlias)
				.Inner.JoinAlias(c => c.CarModel, () => carModelAlias)
				.Inner.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias)
				.JoinEntityAlias(() => currentCarVersion,
					() => currentCarVersion.Car.Id == carAlias.Id
						&& currentCarVersion.StartDate <= currentDateTime
						&& (currentCarVersion.EndDate == null || currentCarVersion.EndDate >= currentDateTime))
				.Left.JoinAlias(c => c.Driver, () => driverAlias)
				.Left.JoinAlias(() => currentCarVersion.CarOwnerOrganization, () => organizationAlias)
				.JoinEntityAlias(() => osagoAlias,
					Subqueries.PropertyEq(nameof(osagoAlias.Id), lastOsagoInsuranceIdSubquery.DetachedCriteria),
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => kaskoAlias,
					Subqueries.PropertyEq(nameof(kaskoAlias.Id), lastKaskoInsuranceIdSubquery.DetachedCriteria),
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => osagoAlias.Insurer, () => osagoInsurerAlias)
				.Left.JoinAlias(() => kaskoAlias.Insurer, () => kaskoInsurerAlias);

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

			var osagoMaxEndDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
				NHibernateUtil.DateTime,
				Projections.Property(() => osagoAlias.EndDate),
				Projections.Constant(DateTime.MinValue.ToShortDateString()));

			var kaskoMaxEndDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
				NHibernateUtil.DateTime,
				Projections.Property(() => kaskoAlias.EndDate),
				Projections.Constant(DateTime.MinValue.ToShortDateString()));

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

			#endregion

			#region Проверка наличия и окончания срока действия ГТО

			var carTechnicalCheckupEndDateProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.DateTime, "IFNULL(?1, ?2)"),
				NHibernateUtil.DateTime,
				Projections.SubQuery(lastCarTechnicalCheckupEventSubquery),
				Projections.Constant(DateTime.MinValue.ToShortDateString()));

			var isCarTechnicalCheckupExpiresRestriction = Restrictions.Conjunction()
				.Add(Restrictions.Disjunction()
					.Add(isCompanyCarRestriction)
					.Add(isRaskatCarRestriction))
				.Add(Restrictions.Le(
					carTechnicalCheckupEndDateProjection,
					DateTime.Today.AddDays(_generalSettings.CarTechnicalCheckupEndingNotificationDaysBefore)));
			#endregion

			var isShowBackgroundColorNotificationProjection = Projections.Conditional(
				Restrictions.Conjunction()
					.Add(Restrictions.Not(Restrictions.Eq(Projections.Property(() => carModelAlias.CarTypeOfUse), CarTypeOfUse.Loader)))
					.Add(Restrictions.Not(Restrictions.In(Projections.Property(() => carAlias.Id), _carEventSettings.CarsExcludedFromReportsIds)))
					.Add(Restrictions.Disjunction()
						.Add(isUpcomingOurCarTechInspectAndIsCompanyCarRestriction)
						.Add(isUpcomingRaskatCarTechInspectAndIsRaskatCarRestriction)
						.Add(isOsagoExpiresRestriction)
						.Add(isKaskoExpiresRestriction)
						.Add(isCarTechnicalCheckupExpiresRestriction)),
				Projections.Constant(true),
				Projections.Constant(false));

			if(_filterViewModel.Insurer != null && !_filterViewModel.IsOnlyCarsWithoutInsurer)
			{
				query.Where(() =>
					osagoInsurerAlias.Id == _filterViewModel.Insurer.Id
					|| (!carAlias.IsKaskoInsuranceNotRelevant && kaskoInsurerAlias.Id == _filterViewModel.Insurer.Id));
			}

			if(_filterViewModel.IsOnlyCarsWithoutInsurer)
			{
				query.Where(() =>
					osagoInsurerAlias.Id == null
					|| (!carAlias.IsKaskoInsuranceNotRelevant && kaskoInsurerAlias.Id == null));
			}

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

			if(_filterViewModel.CarOwner != null && !_filterViewModel.IsOnlyCarsWithoutCarOwner)
			{
				query.Where(() => currentCarVersion.CarOwnerOrganization.Id == _filterViewModel.CarOwner.Id);
			}

			if(_filterViewModel.IsOnlyCarsWithoutCarOwner)
			{
				query.Where(Restrictions.IsNull(Projections.Property(() => currentCarVersion.CarOwnerOrganization.Id)));
			}

			if(!(_filterViewModel.IsUsedInDelivery && _filterViewModel.IsNotUsedInDelivery))
			{
				var isUsedInDeliveryRestirctions = Restrictions.Disjunction();

				if(_filterViewModel.IsUsedInDelivery)
				{
					isUsedInDeliveryRestirctions.Add(Restrictions.Eq(Projections.Property(() => carAlias.IsUsedInDelivery), true));
				}

				if(_filterViewModel.IsNotUsedInDelivery)
				{
					isUsedInDeliveryRestirctions.Add(Restrictions.Eq(Projections.Property(() => carAlias.IsUsedInDelivery), false));
				}
				query.Where(isUsedInDeliveryRestirctions);
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.VinFilter))
			{
				query.Where(Restrictions.InsensitiveLike(
					Projections.Property(() => carAlias.VIN), 
					_filterViewModel.VinFilter, 
					MatchMode.Anywhere));
			}

			query.Where(GetSearchCriterion(
				() => carAlias.Id,
				() => carManufacturerAlias.Name,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber,
				() => driverAlias.Name,
				() => driverAlias.LastName,
				() => driverAlias.Patronymic,
				() => organizationAlias.Name,
				() => osagoInsurerAlias.Name,
				() => kaskoInsurerAlias.Name));

			var result = query
				.SelectList(list => list
					.Select(c => c.Id).WithAlias(() => carJournalNodeAlias.Id)
					.Select(() => organizationAlias.Name).WithAlias(() => carJournalNodeAlias.CarOwner)
					.Select(() => carManufacturerAlias.Name).WithAlias(() => carJournalNodeAlias.ManufacturerName)
					.Select(() => carModelAlias.Name).WithAlias(() => carJournalNodeAlias.ModelName)
					.Select(c => c.RegistrationNumber).WithAlias(() => carJournalNodeAlias.RegistrationNumber)
					.Select(c => c.IsArchive).WithAlias(() => carJournalNodeAlias.IsArchive)
					.Select(c => c.VIN).WithAlias(() => carJournalNodeAlias.VIN)
					.Select(isShowBackgroundColorNotificationProjection).WithAlias(() => carJournalNodeAlias.IsShowBackgroundColorNotification)
					.Select(CustomProjections.Concat_WS(" ",
						Projections.Property(() => driverAlias.LastName),
						Projections.Property(() => driverAlias.Name),
						Projections.Property(() => driverAlias.Patronymic)))
					.WithAlias(() => carJournalNodeAlias.DriverName)
					.Select(() => osagoInsurerAlias.Name).WithAlias(() => carJournalNodeAlias.OsagoInsurer)
					.Select(() => kaskoInsurerAlias.Name).WithAlias(() => carJournalNodeAlias.KaskoInsurer))
				.OrderByAlias(() => carJournalNodeAlias.IsShowBackgroundColorNotification).Desc
				.OrderBy(() => carAlias.Id).Asc
				.TransformUsing(Transformers.AliasToBean<CarJournalNode>());

			return result;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			var canCreate = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanCreate;
			var canEdit = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanRead;
			var canDelete = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanDelete;

			var addAction = new JournalAction("Добавить",
				(selected) => canCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => canEdit && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<CarJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
				(selected) => canDelete && selected.Any(),
				(selected) => VisibleDeleteAction,
				(selected) => DeleteEntities(selected.Cast<CarJournalNode>().ToArray()),
				"Delete"
			);
			NodeActionsList.Add(deleteAction);

			var reportActions = new JournalAction("Отчёты",
				(selected) => true,
				(selected) => true				
			);

			reportActions.ChildActionsList.AddRange(
				new[]
				{
					CreateCarInsurancesReportAction(),
					CreateCarTechInspectReportAction(),
					CreateCarsTechnicalCheckupReportAction(),
					CreateExportJournalItemsToExcelAction()
				}
			);

			NodeActionsList.Add(reportActions);			
		}

		private JournalAction CreateCarInsurancesReportAction()
		{
			var selectAction = new JournalAction("Отчёт по страховкам",
				(selected) => true,
				(selected) => true,
				(selected) => CreateCarInsurancesReport()
			);

			return selectAction;			
		}

		private JournalAction CreateCarTechInspectReportAction()
		{
			var selectAction = new JournalAction("Отчёт по ТО",
				(selected) => true,
				(selected) => true,
				(selected) => CreateCarTechInspectReport()
			);

			return selectAction;
		}

		private JournalAction CreateCarsTechnicalCheckupReportAction()
		{
			var selectAction = new JournalAction("Отчёт по ГТО",
				(selected) => true,
				(selected) => true,
				(selected) => CreateCarTechnicalCheckupReport()
			);

			return selectAction;
		}

		private JournalAction CreateExportJournalItemsToExcelAction()
		{
			var selectAction = new JournalAction("Экспорт в Excel",
				(selected) => true,
				(selected) => true,
				(selected) => ExportJournalItemsToExcel()
			);

			return selectAction;
		}

		private void CreateCarInsurancesReport()
		{
			var osagoInsurances = _carRepository.GetActualCarInsurances(UoW, CarInsuranceType.Osago, _carEventSettings.CarsExcludedFromReportsIds).ToList();
			var kaskoInsurances = _carRepository.GetActualCarInsurances(UoW, CarInsuranceType.Kasko, _carEventSettings.CarsExcludedFromReportsIds).ToList();
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
				.GetCarsTechInspectData(UoW, _carEventSettings.TechInspectCarEventTypeId, _carEventSettings.CarsExcludedFromReportsIds)
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

		private void CreateCarTechnicalCheckupReport()
		{
			var carsTechnicalCheckups =
				_carRepository
				.GetCarsTechnicalCheckupData(UoW, _carEventSettings.CarTechnicalCheckupEventTypeId, _carEventSettings.CarsExcludedFromReportsIds)
				.ToList()
				.OrderByDescending(d => d.LastCarTechnicalCheckupEvent is null)
				.ThenBy(d => d.DaysLeftToNextTechnicalCheckup);

			var dialogSettings = GetSaveExcelReportDialogSettings($"{CarTechnicalCheckupReport.ReportTitle}");

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!result.Successful)
			{
				return;
			}

			CarTechnicalCheckupReport.ExportToExcel(
				result.Path,
				carsTechnicalCheckups,
				_generalSettings.CarTechnicalCheckupEndingNotificationDaysBefore);
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

			try
			{
				CarJournalItemsReport.ExportToExcel(result.Path, journalItems);
			}
			catch(IOException ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					"Не удалось сохранить файл выгрузки. Возможно не закрыт предыдущий файл выгрузки", "Ошибка");
			}
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
