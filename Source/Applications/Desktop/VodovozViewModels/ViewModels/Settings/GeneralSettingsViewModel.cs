using QS.Commands;
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
	}
}
