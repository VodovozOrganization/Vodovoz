using System;
using System.Linq;
using Gamma.GtkWidgets;
using QS.Dialog.GtkUI;
using QS.Services;
using SmsPaymentService;
using Vodovoz.Additions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SmsSendPanelView : Gtk.Bin, IPanelView
	{
		private Phone _selectedPhone;
		private Counterparty _counterparty;
		private Order _order;
		private readonly IPermissionResult _orderPermissionResult;
		private readonly bool _canSendSmsForAdditionalOrderStatuses;

		public SmsSendPanelView(ICurrentPermissionService currentPermissionService)
		{
			if(currentPermissionService == null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}
			Build();
			_orderPermissionResult = currentPermissionService.ValidateEntityPermission(typeof(Order));
			_canSendSmsForAdditionalOrderStatuses = currentPermissionService.ValidatePresetPermission("can_send_sms_for_additional_order_statuses");
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

			ySendSmsButton.Pressed += (btn, args) =>
			{
				if(string.IsNullOrWhiteSpace(validatedPhoneEntry.Text))
				{
					MessageDialogHelper.RunErrorDialog("Вы забыли выбрать номер.", "Ошибка при отправке Sms");
					return;
				}

				ySendSmsButton.Sensitive = false;
				GLib.Timeout.Add(10000, () =>
				{
					ySendSmsButton.Sensitive = true;
					return false;
				});

				var smsSender = new SmsPaymentSender();
				var result = smsSender.SendSmsPaymentToNumber(_order.Id, validatedPhoneEntry.Text);
				switch(result.Status)
				{
					case PaymentResult.MessageStatus.Ok:
						MessageDialogHelper.RunInfoDialog("Sms отправлена успешно");
						break;
					case PaymentResult.MessageStatus.Error:
						MessageDialogHelper.RunErrorDialog(result.ErrorDescription, "Ошибка при отправке Sms");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};
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

			ySendSmsButton.Sensitive = _orderPermissionResult.CanRead;
			yPhonesListTreeView.ItemsDataSource = _counterparty.Phones;
		}

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public bool VisibleOnPanel
		{
			get
			{
				var isStatusAllowedByDefaultForSendingSms =
					new OrderStatus[]
					{
						OrderStatus.Accepted,
						OrderStatus.OnTheWay,
						OrderStatus.Shipped,
						OrderStatus.InTravelList,
						OrderStatus.OnLoading
					}.Contains(_order.OrderStatus);

				var isAdditionalOrderStatus =
					new OrderStatus[]
					{
						OrderStatus.Closed,
						OrderStatus.UnloadingOnStock,
					}.Contains(_order.OrderStatus);

				return isStatusAllowedByDefaultForSendingSms || (isAdditionalOrderStatus && _canSendSmsForAdditionalOrderStatuses);
			}
		}
	}
}
