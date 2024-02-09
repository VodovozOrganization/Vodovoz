﻿using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class GeneralSettingsViewModel : TabViewModelBase
	{
		private const int _carLoadDocumentInfoStringMaxLength = 120;
		private const int _billAdditionalInfoMaxLength = 200;

		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private const int _routeListPrintedFormPhonesLimitSymbols = 500;

		private string _routeListPrintedFormPhones;
		private bool _canAddForwardersToLargus;
		private DelegateCommand _saveRouteListPrintedFormPhonesCommand;
		private DelegateCommand _saveCanAddForwardersToLargusCommand;
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


		public GeneralSettingsViewModel(
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			ICommonServices commonServices,
			RoboatsSettingsViewModel roboatsSettingsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation = null) : base(commonServices?.InteractiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			RoboatsSettingsViewModel = roboatsSettingsViewModel ?? throw new ArgumentNullException(nameof(roboatsSettingsViewModel));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_generalSettingsParametersProvider =
				generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));

			TabName = "Общие настройки";

			RouteListPrintedFormPhones = _generalSettingsParametersProvider.GetRouteListPrintedFormPhones;
			CanAddForwardersToLargus = _generalSettingsParametersProvider.GetCanAddForwardersToLargus;
			CanEditRouteListPrintedFormPhones =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_route_List_printed_form_phones");
			CanEditCanAddForwardersToLargus =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_can_add_forwarders_to_largus");
			CanEditOrderAutoComment =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_order_auto_comment_setting");
			OrderAutoComment = _generalSettingsParametersProvider.OrderAutoComment;

			InitializeSettingsViewModels();

			_canEditDriversStopListSettings = _commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_drivers_stop_list_parameters");
			_driversUnclosedRouteListsHavingDebtCount = _generalSettingsParametersProvider.DriversUnclosedRouteListsHavingDebtMaxCount;
			_driversRouteListsDebtMaxSum = _generalSettingsParametersProvider.DriversRouteListsMaxDebtSum;

			_canActivateClientsSecondOrderDiscount =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Order.CanActivateClientsSecondOrderDiscount);
			_isClientsSecondOrderDiscountActive = _generalSettingsParametersProvider.GetIsClientsSecondOrderDiscountActive;

			_isOrderWaitUntilActive = _generalSettingsParametersProvider.GetIsOrderWaitUntilActive;
			CanEditOrderWaitUntilSetting = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Order.CanEditOrderWaitUntil);
			SaveOrderWaitUntilActiveCommand = new DelegateCommand(SaveIsEditOrderWaitUntilActive, () => CanEditOrderWaitUntilSetting);

			_billAdditionalInfo = _generalSettingsParametersProvider.GetBillAdditionalInfo;
			CanSaveBillAdditionalInfo = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Order.Documents.CanEditBillAdditionalInfo);
			SaveBillAdditionalInfoCommand = new DelegateCommand(SaveBillAdditionalInfo, () => CanSaveBillAdditionalInfo);

			_carLoadDocumentInfoString = _generalSettingsParametersProvider.GetCarLoadDocumentInfoString;
			CanSaveCarLoadDocumentInfoString = _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Store.Documents.CanEditCarLoadDocumentInfoString);
			SaveCarLoadDocumentInfoStringCommand = new DelegateCommand(SaveCarLoadDocumentInfoString, () => CanSaveCarLoadDocumentInfoString);
		}

		#region RouteListPrintedFormPhones

		public bool CanEditRouteListPrintedFormPhones { get; }

		public SubdivisionSettingsViewModel AlternativePricesSubdivisionSettingsViewModel { get; private set; }

		public SubdivisionSettingsViewModel ComplaintsSubdivisionSettingsViewModel { get; private set; }

		public NamedDomainEntitiesSettingsViewModelBase WarehousesForPricesAndStocksIntegrationViewModel { get; private set; }

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

			_generalSettingsParametersProvider.UpdateRouteListPrintedFormPhones(RouteListPrintedFormPhones);
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

		public bool CanAddForwardersToLargus
		{
			get => _canAddForwardersToLargus;
			set => SetField(ref _canAddForwardersToLargus, value);
		}

		public DelegateCommand SaveCanAddForwardersToLargusCommand => _saveCanAddForwardersToLargusCommand
			?? (_saveCanAddForwardersToLargusCommand = new DelegateCommand(() =>
				{
					_generalSettingsParametersProvider.UpdateCanAddForwardersToLargus(CanAddForwardersToLargus);
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
				})
			);

		public RoboatsSettingsViewModel RoboatsSettingsViewModel { get; }

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
				_generalSettingsParametersProvider.UpdateOrderAutoComment(OrderAutoComment);
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
			_generalSettingsParametersProvider.UpdateDriversUnclosedRouteListsHavingDebtMaxCount(DriversUnclosedRouteListsHavingDebtCount);
			_generalSettingsParametersProvider.UpdateDriversRouteListsMaxDebtSum(DriversRouteListsDebtMaxSum);
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
			_generalSettingsParametersProvider.UpdateIsClientsSecondOrderDiscountActive(IsClientsSecondOrderDiscountActive);
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
			_generalSettingsParametersProvider.UpdateIsOrderWaitUntilActive(IsOrderWaitUntilActive);
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

			_generalSettingsParametersProvider.UpdateBillAdditionalInfo(BillAdditionalInfo);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
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

			_generalSettingsParametersProvider.UpdateCarLoadDocumentInfoString(CarLoadDocumentInfoString);
			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Сохранено!");
		}

		#endregion

		private void InitializeSettingsViewModels()
		{
			ComplaintsSubdivisionSettingsViewModel = new SubdivisionSettingsViewModel(_commonServices, _unitOfWorkFactory, NavigationManager,
				_generalSettingsParametersProvider, _generalSettingsParametersProvider.SubdivisionsToInformComplaintHasNoDriverParameterName)
			{
				CanEdit = CanEditRouteListPrintedFormPhones,
				MainTitle = "<b>Настройки рекламаций</b>",
				DetailTitle = "Информировать о незаполненном водителе в рекламациях на следующие отделы:",
				Info = "Сотрудники данных отделов будут проинформированы о незаполненном водителе при закрытии рекламации. " +
					   "Если отдел есть в списке ответственных и итог работы по сотрудникам: Вина доказана."
			};

			var canEditAlternativePrices = _commonServices.CurrentPermissionService.ValidatePresetPermission("сan_edit_alternative_nomenclature_prices");

			AlternativePricesSubdivisionSettingsViewModel = new SubdivisionSettingsViewModel(_commonServices, _unitOfWorkFactory, NavigationManager,
				_generalSettingsParametersProvider, _generalSettingsParametersProvider.SubdivisionsAlternativePricesName)
			{
				CanEdit = canEditAlternativePrices,
				MainTitle = "<b>Настройки альтернативных цен</b>",
				DetailTitle = "Использовать альтернативную цену для авторов заказов из следующих отделов:",
				Info = "Сотрудники данных отделов могут редактировать альтернативные цены"
			};

			WarehousesForPricesAndStocksIntegrationViewModel =
				new WarehousesSettingsViewModel(_commonServices, _unitOfWorkFactory, NavigationManager,
				_generalSettingsParametersProvider, _generalSettingsParametersProvider.WarehousesForPricesAndStocksIntegrationName)
				{
					CanEdit = true,
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

				var subdivisionIdToRetrieve = _generalSettingsParametersProvider.SubdivisionsToInformComplaintHasNoDriver;

				var retrievedSubdivisions = unitOfWork.Session.Query<Subdivision>()
					.Where(subdivision => subdivisionIdToRetrieve.Contains(subdivision.Id))
					.ToList();

				foreach(var subdivision in retrievedSubdivisions)
				{
					ComplaintsSubdivisionSettingsViewModel.ObservableSubdivisions.Add(subdivision);
				}

				var subdivisionIdsForAlternativePrices = _generalSettingsParametersProvider.SubdivisionsForAlternativePrices;

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
	}
}
