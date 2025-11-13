using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using QS.DomainModel.Entity;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.CommonSettings;
using Vodovoz.Settings.Car;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Fuel;
using Vodovoz.Settings.Organizations;
using Vodovoz.ViewModels.Accounting.Payments;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.Services;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class GeneralSettingsViewModel : TabViewModelBase
	{
		private const int _carLoadDocumentInfoStringMaxLength = 80;
		private const int _billAdditionalInfoMaxLength = 140;

		private readonly IGeneralSettings _generalSettings;
		private readonly IFuelControlSettings _fuelControlSettings;
		private readonly ICarInsuranceSettings _carInsuranceSettings;
		private readonly ILogger<GeneralSettingsViewModel> _logger;
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private ILifetimeScope _lifetimeScope;
		private readonly ViewModelEEVMBuilder<Organization> _organizationViewModelBuilder;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IValidator _validator;
		private const int _routeListPrintedFormPhonesLimitSymbols = 500;

		private string _routeListPrintedFormPhones;
		private bool _canAddForwardersToLargus;
		private bool _canAddForwardersToMinivan;
		private DelegateCommand _saveRouteListPrintedFormPhonesCommand;
		private DelegateCommand _saveCanAddForwardersToLargusCommand;
		private DelegateCommand _saveCanAddForwardersToMinivanCommand;
		private DelegateCommand _saveOrderAutoCommentCommand;
		private DelegateCommand _showAutoCommentInfoCommand;
		private string _orderAutoComment;

		private DelegateCommand _saveDriversStopListPropertiesCommand;
		private readonly bool _canEditDriversStopListSettings;
		private int _driversUnclosedRouteListsHavingDebtCount;
		private decimal _driversRouteListsDebtMaxSum;

		private DelegateCommand _saveSecondOrderDiscountAvailabilityCommand;
		private readonly bool _canActivateClientsSecondOrderDiscount;
		private bool _isClientsSecondOrderDiscountActive;

		private bool _isOrderWaitUntilActive;

		private string _billAdditionalInfo;
		private string _carLoadDocumentInfoString;
		private bool _isFastDelivery19LBottlesLimitActive;
		private int _fastDelivery19LBottlesLimitCount;
		private int _upcomingTechInspectForOurCars;
		private int _upcomingTechInspectForRaskatCars;

		private FastDeliveryIntervalFromEnum _fastDeliveryIntervalFrom;
		private bool _isIntervalFromOrderCreated;
		private bool _isIntervalFromAddedInFirstRouteList;
		private bool _isIntervalFromRouteListItemTransfered;
		private int _fastDeliveryMaximumPermissibleLateMinutes;

		private int _largusMaxDailyFuelLimit;
		private int _truckMaxDailyFuelLimit;
		private int _gazelleMaxDailyFuelLimit;
		private int _loaderMaxDailyFuelLimit;
		private int _minivanMaxDailyFuelLimit;
		private int _osagoEndingNotifyDaysBefore;
		private int _kaskoEndingNotifyDaysBefore;
		private int _carTechnicalCheckupEndingNotifyDaysBefore;

		public GeneralSettingsViewModel(
			ILogger<GeneralSettingsViewModel> logger,
			IGeneralSettings generalSettings,
			IFuelControlSettings fuelControlSettings,
			ICarInsuranceSettings carInsuranceSettings,
			ICommonServices commonServices,
			RoboatsSettingsViewModel roboatsSettingsViewModel,
			RecomendationSettingsViewModel recomendationSettingsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope lifetimeScope,
			INavigationManager navigation,
			ViewModelEEVMBuilder<Organization> organizationViewModelBuilder,
			EntityJournalOpener entityJournalOpener,
			IOrganizationForOrderFromSet organizationForOrderFromSet,
			IOrganizationSettings organizationSettings,
			IValidator validator) : base(commonServices?.InteractiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			RoboatsSettingsViewModel = roboatsSettingsViewModel ?? throw new ArgumentNullException(nameof(roboatsSettingsViewModel));
			EntityJournalOpener = entityJournalOpener ?? throw new ArgumentNullException(nameof(entityJournalOpener));
			RecomendationSettingsViewModel = recomendationSettingsViewModel ?? throw new ArgumentNullException(nameof(recomendationSettingsViewModel));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_organizationViewModelBuilder =
				organizationViewModelBuilder ?? throw new ArgumentNullException(nameof(organizationViewModelBuilder));
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			OrganizationForOrderFromSet = 
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_carInsuranceSettings = carInsuranceSettings ?? throw new ArgumentNullException(nameof(carInsuranceSettings));
			TabName = "Общие настройки";

			RouteListPrintedFormPhones = _generalSettings.GetRouteListPrintedFormPhones;
			CanAddForwardersToLargus = _generalSettings.GetCanAddForwardersToLargus;
			CanAddForwardersToMinivan = _generalSettings.GetCanAddForwardersToMinivan;
			CanEditRouteListPrintedFormPhones =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_route_List_printed_form_phones");
			CanEditCanAddForwardersToLargus =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_can_add_forwarders_to_largus");
			CanEditCanAddForwardersToMinivan =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_can_add_forwarders_to_minivan");
			CanEditOrderAutoComment =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_order_auto_comment_setting");
			OrderAutoComment = _generalSettings.OrderAutoComment;

			InitializeSettingsViewModels();

			_canEditDriversStopListSettings = _commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_drivers_stop_list_parameters");
			_driversUnclosedRouteListsHavingDebtCount = _generalSettings.DriversUnclosedRouteListsHavingDebtMaxCount;
			_driversRouteListsDebtMaxSum = _generalSettings.DriversRouteListsMaxDebtSum;

			_canActivateClientsSecondOrderDiscount =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanActivateClientsSecondOrderDiscount);
			_isClientsSecondOrderDiscountActive = _generalSettings.GetIsClientsSecondOrderDiscountActive;

			_isOrderWaitUntilActive = _generalSettings.GetIsOrderWaitUntilActive;
			CanEditOrderWaitUntilSetting = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanEditOrderWaitUntil);
			SaveOrderWaitUntilActiveCommand = new DelegateCommand(SaveIsEditOrderWaitUntilActive, () => CanEditOrderWaitUntilSetting);

			_isFastDelivery19LBottlesLimitActive = _generalSettings.IsFastDelivery19LBottlesLimitActive;
			_fastDelivery19LBottlesLimitCount = _generalSettings.FastDelivery19LBottlesLimitCount;
			CanEditFastDelivery19LBottlesLimitSetting = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanEditFastDelivery19LBottlesLimit);
			SaveFastDelivery19LBottlesLimitActiveCommand = new DelegateCommand(SaveIsFastDelivery19LBottlesLimitActive, () => CanEditFastDelivery19LBottlesLimitSetting);

			_billAdditionalInfo = _generalSettings.GetBillAdditionalInfo;
			CanSaveBillAdditionalInfo = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.Documents.CanEditBillAdditionalInfo);
			SaveBillAdditionalInfoCommand = new DelegateCommand(SaveBillAdditionalInfo, () => CanSaveBillAdditionalInfo);

			InitializeEmployeesFixedPricesViewModel();

			_carLoadDocumentInfoString = _generalSettings.GetCarLoadDocumentInfoString;
			CanSaveCarLoadDocumentInfoString = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.StorePermissions.Documents.CanEditCarLoadDocumentInfoString);
			SaveCarLoadDocumentInfoStringCommand = new DelegateCommand(SaveCarLoadDocumentInfoString, () => CanSaveCarLoadDocumentInfoString);

			_upcomingTechInspectForOurCars = _generalSettings.UpcomingTechInspectForOurCars;
			_upcomingTechInspectForRaskatCars = _generalSettings.UpcomingTechInspectForRaskatCars;
			CanEditUpcomingTechInspectSetting = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanEditTechInspectSetting);
			SaveUpcomingTechInspectCommand = new DelegateCommand(SaveUpcomingTechInspect, () => CanEditUpcomingTechInspectSetting);

			_osagoEndingNotifyDaysBefore = _carInsuranceSettings.OsagoEndingNotifyDaysBefore;
			_kaskoEndingNotifyDaysBefore = _carInsuranceSettings.KaskoEndingNotifyDaysBefore;
			CanEditInsuranceNotificationsSettings =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanEditInsuranceNotificationsSettings);
			SaveInsuranceNotificationsSettingsCommand = new DelegateCommand(SaveInsuranceNotificationsSettings, () => CanEditInsuranceNotificationsSettings);

			_carTechnicalCheckupEndingNotifyDaysBefore = _generalSettings.CarTechnicalCheckupEndingNotificationDaysBefore;
			CanEditCarTechnicalCheckupNotificationsSettings =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanEditCarTechnicalCheckupNotificationsSettings);
			SaveCarTechnicalCheckupSettingsCommand = new DelegateCommand(SaveCarTechnicalCheckupSettings, () => CanEditCarTechnicalCheckupNotificationsSettings);

			SetFastDeliveryIntervalFrom(_generalSettings.FastDeliveryIntervalFrom);
			CanEditFastDeliveryIntervalFromSetting = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.CanEditFastDeliveryIntervalFromSetting);
			SaveFastDeliveryIntervalFromCommand = new DelegateCommand(SaveFastDeliveryIntervalFrom, () => CanEditFastDeliveryIntervalFromSetting);

			_fastDeliveryMaximumPermissibleLateMinutes = _generalSettings.FastDeliveryMaximumPermissibleLateMinutes;
			SaveFastDeliveryMaximumPermissibleLateCommand = new DelegateCommand(SaveFastDeliveryMaximumPermissibleLate, () => CanEditFastDeliveryIntervalFromSetting);

			_largusMaxDailyFuelLimit = _fuelControlSettings.LargusMaxDailyFuelLimit;
			_truckMaxDailyFuelLimit = _fuelControlSettings.TruckMaxDailyFuelLimit;
			_gazelleMaxDailyFuelLimit = _fuelControlSettings.GAZelleMaxDailyFuelLimit;
			_loaderMaxDailyFuelLimit = _fuelControlSettings.LoaderMaxDailyFuelLimit;
			_minivanMaxDailyFuelLimit = fuelControlSettings.MinivanMaxDailyFuelLimit;
			
			CanEditOrderOrganizationsSettings = 
				_commonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Core.Domain.Permissions.SettingsPermissions.CanEditOrderOrganizationsSettings);
			CanEditDailyFuelLimitsSetting =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Fuel.CanEditMaxDailyFuelLimit);
			SaveDailyFuelLimitsCommand = new DelegateCommand(SaveDailyFuelLimits, () => CanEditDailyFuelLimitsSetting);

			_defaultPaymentDeferment = generalSettings.DefaultPaymentDeferment;
			
			InitializeAccountingSettingsViewModels();
			ConfigureOrderOrganizationsSettings();
		}

		#region RouteListPrintedFormPhones

		public bool CanEditRouteListPrintedFormPhones { get; }

		public SubdivisionSettingsViewModel AlternativePricesSubdivisionSettingsViewModel { get; private set; }

		public SubdivisionSettingsViewModel ComplaintsSubdivisionSettingsViewModel { get; private set; }

		public NamedDomainEntitiesSettingsViewModelBase WarehousesForPricesAndStocksIntegrationViewModel { get; private set; }
		public EmployeeFixedPricesViewModel EmployeeFixedPricesViewModel { get; private set; }

		public string RouteListPrintedFormPhones
		{
			get => _routeListPrintedFormPhones;
			set => SetField(ref _routeListPrintedFormPhones, value);
		}

		public DelegateCommand SaveRouteListPrintedFormPhonesCommand => _saveRouteListPrintedFormPhonesCommand
			?? (_saveRouteListPrintedFormPhonesCommand = new DelegateCommand(SaveRouteListPrintedFormPhones)
			);

		private void SaveRouteListPrintedFormPhones()
		{
			if(!ValidateRouteListPrintedFormPhones())
			{
				return;
			}

			_generalSettings.UpdateRouteListPrintedFormPhones(RouteListPrintedFormPhones);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		private bool ValidateRouteListPrintedFormPhones()
		{
			if(string.IsNullOrWhiteSpace(RouteListPrintedFormPhones))
			{
				ShowWarningMessage("Строка с телефонами для печатной формы МЛ не может быть пуста!");
				return false;
			}

			if(RouteListPrintedFormPhones != null && RouteListPrintedFormPhones.Length > _routeListPrintedFormPhonesLimitSymbols)
			{
				ShowWarningMessage(
					$"Строка с телефонами для печатной формы МЛ не может превышать {_routeListPrintedFormPhonesLimitSymbols} символов!");
				return false;
			}

			return true;
		}

		#endregion

		#region CanAddForwardersToLargus

		public bool CanEditCanAddForwardersToLargus { get; }

		public bool CanEditCanAddForwardersToMinivan { get; }

		public bool CanAddForwardersToLargus
		{
			get => _canAddForwardersToLargus;
			set => SetField(ref _canAddForwardersToLargus, value);
		}

		public bool CanAddForwardersToMinivan
		{
			get => _canAddForwardersToMinivan;
			set => SetField(ref _canAddForwardersToMinivan, value);
		}

		public DelegateCommand SaveCanAddForwardersToLargusCommand => _saveCanAddForwardersToLargusCommand
			?? (_saveCanAddForwardersToLargusCommand = new DelegateCommand(() =>
				{
					_generalSettings.UpdateCanAddForwardersToLargus(CanAddForwardersToLargus);
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
				})
			);

		public DelegateCommand SaveCanAddForwardersToMinivanCommand => _saveCanAddForwardersToMinivanCommand
			?? (_saveCanAddForwardersToMinivanCommand = new DelegateCommand(() =>
			{
				_generalSettings.UpdateCanAddForwardersToMinivan(CanAddForwardersToMinivan);
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
			})
			);

		public RoboatsSettingsViewModel RoboatsSettingsViewModel { get; }
		public RecomendationSettingsViewModel RecomendationSettingsViewModel { get; }

		#endregion

		#region OrderAutoComment

		public string OrderAutoComment
		{
			get => _orderAutoComment;
			set => SetField(ref _orderAutoComment, value);
		}

		public bool CanEditOrderAutoComment { get; }

		public DelegateCommand SaveOrderAutoCommentCommand =>
			_saveOrderAutoCommentCommand ?? (_saveOrderAutoCommentCommand = new DelegateCommand(() =>
			{
				_generalSettings.UpdateOrderAutoComment(OrderAutoComment);
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
			}));

		public DelegateCommand ShowAutoCommentInfoCommand =>
			_showAutoCommentInfoCommand ?? (_showAutoCommentInfoCommand = new DelegateCommand(() =>
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Info,
					"Если в заказе стоит бесконтактная доставка и доставляется промонабор для новых клиентов (в наборе не стоит галочка \"для многократного использования\"),\n" +
					"то в начало комментария к заказу добавляется текст из настройки."
					);
			}));

		#endregion

		#region Drivers Stop-list settings

		public int DriversUnclosedRouteListsHavingDebtCount
		{
			get => _driversUnclosedRouteListsHavingDebtCount;
			set => SetField(ref _driversUnclosedRouteListsHavingDebtCount, value);
		}

		public decimal DriversRouteListsDebtMaxSum
		{
			get => _driversRouteListsDebtMaxSum;
			set => SetField(ref _driversRouteListsDebtMaxSum, value);
		}

		public DelegateCommand SaveDriversStopListPropertiesCommand
		{
			get
			{
				if(_saveDriversStopListPropertiesCommand == null)
				{
					_saveDriversStopListPropertiesCommand = new DelegateCommand(SaveDriversStopListProperties, () => CanSaveDriversStopListProperties);
					_saveDriversStopListPropertiesCommand.CanExecuteChangedWith(this, x => x.CanSaveDriversStopListProperties);
				}
				return _saveDriversStopListPropertiesCommand;
			}
		}

		public bool CanSaveDriversStopListProperties => _canEditDriversStopListSettings;

		private void SaveDriversStopListProperties()
		{
			_generalSettings.UpdateDriversUnclosedRouteListsHavingDebtMaxCount(DriversUnclosedRouteListsHavingDebtCount);
			_generalSettings.UpdateDriversRouteListsMaxDebtSum(DriversRouteListsDebtMaxSum);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}
		#endregion

		#region ClientsSecondOrderDiscount

		public bool IsClientsSecondOrderDiscountActive
		{
			get => _isClientsSecondOrderDiscountActive;
			set => SetField(ref _isClientsSecondOrderDiscountActive, value);
		}

		public DelegateCommand SaveSecondOrderDiscountAvailabilityCommand
		{
			get
			{
				if(_saveSecondOrderDiscountAvailabilityCommand == null)
				{
					_saveSecondOrderDiscountAvailabilityCommand = new DelegateCommand(SaveSecondOrderDiscountAvailability, () => CanSaveSecondOrderDiscountAvailability);
					_saveSecondOrderDiscountAvailabilityCommand.CanExecuteChangedWith(this, x => x.CanSaveSecondOrderDiscountAvailability);
				}
				return _saveSecondOrderDiscountAvailabilityCommand;
			}
		}

		public bool CanSaveSecondOrderDiscountAvailability => _canActivateClientsSecondOrderDiscount;

		private void SaveSecondOrderDiscountAvailability()
		{
			_generalSettings.UpdateIsClientsSecondOrderDiscountActive(IsClientsSecondOrderDiscountActive);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		#endregion

		#region OrderWaitUntil

		public bool IsOrderWaitUntilActive
		{
			get => _isOrderWaitUntilActive;
			set => SetField(ref _isOrderWaitUntilActive, value);
		}

		public DelegateCommand SaveOrderWaitUntilActiveCommand { get; }
		public bool CanEditOrderWaitUntilSetting { get; }

		private void SaveIsEditOrderWaitUntilActive()
		{
			_generalSettings.UpdateIsOrderWaitUntilActive(IsOrderWaitUntilActive);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		#endregion

		#region FastDelivery19LBottlesLimit

		public bool IsFastDelivery19LBottlesLimitActive
		{
			get => _isFastDelivery19LBottlesLimitActive;
			set => SetField(ref _isFastDelivery19LBottlesLimitActive, value);
		}

		public DelegateCommand SaveFastDelivery19LBottlesLimitActiveCommand { get; }
		public bool CanEditFastDelivery19LBottlesLimitSetting { get; }

		private void SaveIsFastDelivery19LBottlesLimitActive()
		{
			_generalSettings.UpdateIsFastDelivery19LBottlesLimitActive(IsFastDelivery19LBottlesLimitActive);
			_generalSettings.UpdateFastDelivery19LBottlesLimitCount(FastDelivery19LBottlesLimitCount);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		public int FastDelivery19LBottlesLimitCount
		{
			get => _fastDelivery19LBottlesLimitCount;
			set => SetField(ref _fastDelivery19LBottlesLimitCount, value);
		}

		#endregion

		#region Настройка уведомления о приближающемся ТО

		public int UpcomingTechInspectForOurCars
		{
			get => _upcomingTechInspectForOurCars;
			set => SetField(ref _upcomingTechInspectForOurCars, value);
		}

		public int UpcomingTechInspectForRaskatCars
		{
			get => _upcomingTechInspectForRaskatCars;
			set => SetField(ref _upcomingTechInspectForRaskatCars, value);
		}

		public DelegateCommand SaveUpcomingTechInspectCommand { get; }
		public bool CanEditUpcomingTechInspectSetting { get; }

		private void SaveUpcomingTechInspect()
		{
			_generalSettings.UpdateUpcomingTechInspectForOurCars(UpcomingTechInspectForOurCars);
			_generalSettings.UpdateUpcomingTechInspectForRaskatCars(UpcomingTechInspectForRaskatCars);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		#endregion

		#region Настройка уведомлений о приближающихся страховках

		public int OsagoEndingNotifyDaysBefore
		{
			get => _osagoEndingNotifyDaysBefore;
			set => SetField(ref _osagoEndingNotifyDaysBefore, value);
		}

		public int KaskoEndingNotifyDaysBefore
		{
			get => _kaskoEndingNotifyDaysBefore;
			set => SetField(ref _kaskoEndingNotifyDaysBefore, value);
		}

		public DelegateCommand SaveInsuranceNotificationsSettingsCommand { get; }
		public bool CanEditInsuranceNotificationsSettings { get; }

		private void SaveInsuranceNotificationsSettings()
		{
			_carInsuranceSettings.SetOsagoEndingNotifyDaysBefore(OsagoEndingNotifyDaysBefore);
			_carInsuranceSettings.SetKaskoEndingNotifyDaysBefore(KaskoEndingNotifyDaysBefore);

			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");

		}

		#endregion

		#region Настройка уведомлений о приближающемся ГТО

		public int CarTechnicalCheckupEndingNotifyDaysBefore
		{
			get => _carTechnicalCheckupEndingNotifyDaysBefore;
			set => SetField(ref _carTechnicalCheckupEndingNotifyDaysBefore, value);
		}

		public DelegateCommand SaveCarTechnicalCheckupSettingsCommand { get; }
		public bool CanEditCarTechnicalCheckupNotificationsSettings { get; }

		private void SaveCarTechnicalCheckupSettings()
		{
			_generalSettings.UpdateCarTechnicalCheckupEndingNotificationDaysBefore(CarTechnicalCheckupEndingNotifyDaysBefore);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");

		}

		#endregion

		#region BillAdditionalInfo

		public string BillAdditionalInfo
		{
			get => _billAdditionalInfo;
			set => SetField(ref _billAdditionalInfo, value);
		}

		public DelegateCommand SaveBillAdditionalInfoCommand { get; }

		public bool CanSaveBillAdditionalInfo { get; }

		private void SaveBillAdditionalInfo()
		{
			if(!string.IsNullOrEmpty(BillAdditionalInfo) && BillAdditionalInfo.Length > _billAdditionalInfoMaxLength)
			{
				ShowMaxStringLengthExceededErrorMessage(BillAdditionalInfo.Length, _billAdditionalInfoMaxLength);
				return;
			}

			_generalSettings.UpdateBillAdditionalInfo(BillAdditionalInfo);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		#endregion

		#region Фикса для сотрудников

		private void InitializeEmployeesFixedPricesViewModel()
		{
			EmployeeFixedPricesViewModel =
				_lifetimeScope.Resolve<EmployeeFixedPricesViewModel>(new TypedParameter(typeof(DialogViewModelBase), this));
		}

		#endregion

		#region CarLoadDocumentInfoString

		public string CarLoadDocumentInfoString
		{
			get => _carLoadDocumentInfoString;
			set => SetField(ref _carLoadDocumentInfoString, value);
		}

		public DelegateCommand SaveCarLoadDocumentInfoStringCommand { get; }

		public bool CanSaveCarLoadDocumentInfoString { get; }

		private void SaveCarLoadDocumentInfoString()
		{
			if(!string.IsNullOrEmpty(CarLoadDocumentInfoString) && CarLoadDocumentInfoString.Length > _carLoadDocumentInfoStringMaxLength)
			{
				ShowMaxStringLengthExceededErrorMessage(CarLoadDocumentInfoString.Length, _carLoadDocumentInfoStringMaxLength);
				return;
			}

			_generalSettings.UpdateCarLoadDocumentInfoString(CarLoadDocumentInfoString);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		
		#endregion

		#region FastDeliveryLates

		public bool IsIntervalFromOrderCreated
		{
			get => _isIntervalFromOrderCreated;
			set => SetField(ref _isIntervalFromOrderCreated, value);
		}

		public bool IsIntervalFromAddedInFirstRouteList
		{
			get => _isIntervalFromAddedInFirstRouteList;
			set => SetField(ref _isIntervalFromAddedInFirstRouteList, value);
		}

		public bool IsIntervalFromRouteListItemTransfered
		{
			get => _isIntervalFromRouteListItemTransfered;
			set => SetField(ref _isIntervalFromRouteListItemTransfered, value);
		}

		public int FastDeliveryMaximumPermissibleLateMinutes
		{
			get => _fastDeliveryMaximumPermissibleLateMinutes;
			set => SetField(ref _fastDeliveryMaximumPermissibleLateMinutes, value);
		}

		private FastDeliveryIntervalFromEnum FastDeliveryIntervalFrom =>
			IsIntervalFromOrderCreated
				? FastDeliveryIntervalFromEnum.OrderCreated
				: IsIntervalFromAddedInFirstRouteList ? FastDeliveryIntervalFromEnum.AddedInFirstRouteList : FastDeliveryIntervalFromEnum.RouteListItemTransfered;

		private void SetFastDeliveryIntervalFrom(FastDeliveryIntervalFromEnum fastDeliveryIntervalFrom)
		{
			switch(fastDeliveryIntervalFrom)
			{
				case FastDeliveryIntervalFromEnum.OrderCreated:
					IsIntervalFromOrderCreated = true;
					break;
				case FastDeliveryIntervalFromEnum.AddedInFirstRouteList:
					IsIntervalFromAddedInFirstRouteList = true;
					break;
				case FastDeliveryIntervalFromEnum.RouteListItemTransfered:
					IsIntervalFromRouteListItemTransfered = true;
					break;
			}
		}

		public DelegateCommand SaveFastDeliveryIntervalFromCommand { get; }

		public bool CanEditFastDeliveryIntervalFromSetting { get; }

		private void SaveFastDeliveryIntervalFrom()
		{
			_generalSettings.UpdateFastDeliveryIntervalFrom(FastDeliveryIntervalFrom);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		public DelegateCommand SaveFastDeliveryMaximumPermissibleLateCommand { get; }

		private void SaveFastDeliveryMaximumPermissibleLate()
		{
			_generalSettings.UpdateFastDeliveryMaximumPermissibleLateMinutes(FastDeliveryMaximumPermissibleLateMinutes);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		#endregion

		#region Настройка максимальных суточных лимитов для авто

		public int LargusMaxDailyFuelLimit
		{
			get => _largusMaxDailyFuelLimit;
			set => SetField(ref _largusMaxDailyFuelLimit, value);
		}

		public int TruckMaxDailyFuelLimit
		{
			get => _truckMaxDailyFuelLimit;
			set => SetField(ref _truckMaxDailyFuelLimit, value);
		}

		public int GazelleMaxDailyFuelLimit
		{
			get => _gazelleMaxDailyFuelLimit;
			set => SetField(ref _gazelleMaxDailyFuelLimit, value);
		}

		public int LoaderMaxDailyFuelLimit
		{
			get => _loaderMaxDailyFuelLimit;
			set => SetField(ref _loaderMaxDailyFuelLimit, value);
		}

		public int MinivanMaxDailyFuelLimit
		{
			get => _minivanMaxDailyFuelLimit;
			set => SetField(ref _minivanMaxDailyFuelLimit, value);
		}

		public DelegateCommand SaveDailyFuelLimitsCommand { get; }
		public bool CanEditDailyFuelLimitsSetting { get; }
		public PaymentWriteOffAllowedFinancialExpenseCategorySettingsViewModel PaymentWriteOffAllowedFinancialExpenseCategoriesViewModel { get; private set; }

		private void SaveDailyFuelLimits()
		{
			_fuelControlSettings.SetLargusMaxDailyFuelLimit(LargusMaxDailyFuelLimit);
			_fuelControlSettings.SetTruckMaxDailyFuelLimit(TruckMaxDailyFuelLimit);
			_fuelControlSettings.SetGAZelleMaxDailyFuelLimit(GazelleMaxDailyFuelLimit);
			_fuelControlSettings.SetLoaderMaxDailyFuelLimit(LoaderMaxDailyFuelLimit);
			_fuelControlSettings.SetMinivanMaxDailyFuelLimit(MinivanMaxDailyFuelLimit);
			
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}
		#endregion

		#region Бухгалтерия

		private void InitializeAccountingSettingsViewModels()
		{
			PaymentWriteOffAllowedFinancialExpenseCategoriesViewModel
				= new PaymentWriteOffAllowedFinancialExpenseCategorySettingsViewModel(
					_commonServices,
					_unitOfWorkFactory,
					NavigationManager,
					_generalSettings,
					_commonServices.CurrentPermissionService,
					this)
				{
					MainTitle = "<b>Настройки списаний с баланса клиента</b>",
					DetailTitle = "Статьи расхода доступные для выбора в карточке списаний с баланса клиента:"
				};
		}

		#endregion Бухгалтерия

		#region Настройка юр.лиц в заказе
		
		public IOrganizationForOrderFromSet OrganizationForOrderFromSet { get; }

		public bool CanEditOrderOrganizationsSettings { get; private set; }

		public ICommand SaveOrderOrganizationSettingsCommand { get; private set; }
		
		private int _orderSettingsCurrentPage;
		public int OrderSettingsCurrentPage
		{
			get => _orderSettingsCurrentPage;
			set => SetField(ref _orderSettingsCurrentPage, value);
		}
		
		private bool _orderGeneralSettingsTabActive;
		public bool OrderGeneralSettingsTabActive
		{
			get => _orderGeneralSettingsTabActive;
			set
			{
				if(SetField(ref _orderGeneralSettingsTabActive, value) && value)
				{
					OrderSettingsCurrentPage = 0;
				}
			}
		}

		private bool _orderOrganizationSettingsTabActive;
		public bool OrderOrganizationSettingsTabActive
		{
			get => _orderOrganizationSettingsTabActive;
			set
			{
				if(SetField(ref _orderOrganizationSettingsTabActive, value) && value)
				{
					OrderSettingsCurrentPage = 1;
				}
			}
		}
		
		private short _selectedOrganizationBasedOrderContentSet;

		public short SelectedOrganizationBasedOrderContentSet
		{
			get => _selectedOrganizationBasedOrderContentSet;
			set
			{
				if(SetField(ref _selectedOrganizationBasedOrderContentSet, value))
				{
					_organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings = OrganizationsByOrderContent[value];
				}
			}
		}

		public IUnitOfWork UowOrderOrganizationSettings { get; private set; }
		private OrganizationByOrderAuthorSettings _organizationByOrderAuthorSettings;
		private int _targetPaymentDeferment;
		private int _newPaymentDeferment;
		private int _defaultPaymentDeferment;
		public IEnumerable<Subdivision> AuthorsSubdivisions { get; private set; }
		public IEnumerable<short> AuthorsSets { get; private set; }
		public IReadOnlyDictionary<short, OrganizationBasedOrderContentSettings> OrganizationsByOrderContent { get; private set; }
		public ILookup<PaymentType, PaymentTypeOrganizationSettings> PaymentTypesOrganizationSettings { get; private set; }

		private void ConfigureOrderOrganizationsSettings()
		{
			UowOrderOrganizationSettings = _unitOfWorkFactory.CreateWithoutRoot("Настройки юр лиц для заказа");
			InitializeOrderOrganizationsSettingsCommands();
			ConfigureDataForSetWidgets();
			ConfigurePaymentTypeSettings();
		}

		private void InitializeOrderOrganizationsSettingsCommands()
		{
			SaveOrderOrganizationSettingsCommand = new DelegateCommand(SaveOrderOrganizationsSettings);
			CalculatePaymentDefermentCommand = new DelegateCommand(CalculatePaymentDefermentCommandHandler);
			SaveDefaultPaymentDefermentCommand = new DelegateCommand(SaveDefaultPaymentDefermentCommandHandler);
		}

		private void SaveOrderOrganizationsSettings()
		{
			if(!ValidateOrderOrganizationSettings())
			{
				return;
			}

			SaveOrganizationBasedOrderContentSettings();
			SavePaymentTypesOrganizationSettings();
			SaveOrganizationByOrderAuthorSettings();
			
			try
			{
				UowOrderOrganizationSettings.Commit();
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Настройки по выбору организации для заказа сохранены!");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении настроек юр лиц для заказа");
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"При сохранении настроек юр лиц для заказа произошла ошибка. Переоткройте вкладку и попробуйте снова");
				Close(false, CloseSource.Self);
			}
		}

		private void SaveOrganizationBasedOrderContentSettings()
		{
			foreach(var keyPairValue in OrganizationsByOrderContent)
			{
				var organizationByOrderContent = keyPairValue.Value;
				UowOrderOrganizationSettings.Save(organizationByOrderContent);
			}
		}
		
		private void SavePaymentTypesOrganizationSettings()
		{
			foreach(var groupSettings in PaymentTypesOrganizationSettings)
			{
				foreach(var paymentTypeOrganizationSettings in groupSettings)
				{
					UowOrderOrganizationSettings.Save(paymentTypeOrganizationSettings);
				}
			}
		}
		
		private void SaveOrganizationByOrderAuthorSettings()
		{
			UowOrderOrganizationSettings.Save(_organizationByOrderAuthorSettings);
		}

		private bool ValidateOrderOrganizationSettings()
		{
			var validationRequests = new List<ValidationRequest>();
			AddValidationRequestsForOrganizationsByOrderContentSettings(validationRequests);
			AddValidationRequestsForPaymentTypesOrganizationSettings(validationRequests);

			return _validator.Validate(validationRequests);
		}

		private void AddValidationRequestsForOrganizationsByOrderContentSettings(ICollection<ValidationRequest> validationRequests)
		{
			foreach(var keyPairValue in OrganizationsByOrderContent)
			{
				var otherSetSettings = OrganizationsByOrderContent
					.Where(x => x.Key != keyPairValue.Key)
					.Select(x => x.Value)
					.ToList();

				var contextItems = new Dictionary<object, object>
				{
					{ "OtherSetsSettings", otherSetSettings }
				};

				var validationContext = new ValidationContext(keyPairValue.Value, contextItems);
				validationContext.ServiceContainer.AddService(typeof(IOrganizationSettings), _organizationSettings);

				validationRequests.Add(new ValidationRequest(keyPairValue.Value, validationContext));
			}
		}

		private void AddValidationRequestsForPaymentTypesOrganizationSettings(ICollection<ValidationRequest> validationRequests)
		{
			foreach(var paymentTypeOrganizationSettings in PaymentTypesOrganizationSettings)
			{
				foreach(var paymentTypeSettings in paymentTypeOrganizationSettings)
				{
					validationRequests.Add(new ValidationRequest(paymentTypeSettings));
				}
			}
		}

		private void ConfigureDataForSetWidgets()
		{
			var orderContentSettings =
				UowOrderOrganizationSettings
					.GetAll<OrganizationBasedOrderContentSettings>()
					.ToDictionary(x => x.OrderContentSet);

			OrganizationsByOrderContent = orderContentSettings;
			
			InitializeDataForSet1(orderContentSettings);
			InitializeDataForSet2(orderContentSettings);
			InitializeDataForSet3();
		}

		private void InitializeDataForSet1(IDictionary<short, OrganizationBasedOrderContentSettings> organizationsByOrderContent)
		{
			const short set = 1;
			organizationsByOrderContent.TryGetValue(set, out var organizationSettings);
			
			if(organizationSettings is null)
			{
				organizationSettings = new OrganizationBasedOrderContentSettings
				{
					OrderContentSet = set
				};
				
				organizationsByOrderContent.Add(set, organizationSettings);
			}
		}

		private void InitializeDataForSet2(IDictionary<short, OrganizationBasedOrderContentSettings> organizationsByOrderContent)
		{
			const short set = 2;
			organizationsByOrderContent.TryGetValue(set, out var organizationSettings);
			
			if(organizationSettings is null)
			{
				organizationSettings = new OrganizationBasedOrderContentSettings
				{
					OrderContentSet = set
				};
				
				organizationsByOrderContent.Add(set, organizationSettings);
			}
		}
		
		private void InitializeDataForSet3()
		{
			AuthorsSets = OrganizationsByOrderContent.Keys.ToList();
			
			_organizationByOrderAuthorSettings =
				UowOrderOrganizationSettings.GetAll<OrganizationByOrderAuthorSettings>().SingleOrDefault();

			if(_organizationByOrderAuthorSettings is null)
			{
				_organizationByOrderAuthorSettings = new OrganizationByOrderAuthorSettings();
				SelectedOrganizationBasedOrderContentSet = 1;
			}
			else
			{
				_selectedOrganizationBasedOrderContentSet =
					_organizationByOrderAuthorSettings.OrganizationBasedOrderContentSettings.OrderContentSet;
			}
			
			AuthorsSubdivisions = _organizationByOrderAuthorSettings.OrderAuthorsSubdivisions;
		}
		
		private void ConfigurePaymentTypeSettings()
		{
			PaymentTypesOrganizationSettings = UowOrderOrganizationSettings.GetAll<PaymentTypeOrganizationSettings>()
				.ToLookup(x => x.PaymentType);
		}

		#endregion

		#region Массовое изменение отсрочки платежа задолженности

		public int TargetPaymentDeferment
		{
			get => _targetPaymentDeferment;
			set => SetField(ref _targetPaymentDeferment, value);
		}

		public int NewPaymentDeferment
		{
			get => _newPaymentDeferment;
			set => SetField(ref _newPaymentDeferment, value);
		}

		public int DefaultPaymentDeferment
		{
			get => _defaultPaymentDeferment;
			set => SetField(ref _defaultPaymentDeferment, value);
		}

		public DelegateCommand CalculatePaymentDefermentCommand { get; set; }

		private void CalculatePaymentDefermentCommandHandler()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				uow.Session
					.CreateSQLQuery(@"UPDATE counterparty SET delay_days_for_buyers = :newValue WHERE delay_days_for_buyers = :oldValue")
					.SetParameter("oldValue", TargetPaymentDeferment)
					.SetParameter("newValue", NewPaymentDeferment)
					.ExecuteUpdate();
				
				uow.Commit();
			}
			
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Пересчет произведен!");
		}
		
		public DelegateCommand SaveDefaultPaymentDefermentCommand { get; set; }

		private void SaveDefaultPaymentDefermentCommandHandler()
		{
			_generalSettings.SaveDefaultPaymentDeferment(DefaultPaymentDeferment);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}
		

		#endregion
		public EntityJournalOpener EntityJournalOpener { get; }

		private void InitializeSettingsViewModels()
		{
			ComplaintsSubdivisionSettingsViewModel = new SubdivisionSettingsViewModel(_commonServices, _unitOfWorkFactory, NavigationManager,
				_generalSettings, _generalSettings.SubdivisionsToInformComplaintHasNoDriverParameterName)
			{
				CanEdit = CanEditRouteListPrintedFormPhones,
				MainTitle = "<b>Настройки рекламаций</b>",
				DetailTitle = "Информировать о незаполненном водителе в рекламациях на следующие отделы:",
				Info = "Сотрудники данных отделов будут проинформированы о незаполненном водителе при закрытии рекламации. " +
					   "Если отдел есть в списке ответственных и итог работы по сотрудникам: Вина доказана."
			};

			var canEditAlternativePrices = _commonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_alternative_nomenclature_prices");

			AlternativePricesSubdivisionSettingsViewModel = new SubdivisionSettingsViewModel(_commonServices, _unitOfWorkFactory, NavigationManager,
				_generalSettings, _generalSettings.SubdivisionsAlternativePricesName)
			{
				CanEdit = canEditAlternativePrices,
				MainTitle = "<b>Настройки альтернативных цен</b>",
				DetailTitle = "Использовать альтернативную цену для авторов заказов из следующих отделов:",
				Info = "Сотрудники данных отделов могут редактировать альтернативные цены"
			};

			WarehousesForPricesAndStocksIntegrationViewModel =
				new WarehousesSettingsViewModel(_commonServices, _unitOfWorkFactory, NavigationManager,
				_generalSettings, _generalSettings.WarehousesForPricesAndStocksIntegrationName)
				{
					CanEdit = _commonServices.CurrentPermissionService.ValidatePresetPermission(
						Vodovoz.Core.Domain.Permissions.NomenclaturePermissions.HasAccessToSitesAndAppsTab),
					MainTitle = "<b>Настройки складов для интеграции остатков и цен</b>",
					DetailTitle = "Использовать следующие склады при подсчете остатков для ИПЗ:",
					Info = "Подсчет остатков при отправке в ИПЗ будет производиться только по выбранным складам."
				};

			FillItemSources();
		}

		private void FillItemSources()
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				unitOfWork.Session.DefaultReadOnly = true;

				var subdivisionIdToRetrieve = _generalSettings.SubdivisionsToInformComplaintHasNoDriver;

				var retrievedSubdivisions = unitOfWork.Session.Query<Subdivision>()
					.Where(subdivision => subdivisionIdToRetrieve.Contains(subdivision.Id))
					.ToList();

				foreach(var subdivision in retrievedSubdivisions)
				{
					ComplaintsSubdivisionSettingsViewModel.ObservableSubdivisions.Add(subdivision);
				}

				var subdivisionIdsForAlternativePrices = _generalSettings.SubdivisionsForAlternativePrices;

				var subdivisionForAlternativePrices = unitOfWork.Session.Query<Subdivision>()
					.Where(s => subdivisionIdsForAlternativePrices.Contains(s.Id))
					.ToList();

				foreach(var subdivision in subdivisionForAlternativePrices)
				{
					AlternativePricesSubdivisionSettingsViewModel.ObservableSubdivisions.Add(subdivision);
				}
			}
		}

		private void ShowMaxStringLengthExceededErrorMessage(int currentLength, int maxLength)
		{
			_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Сохранение недоступно!" +
					$"\nМаксимально допустимая длина строки составляет {maxLength} символов." +
					$"\nВы ввели {currentLength} символов.");
		}

		public override void Dispose()
		{
			EmployeeFixedPricesViewModel.Dispose();
			UowOrderOrganizationSettings.Dispose();
			base.Dispose();
		}
	}
}
