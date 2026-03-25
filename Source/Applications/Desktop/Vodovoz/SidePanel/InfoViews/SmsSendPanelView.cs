using Autofac;
using Gamma.GtkWidgets;
using QS.Dialog;
using QS.Project.DB;
using QS.Services;
using Sms.Internal;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Models;
using Vodovoz.Settings;
using Vodovoz.Settings.Database.Sms;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Settings.Sms;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SmsSendPanelView : Gtk.Bin, IPanelView
	{
		private readonly ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IPermissionResult _orderPermissionResult;
		private readonly IInteractiveService _interactiveService;
		private readonly ISmsSettings _smsSettings;
		private readonly IFastPaymentSender _fastPaymentSender;
		private readonly bool _canSendSmsForAdditionalOrderStatuses;
		private readonly bool _canSendSmsForPayFromSbpByCard;
		private Phone _selectedPhone;
		private Counterparty _counterparty;
		private Order _order;
		private bool _isPaidOrder;
		
		public SmsSendPanelView(
			ICommonServices commonServices,
			IFastPaymentRepository fastPaymentRepository,
			IFastPaymentSettings fastPaymentSettings)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			var currentPermissionService = commonServices.CurrentPermissionService;
			_interactiveService = commonServices.InteractiveService;

			Build();
			_orderPermissionResult = currentPermissionService.ValidateEntityPermission(typeof(Order));
			_canSendSmsForAdditionalOrderStatuses =
				currentPermissionService.ValidatePresetPermission("can_send_sms_for_additional_order_statuses");
			_canSendSmsForPayFromSbpByCard = currentPermissionService.ValidatePresetPermission("can_send_sms_for_pay_from_sbp_by_card");
			var settingsController = _lifetimeScope.Resolve<ISettingsController>();
			var databaseInfo = _lifetimeScope.Resolve<IDataBaseInfo>();
			_smsSettings = new SmsSettings(settingsController, databaseInfo);
			_fastPaymentSender = _lifetimeScope.Resolve<IFastPaymentSender>();

			Configure();
		}

		private void Configure()
		{
			validatedPhoneEntry.WidthRequest = 135;
			validatedPhoneEntry.ValidationMode = QSWidgetLib.ValidationType.phone;
			yPhonesListTreeView.ColumnsConfig = ColumnsConfigFactory.Create<Phone>()
				.AddColumn("Телефоны")
				.AddTextRenderer(x => x.Number)
				.Finish();

			if(_orderPermissionResult.CanRead)
			{
				yPhonesListTreeView.Selection.Changed += (sender, args) =>
				{
					_selectedPhone = yPhonesListTreeView.GetSelectedObject() as Phone;
					validatedPhoneEntry.Text = _selectedPhone?.Number ?? "";
				};
			}
			validatedPhoneEntry.Sensitive = _orderPermissionResult.CanRead;

			btnSendFastPaymentPayByCardUrlBySms.Sensitive = _canSendSmsForPayFromSbpByCard;
				
			btnSendFastPaymentPayByQrUrlBySms.Clicked += OnSendFastPaymentUrlBySmsClicked;
			btnSendFastPaymentPayByCardUrlBySms.Clicked += OnSendFastPaymentUrlBySmsClicked;
		}
		
		private void OnSendFastPaymentUrlBySmsClicked(object btn, EventArgs args)
		{
			if(_order.Id == 0)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Перед отправкой QR необходимо сохранить заказ",
					"Не удалось отправить QR по SMS");
				return;
			}
			if(string.IsNullOrWhiteSpace(validatedPhoneEntry.Text))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Вы забыли выбрать номер.", "Не удалось отправить QR по SMS");
				return;
			}

			if(!_smsSettings.SmsSendingAllowed)
			{
				return;
			}

			var lastProccessingPayment = _fastPaymentRepository.GetProcessingPaymentForOrder(InfoProvider.UoW, _order.Id);

			if(lastProccessingPayment != null)
			{
				if(!_interactiveService.Question(
						"Будет отменена текущая действующая сессия оплаты и сформирована новая ссылка на оплату. Продолжить?",
						"Вы уверены что хотите отправить новую ссылку на оплату по SMS?"))
				{
					return;
				}
			}

			btnSendFastPaymentPayByQrUrlBySms.Sensitive = btnSendFastPaymentPayByCardUrlBySms.Sensitive = false;
			GLib.Timeout.Add(10000, () =>
			{
				if(!_isPaidOrder)
				{
					btnSendFastPaymentPayByQrUrlBySms.Sensitive = true;
					btnSendFastPaymentPayByCardUrlBySms.Sensitive = _canSendSmsForPayFromSbpByCard;
				}

				return false;
			});

			var isQr = (btn as yButton)?.Name == nameof(btnSendFastPaymentPayByQrUrlBySms);

			var resultTask = _fastPaymentSender.SendFastPaymentUrlAsync(_order.Id, validatedPhoneEntry.Text, isQr);
			resultTask.Wait();
			var result = resultTask.Result;

			switch(result.Status)
			{
				case ResultStatus.Ok:
					_interactiveService.ShowMessage(ImportanceLevel.Info, "SMS отправлена успешно");
					break;
				case ResultStatus.Error:
					if(result.OrderAlreadyPaied)
					{
						_isPaidOrder = true;
					}
					_interactiveService.ShowMessage(ImportanceLevel.Error, result.ErrorMessage, "Не удалось отправить SMS");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public IInfoProvider InfoProvider { get; set; }

		public void Refresh()
		{
			if(InfoProvider is ISmsSendProvider smsSendProvider)
			{
				_counterparty = smsSendProvider.Counterparty;
				_order = smsSendProvider.Order;
			}

			if(_counterparty == null || _order == null)
			{
				return;
			}

			yPhonesListTreeView.ItemsDataSource = _counterparty.Phones;
		}

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public bool VisibleOnPanel
		{
			get
			{
				var isStatusAllowedByDefaultForSendingSms =
					new[]
					{
						OrderStatus.Accepted,
						OrderStatus.OnTheWay,
						OrderStatus.Shipped,
						OrderStatus.InTravelList,
						OrderStatus.OnLoading
					}.Contains(_order.OrderStatus);

				var isAdditionalOrderStatus =
					new[]
					{
						OrderStatus.Closed,
						OrderStatus.UnloadingOnStock,
					}.Contains(_order.OrderStatus);

				return isStatusAllowedByDefaultForSendingSms || (isAdditionalOrderStatus && _canSendSmsForAdditionalOrderStatuses);
			}
		}
	}
}
