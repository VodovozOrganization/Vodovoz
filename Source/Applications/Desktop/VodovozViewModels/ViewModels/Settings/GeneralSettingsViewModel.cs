using System;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class GeneralSettingsViewModel : TabViewModelBase
	{
		private readonly IGeneralSettingsParametersProvider _generalSettingsParametersProvider;
		private readonly ICommonServices _commonServices;
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
			INavigationManager navigation = null) : base(commonServices?.InteractiveService, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			RoboatsSettingsViewModel = roboatsSettingsViewModel ?? throw new ArgumentNullException(nameof(roboatsSettingsViewModel));
			_generalSettingsParametersProvider =
				generalSettingsParametersProvider ?? throw new ArgumentNullException(nameof(generalSettingsParametersProvider));

			TabName = "Общие настройки";

			RouteListPrintedFormPhones = _generalSettingsParametersProvider.GetRouteListPrintedFormPhones;
			CanAddForwardersToLargus = _generalSettingsParametersProvider.GetCanAddForwardersToLargus;
			CanEditRouteListPrintedFormPhones =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_route_List_printed_form_phones");
			CanEditCanAddForwardersToLargus =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_can_add_forwarders_to_largus");

			OrderAutoComment = _generalSettingsParametersProvider.OrderAutoComment;
		}

		#region RouteListPrintedFormPhones

		public bool CanEditRouteListPrintedFormPhones { get; }

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

		public DelegateCommand SaveOrderAutoCommentCommand =>
			_saveOrderAutoCommentCommand ?? (_saveOrderAutoCommentCommand = new DelegateCommand(() =>
			{
				_generalSettingsParametersProvider.UpdateOrderAutoComment(OrderAutoComment);
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
	}
}
