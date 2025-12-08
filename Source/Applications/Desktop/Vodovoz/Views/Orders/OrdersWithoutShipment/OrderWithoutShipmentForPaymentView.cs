using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[ToolboxItem(true)]
	public partial class OrderWithoutShipmentForPaymentView : TabViewBase<OrderWithoutShipmentForPaymentViewModel>
	{
		public OrderWithoutShipmentForPaymentView(OrderWithoutShipmentForPaymentViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			ybtnOpenBill.Clicked += (sender, e) => ViewModel.OpenBillCommand.Execute();
			
			ylabelOrderNum.Binding
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			ylabelOrderDate.Binding
				.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text)
				.InitializeFromSource();

			ylabelOrderAuthor.Binding
				.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text)
				.InitializeFromSource();

			yCheckBtnHideSignature.Binding
				.AddBinding(ViewModel.Entity, e => e.HideSignature, w => w.Active)
				.InitializeFromSource();

			entityViewModelEntryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);
			entityViewModelEntryCounterparty.Changed += ViewModel.OnCounterpartyEntityViewModelEntryChanged;

			entityViewModelEntryCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.Client, w => w.Subject)
				.InitializeFromSource();

			entityViewModelEntryCounterparty.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive)
				.InitializeFromSource();
			entityViewModelEntryCounterparty.CanEditReference = true;

			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hboxSendDocuments.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpartyJournal += entityViewModelEntryCounterparty.OpenSelectDialog;

			daterangepickerOrdersDate.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull)
				.InitializeFromSource();

			daterangepickerOrdersDate.Binding
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			daterangepickerOrdersDate.PeriodChangedByUser += UpdateAvailableOrders;

			organizationEntry.ViewModel = ViewModel.OrganizationViewModel;
			organizationEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanSetOrganization, w => w.Sensitive)
				.InitializeFromSource();
			
			ytreeviewOrders.ColumnsConfig = FluentColumnsConfig<OrderWithoutShipmentForPaymentNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(node => node.IsSelected).ToggledEvent(UseFine_Toggled)
				.AddColumn("Номер").AddTextRenderer(node => node.OrderId.ToString())
				.AddColumn("Дата\nдоставки").AddTextRenderer(node => node.OrderDate.ToShortDateString())
				.AddColumn("Статус").AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Бутыли").AddTextRenderer(node => $"{node.Bottles:N0}")
				.AddColumn("Сумма").AddTextRenderer(node => node.OrderSum.ToString())
				.AddColumn("Статус\nоплаты").AddEnumRenderer(node => node.OrderPaymentStatus)
				.AddColumn("Адрес").AddTextRenderer(node => node.DeliveryAddress)
				.AddColumn("Название организации").AddTextRenderer(node => node.OrganizationName)
				.Finish();

			ytreeviewOrders.ItemsDataSource = ViewModel.ObservableAvailableOrders;

			treeViewEdoContainers.ColumnsConfig = FluentColumnsConfig<EdoContainer>.Create()
				.AddColumn("Код документооборота")
					.AddTextRenderer(x => x.DocFlowId.HasValue ? x.DocFlowId.ToString() : string.Empty)
				.AddColumn("Отправленные\nдокументы")
					.AddTextRenderer(x => x.SentDocuments)
				.AddColumn("Статус\nдокументооборота")
					.AddEnumRenderer(x => x.EdoDocFlowStatus)
				.AddColumn("Доставлено\nклиенту?")
					.AddToggleRenderer(x => x.Received)
					.Editing(false)
				.AddColumn("Описание ошибки")
					.AddTextRenderer(x => x.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("")
				.Finish();

			if(ViewModel.Entity.Id != 0)
			{
				CustomizeSendDocumentAgainButton();
			}

			treeViewEdoContainers.ItemsDataSource = ViewModel.EdoContainers;

			btnUpdateEdoDocFlowStatus.Clicked += (sender, args) =>
			{
				ViewModel.UpdateEdoContainers();
				CustomizeSendDocumentAgainButton();
			};

			ybuttonSendDocumentAgain.Clicked += ViewModel.OnButtonSendDocumentAgainClicked;

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			UpdateContainersVisibility();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanSendBillByEdo)
				|| e.PropertyName == nameof(ViewModel.CanResendEdoBill))
			{
				UpdateContainersVisibility();
				CustomizeSendDocumentAgainButton();
			}
		}

		private void UpdateContainersVisibility()
		{
			vboxEdo.Visible = ViewModel.CanSendBillByEdo || ViewModel.EdoContainers.Any();
		}

		private void CustomizeSendDocumentAgainButton()
		{
			if(!ViewModel.EdoContainers.Any())
			{
				ybuttonSendDocumentAgain.Sensitive = ViewModel.CanSendBillByEdo;
				ybuttonSendDocumentAgain.Label = "Отправить";
				return;
			}

			ybuttonSendDocumentAgain.Sensitive = ViewModel.CanResendEdoBill;
			ybuttonSendDocumentAgain.Label = "Отправить повторно";
		}

		private void UpdateAvailableOrders(object sender, EventArgs e)
		{
			ViewModel.UpdateAvailableOrders();
		}

		void UseFine_Toggled(object o, ToggledArgs args) =>
			//Вызываем через Gtk.Application.Invoke чтобы событие вызывалось уже после того как поле обновилось.
			Gtk.Application.Invoke(delegate { OnToggleClicked(this, EventArgs.Empty); });

		void OnToggleClicked(object sender, EventArgs e)
		{
			var selectedObj = ytreeviewOrders.GetSelectedObject();

			if(selectedObj == null)
			{
				return;
			}

			ViewModel.SelectedNode = selectedObj as OrderWithoutShipmentForPaymentNode;

			ViewModel.UpdateItems();
		}

		public override void Destroy()
		{
			entityViewModelEntryCounterparty.Changed -= ViewModel.OnCounterpartyEntityViewModelEntryChanged;
			ViewModel.OpenCounterpartyJournal -= entityViewModelEntryCounterparty.OpenSelectDialog;

			base.Destroy();
		}
	}
}
