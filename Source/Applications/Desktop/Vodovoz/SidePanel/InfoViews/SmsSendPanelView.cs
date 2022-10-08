using System;
using System.Linq;
using System.Text;
using Gamma.GtkWidgets;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Numeric;
using Sms.Internal;
using Sms.Internal.Client;
using SmsPaymentService;
using Vodovoz.Additions;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Sms;
using Vodovoz.Settings.Sms;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SmsSendPanelView : Gtk.Bin, IPanelView
	{
		private readonly ISmsPaymentRepository _smsPaymentRepository;
		private readonly IFastPaymentRepository _fastPaymentRepository;
		private readonly IFastPaymentParametersProvider _fastPaymentParametersProvider;
		private readonly IPermissionResult _orderPermissionResult;
		private readonly IInteractiveService _interactiveService;
		private readonly ISmsSettings _smsSettings;
		private readonly SmsClientChannelFactory _smsClientChannelFactory;
		private readonly PhoneFormatter _phoneFormatter;
		private static readonly SmsPaymentStatus[] _excludeSmsPaymentStatuses =
			{ SmsPaymentStatus.ReadyToSend, SmsPaymentStatus.Cancelled };

		private readonly bool _canSendSmsForAdditionalOrderStatuses;
		private readonly bool _canSendSmsForPayFromYookassa;
		private readonly bool _canSendSmsForPayFromSbpByCard;
		private Phone _selectedPhone;
		private Counterparty _counterparty;
		private Order _order;
		private bool _isPaidOrder;
		

		public SmsSendPanelView(
			ICommonServices commonServices,
			ISmsPaymentRepository smsPaymentRepository,
			IFastPaymentRepository fastPaymentRepository,
			IFastPaymentParametersProvider fastPaymentParametersProvider)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			_smsPaymentRepository = smsPaymentRepository ?? throw new ArgumentNullException(nameof(smsPaymentRepository));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
			_fastPaymentParametersProvider =
				fastPaymentParametersProvider ?? throw new ArgumentNullException(nameof(fastPaymentParametersProvider));
			var currentPermissionService = commonServices.CurrentPermissionService;
			_interactiveService = commonServices.InteractiveService;
			_phoneFormatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);

			Build();
			_orderPermissionResult = currentPermissionService.ValidateEntityPermission(typeof(Order));
			_canSendSmsForAdditionalOrderStatuses =
				currentPermissionService.ValidatePresetPermission("can_send_sms_for_additional_order_statuses");
			_canSendSmsForPayFromYookassa = currentPermissionService.ValidatePresetPermission("can_send_sms_for_pay_from_yookassa");
			_canSendSmsForPayFromSbpByCard = currentPermissionService.ValidatePresetPermission("can_send_sms_for_pay_from_sbp_by_card");
			var settingsController = new SettingsController(UnitOfWorkFactory.GetDefaultFactory);
			_smsSettings = new SmsSettings(settingsController, MainClass.DataBaseInfo);
			_smsClientChannelFactory = new SmsClientChannelFactory(_smsSettings);

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

			ySendSmsButton.Sensitive = _canSendSmsForPayFromYookassa;
			btnSendFastPaymentPayByCardUrlBySms.Sensitive = _canSendSmsForPayFromSbpByCard;
				
			ySendSmsButton.Pressed += OnSendSmsButtonPressed;
			btnSendFastPaymentPayByQrUrlBySms.Clicked += OnSendFastPaymentUrlBySmsClicked;
			btnSendFastPaymentPayByCardUrlBySms.Clicked += OnSendFastPaymentUrlBySmsClicked;
		}

		private void OnSendSmsButtonPressed(object btn, EventArgs args)
		{
			if(_order.Id == 0)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Перед отправкой SMS необходимо сохранить заказ",
					"Не удалось отправить SMS");
				return;
			}
			if(string.IsNullOrWhiteSpace(validatedPhoneEntry.Text))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Вы забыли выбрать номер.", "Не удалось отправить SMS");
				return;
			}

			var alreadySentSms = _smsPaymentRepository.GetSmsPaymentsForOrder(
				InfoProvider.UoW,
				_order.Id,
				_excludeSmsPaymentStatuses
			);

			var paidSmsPayments = alreadySentSms.Where(x => x.SmsPaymentStatus == SmsPaymentStatus.Paid).ToList();
			var waitingSmsPayments = alreadySentSms.Where(x =>
				x.SmsPaymentStatus == SmsPaymentStatus.WaitingForPayment
				&& DateTime.Now.Subtract(x.CreationDate).TotalMinutes < 60
			).ToList();

			if(paidSmsPayments.Any())
			{
				var paidStringBuilder = new StringBuilder();

				foreach(var payment in paidSmsPayments)
				{
					paidStringBuilder.AppendLine($"\tКод платежа:     {payment.Id}");
					paidStringBuilder.AppendLine($"\tТелефон:            +7 {_phoneFormatter.FormatString(payment.PhoneNumber)}");
					paidStringBuilder.AppendLine($"\tДата создания:  {payment.CreationDate}");
					paidStringBuilder.AppendLine($"\tДата оплаты:     {payment.PaidDate}");
					paidStringBuilder.AppendLine($"\tНомер оплаты:  {payment.ExternalId}");
					paidStringBuilder.AppendLine();
				}

				var sendPayment = _interactiveService.Question("Для заказа уже есть ранее оплаченные платежи по SMS:\n\n" +
					$"{paidStringBuilder}" +
					"Вы уверены что хотите отправить ещё одну SMS?");

				if(!sendPayment)
				{
					return;
				}
			}
			else if(waitingSmsPayments.Any())
			{
				var waitingStringBuilder = new StringBuilder();

				foreach(var payment in waitingSmsPayments)
				{
					waitingStringBuilder.AppendLine($"\tКод платежа:     {payment.Id}");
					waitingStringBuilder.AppendLine($"\tТелефон:            +7 {_phoneFormatter.FormatString(payment.PhoneNumber)}");
					waitingStringBuilder.AppendLine($"\tДата создания:  {payment.CreationDate}");
					waitingStringBuilder.AppendLine();
				}

				var sendPayment = _interactiveService.Question("Для заказа найдены SMS, ожидающие оплату клиента:\n\n" +
					$"{waitingStringBuilder}" +
					"Вы уверены что хотите отправить ещё одну SMS?");

				if(!sendPayment)
				{
					return;
				}
			}

			btnSendFastPaymentPayByQrUrlBySms.Sensitive = btnSendFastPaymentPayByCardUrlBySms.Sensitive = ySendSmsButton.Sensitive = false;
			GLib.Timeout.Add(10000, () =>
			{
				btnSendFastPaymentPayByQrUrlBySms.Sensitive = true;
				btnSendFastPaymentPayByCardUrlBySms.Sensitive = _canSendSmsForPayFromSbpByCard;
				ySendSmsButton.Sensitive = _canSendSmsForPayFromYookassa;

				return false;
			});

			var smsSender = new SmsPaymentSender();
			PaymentResult result;
			try
			{
				result = smsSender.SendSmsPaymentToNumber(_order.Id, validatedPhoneEntry.Text);
			}
			catch(TimeoutException)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning,
					"Превышено время ожидания ответа от сервиса оплаты по SMS.\nЕсли SMS не была отправлена, произведите повторную отправку");
				return;
			}
			switch(result.Status)
			{
				case PaymentResult.MessageStatus.Ok:
					_interactiveService.ShowMessage(ImportanceLevel.Info, "SMS отправлена успешно");
					break;
				case PaymentResult.MessageStatus.Error:
					_interactiveService.ShowMessage(ImportanceLevel.Error, result.ErrorDescription, "Не удалось отправить SMS");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
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

			btnSendFastPaymentPayByQrUrlBySms.Sensitive = btnSendFastPaymentPayByCardUrlBySms.Sensitive = ySendSmsButton.Sensitive = false;
			GLib.Timeout.Add(10000, () =>
			{
				if(!_isPaidOrder)
				{
					btnSendFastPaymentPayByQrUrlBySms.Sensitive = true;
					btnSendFastPaymentPayByCardUrlBySms.Sensitive = _canSendSmsForPayFromSbpByCard;
					ySendSmsButton.Sensitive = _canSendSmsForPayFromYookassa;
				}

				return false;
			});

			var isQr = (btn as yButton)?.Name == nameof(btnSendFastPaymentPayByQrUrlBySms);
			var fastPaymentSender = new FastPaymentSender(_fastPaymentParametersProvider, _smsClientChannelFactory, _smsSettings);
			var resultTask = fastPaymentSender.SendFastPaymentUrlAsync(_order, validatedPhoneEntry.Text, isQr);
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
						ySendSmsButton.Sensitive = false;
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

			ySendSmsButton.Sensitive = _orderPermissionResult.CanRead && _canSendSmsForPayFromYookassa;
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
