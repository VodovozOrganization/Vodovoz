using QS.Banks.Domain;
using QS.BusinessCommon.Domain;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Validation;
using QSBanks;
using QSOrmProject;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.Presentation.ViewModels.Employees.Journals;
using Vodovoz.Presentation.ViewModels.Organisations;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Counterparties.ClientClassification;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Dialogs.Roboats;
using Vodovoz.ViewModels.Fuel.FuelCards;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints.ComplaintResults;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Flyers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.Profitability;
using Vodovoz.ViewModels.Store;

public partial class MainWindow
{

	#region Наша организация

	/// <summary>
	/// Организации
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrganizationsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrganizationJournalViewModel>(null);
	}

	/// <summary>
	/// Подразделения
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnSubdivisionsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SubdivisionsJournalViewModel>(null);
	}

	/// <summary>
	/// Центры финансовой ответственности
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnFinancialResponsibilityCenterJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FinancialResponsibilityCenterJournalViewModel>(null);
	}

	/// <summary>
	/// Склады
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionWarehousesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenTdiTab<WarehousesView>(null);
	}

	#region Зарплата

	/// <summary>
	/// Зарплатные районы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionWageDistrictActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WageDistrictsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Ставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRatesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WageDistrictLevelRatesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Планы продаж
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSalesPlansActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SalesPlanJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Виды оформления
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnEmployeeRegistrationsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmployeeRegistrationsJournalViewModel>(null);
	}

	#endregion Зарплата

	/// <summary>
	/// Сотрудники
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEmployeeActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmployeesJournalViewModel, Action<EmployeeFilterViewModel>>(null,
			filter =>
			{
				filter.Status = EmployeeStatus.IsWorking;
			}, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Национальность
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionNationalityActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Nationality));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Иностранное гражданство
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCitizenshipActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Citizenship));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Источники рекламаций
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionComplaintSourcesActivated(object sender, EventArgs e)
	{
		var complaintSourcesViewModel = new SimpleEntityJournalViewModel<ComplaintSource, ComplaintSourceViewModel>(
			x => x.Name,
			() => new ComplaintSourceViewModel(EntityUoWBuilder.ForCreate(), ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices),
			(node) => new ComplaintSourceViewModel(EntityUoWBuilder.ForOpen(node.Id), ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices),
			 ServicesConfig.UnitOfWorkFactory,
			ServicesConfig.CommonServices
		);
		tdiMain.AddTab(complaintSourcesViewModel);
	}

	#region Результаты рассмотрения рекламаций

	/// <summary>
	/// Результаты рассмотрения рекламаций по клиенту
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnComplaintResultsOfCounterpartyActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintResultsOfCounterpartyJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Результаты рассмотрения рекламаций по сотрудникам
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnComplaintResultsOfEmployeesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintResultsOfEmployeesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Результаты рассмотрения рекламаций

	/// <summary>
	/// Объекты рекламаций
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionComplaintObjectActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintObjectJournalViewModel, Action<ComplaintObjectJournalFilterViewModel>>(null, filter => filter.HidenByDefault = true);
	}

	/// <summary>
	/// Виды рекламаций
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionComplaintKindActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintKindJournalViewModel, Action<ComplaintKindJournalFilterViewModel>>(
			null,
			filter =>
			{
				filter.HidenByDefault = true;
			});
	}

	/// <summary>
	/// Детализация видов рекламаций
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionComplaintDetalizationJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintDetalizationJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Источники проблем
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUndeliveryProblemSourcesActivated(object sender, EventArgs e)
	{
		var undeliveryProblemSourcesViewModel = new SimpleEntityJournalViewModel<UndeliveryProblemSource, UndeliveryProblemSourceViewModel>(
			x => x.Name,
			() => new UndeliveryProblemSourceViewModel(
				EntityUoWBuilder.ForCreate(),
				ServicesConfig.UnitOfWorkFactory,
				ServicesConfig.CommonServices
			),
			(node) => new UndeliveryProblemSourceViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				ServicesConfig.UnitOfWorkFactory,
				ServicesConfig.CommonServices
			),
			ServicesConfig.UnitOfWorkFactory,
			ServicesConfig.CommonServices
		);
		undeliveryProblemSourcesViewModel.SetActionsVisible(deleteActionEnabled: false);
		tdiMain.AddTab(undeliveryProblemSourcesViewModel);
	}

	/// <summary>
	/// Причины рекламаций водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDriversComplaintReasonsJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DriverComplaintReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Ответственные за рекламации
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionResponsibleActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ResponsibleJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Типы телефонов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPhoneTypesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PhoneTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Типы e-mail адресов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEMailTypesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmailTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Причины отписки от рассылки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnUnsubscribingReasonsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<BulkEmailEventReasonJournalViewModel>(null);
	}

	/// <summary>
	/// Справочники Roboats
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnRoboatsExportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboatsCatalogExportViewModel>(null);
	}

	/// <summary>
	/// Справочники Roboats
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnInnerPhonesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InnerPhonesJournalViewModel>(null);
	}

	#endregion Наша организация

	#region ТМЦ

	/// <summary>
	/// Номенклатура
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNomenclatureActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(null);
	}

	#region Инвентарный учет

	/// <summary>
	/// Экземпляры номенклатур
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnInventoryInstancesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(null);
	}

	/// <summary>
	/// Номенклатуры с инвентарным учетом
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnInventoryNomenclaturesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryNomenclaturesJournalViewModel>(null);
	}

	#endregion Инвентарный учет

	/// <summary>
	/// Единицы измерения
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionUnitsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(MeasurementUnits));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Группы товаров
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionProductGroupsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ProductGroupsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Папки номенклатуры в 1с
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionFolders1cActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Folder1c>(),
			() => new OrmReference(typeof(Folder1c))
		);
	}

	/// <summary>
	/// Промонаборы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPromotionalSetsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PromotionalSetsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Настройка запаса и радиуса
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionAdditionalLoadSettingsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<AdditionalLoadingSettingsViewModel>(null);
	}

	/// <summary>
	/// Групповое заполнение себестоимости
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ActionGroupPricingActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclatureGroupPricingViewModel>(null);
	}

	/// <summary>
	/// Оборудование
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить или удалить, не активно")]
	protected void OnActionEquipmentActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Equipment));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Виды оборудования
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEquipmentKindsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EquipmentKindJournalViewModel>(null);
	}

	/// <summary>
	/// Производители оборудования
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionManufacturersActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Manufacturer));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Цвета оборудования
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionColorsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(EquipmentColors));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Спецификация продукции
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionProductSpecificationActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(ProductSpecification));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Сертификаты продукции
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCertificatesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Certificate>(),
			() => new OrmReference(typeof(Certificate))
		);
	}

	/// <summary>
	/// Шаблоны для пересортицы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionRegrandingOfGoodsTempalteActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RegradingOfGoodsTemplateJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Категории выбраковки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCullingCategoryActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CullingCategory));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Фуры
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionTransportationWagonActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<MovementWagonJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Пакеты бесплатной аренды
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFreeRentPackageActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FreeRentPackagesJournalViewModel>(null);
	}

	/// <summary>
	/// Условия платной аренды
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPaidRentPackageActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PaidRentPackagesJournalViewModel>(null);
	}

	/// <summary>
	/// Основания для скидок
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDiscountReasonsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DiscountReasonJournalViewModel>(null);
	}

	/// <summary>
	/// Причины несдачи тары
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNonReturnReasonsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NonReturnReasonJournalViewModel>(null);
	}

	/// <summary>
	/// Причины забора тары
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionReturnTareReasonsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ReturnTareReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Категории забора тары
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionReturnTareReasonCategoriesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ReturnTareReasonCategoriesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Рекламные листовки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFlyersActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FlyersJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// ИПЗ - Онлайн каталоги - Онлайн каталоги сайта ВВ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnVodovozWebSiteNomenclatureOnlineCatalogsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<VodovozWebSiteNomenclatureOnlineCatalogsJournalViewModel>(null);
	}

	/// <summary>
	/// ИПЗ - Онлайн каталоги - Онлайн каталоги мобильного приложения
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnMobileAppNomenclatureOnlineCatalogsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<MobileAppNomenclatureOnlineCatalogsJournalViewModel>(null);
	}

	/// <summary>
	/// ИПЗ - Онлайн каталоги - Онлайн каталоги сайта Кулер Сэйл
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnKulerSaleWebSiteNomenclatureOnlineCatalogsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<KulerSaleWebSiteNomenclatureOnlineCatalogsJournalViewModel>(null);
	}

	/// <summary>
	/// ИПЗ - Группы товаров в ИПЗ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnNomenclatureOnlineGroupsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclatureOnlineGroupsJournalViewModel>(null);
	}

	/// <summary>
	/// ИПЗ - Типы товаров в ИПЗ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnNomenclatureOnlineCategoriesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclatureOnlineCategoriesJournalViewModel>(null);
	}

	#endregion ТМЦ

	#region Банки/Операторы ЭДО

	/// <summary>
	/// Банки РФ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionBanksRFActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Bank));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Обновить с сайта Центрального банка
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUpdateBanksFromCBRActivated(object sender, EventArgs e)
	{
		BanksUpdater.CheckBanksUpdate(true);
	}

	/// <summary>
	/// Операторы ЭДО
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionEdoOperatorsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EdoOperatorsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Банки/Операторы ЭДО

	#region Финансы

	/// <summary>
	/// Статьи доходов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction14Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<IncomeCategoryJournalViewModel>(null);
	}

	/// <summary>
	/// Статьи расходов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction15Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExpenseCategoryJournalViewModel>(null);
	}

	/// <summary>
	/// Константы для рентабельности
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnProfitabilityConstantsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ProfitabilityConstantsViewModel, IValidator>(
			null, ServicesConfig.ValidationService, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Финансовые статьи
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFinancialCategoriesGroupsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FinancialCategoriesGroupsJournalViewModel>(null);
	}

	/// <summary>
	/// Категории штрафов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFineCategoryJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FineCategoryJournalViewModel>(null);
	}

	protected void OnCompanyBalanceByDateActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CompanyBalanceByDateViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());
	}

	protected void OnBusinessActivitiesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<BusinessActivitiesJournalViewModel>(null);

	}

	protected void OnBusinesAccountsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<BusinessAccountsJournalViewModel>(null);

	}

	protected void OnFundsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FundsJournalViewModel>(null);
	}

	#endregion Финансы

	#region Контрагенты

	/// <summary>
	/// Контрагенты
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyHandbookActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CounterpartyJournalViewModel, Action<CounterpartyJournalFilterViewModel>>(null, filter => filter.IsForRetail = false, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Точки доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveryPointsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DeliveryPointJournalViewModel, bool, bool>(null, true, true);
	}

	/// <summary>
	/// Откуда клиент
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCameFromActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ClientCameFromJournalViewModel, Action<ClientCameFromFilterViewModel>>(
			null,
			filter => filter.HidenByDefault = true,
			OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Типы объектов в точках доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionDeliveryPointCategoryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryPointCategory>(),
			() => new OrmReference(typeof(DeliveryPointCategory))
		);
	}

	/// <summary>
	/// Виды деятельности контрагента
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCounterpartyActivityKindsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<CounterpartyActivityKind>(),
			() => new OrmReference(typeof(CounterpartyActivityKind))
		);
	}

	/// <summary>
	/// Типы ответственных за точку доставки лиц
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionResponsiblePersonTypesJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DeliveryPointResponsiblePersonTypeJournalViewModel>(null);
	}

	/// <summary>
	/// Каналы сбыта
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionSalesChannelsJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SalesChannelJournalViewModel>(null);
	}

	/// <summary>
	/// Формы собственности контрагентов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOrganizationOwnershipTypeActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrganizationOwnershipTypeJournalViewModel>(null);
	}

	/// <summary>
	/// Должности сотрудников контрагента
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCounterpartyPostActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Post));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Имена контрагентов Roboats
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRoboAtsCounterpartyNameActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboAtsCounterpartyNameJournalViewModel>(null);
	}

	/// <summary>
	/// Отчества контрагентов Roboats
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRoboAtsCounterpartyPatronymicActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboAtsCounterpartyPatronymicJournalViewModel>(null);
	}

	/// <summary>
	/// Сопоставление клиентов из внешних источников
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnExternalCounterpartiesMatchingActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExternalCounterpartiesMatchingJournalViewModel>(null);
	}

	/// <summary>
	/// Подтипы контрагентов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartySubtypesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SubtypesJournalViewModel>(null);
	}

	/// <summary>
	/// Пересчёт классификации контрагентов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCounterpartyClassificationCalculationActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CounterpartyClassificationCalculationViewModel>(null);
	}

	#endregion Контрагенты

	#region Логистика

	/// <summary>
	/// Графики доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveryScheduleActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DeliveryScheduleJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Правила для цен доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionDeliveryPriceRulesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DeliveryPriceRuleJournalViewModel>(null);
	}

	/// <summary>
	/// Тарифные зоны
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionTariffZonesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<TariffZoneJournalViewModel>(null);
	}

	/// <summary>
	/// График работы водителя
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionDeliveryDayScheduleActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryDaySchedule>(),
			() => new OrmReference(typeof(DeliveryDaySchedule))
		);
	}

	/// <summary>
	/// Смена доставки
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionDeliveryShiftActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(DeliveryShift));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Автомобили
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCarsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarJournalViewModel>(null);
	}

	/// <summary>
	/// Виды топлива
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelTypeActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FuelTypeJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Модели автомобилей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCarModelsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarModelJournalViewModel>(null);
	}

	/// <summary>
	/// Производители автомобилей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCarManufacturersActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarManufacturerJournalViewModel>(null);
	}

	/// <summary>
	/// Колонки номенклатуры
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRouteColumnsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RouteColumnJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Причины опозданий водителей
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionLateArrivalReasonsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<LateArrivalReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Виды событий ТС
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionCarEventTypeActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarEventTypeJournalViewModel>(null);
	}

	/// <summary>
	/// Причины пересортицы
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionRegradingOfGoodsReasonsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RegradingOfGoodsReasonsJournalViewModel>(null);
	}

	/// <summary>
	/// Топливные карты
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionFuelCardsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FuelCardJournalViewModel>(null);
	}

	/// <summary>
	/// Причины списания километража
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionMileageWriteOffReasonsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<MileageWriteOffReasonJournalViewModel>(null);
	}

	#region События нахождения на складе водителей

	/// <summary>
	/// События
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnDriversWarehousesEventsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DriversWarehousesEventsJournalViewModel>(null);
	}

	/// <summary>
	/// Завершенные события
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnCompletedDriversWarehousesEventsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CompletedDriversWarehousesEventsJournalViewModel>(null);
	}

	#endregion

	#endregion Логистика

	#region Помощники

	/// <summary>
	/// Шаблоны комментариев
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CommentTemplate));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Шаблоны комментариев для штрафов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[Obsolete("Старый диалог, заменить")]
	protected void OnActionFineCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(FineTemplate));
		tdiMain.AddTab(refWin);
	}

	/// <summary>
	/// Шаблоны комментариев для премий
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAction47Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PremiumTemplateJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}
	
	/// <summary>
	/// Настройка текстов пуш-уведомлений
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPushNotificationTextSettingsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OnlineOrderNotificationSettingJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}
	
	#endregion Помощники

	#region Заказы

	/// <summary>
	/// Типы оплаты по карте
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionPaymentsFromActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PaymentsFromJournalViewModel>(null);
	}

	/// <summary>
	/// План продаж для КЦ
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionNomenclaturePlanActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclaturesPlanJournalViewModel, Action<NomenclaturePlanFilterViewModel>>(null, filter => filter.HidenByDefault = true);
	}

	/// <summary>
	/// Причины отсутствия переноса
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUndeliveryTransferAbsenceReasonActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveryTransferAbsenceReasonJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Объекты недовозов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUndeliveryObjectActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveryObjectJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Виды недовозов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUndeliveryKindActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveryKindJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	/// <summary>
	/// Детализация недовозов
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionUndeliveryDetalizationActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UndeliveryDetalizationJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnOrdersRatingReasonsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrdersRatingReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnOnlineOrdersCancellationReasonsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OnlineOrderCancellationReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	#endregion Заказы
}
