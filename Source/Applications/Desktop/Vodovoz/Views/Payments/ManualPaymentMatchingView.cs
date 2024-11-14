﻿using System;
using System.ComponentModel;
using Gamma.ColumnConfig;
using Gamma.Widgets.Additions;
using Gtk;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	[ToolboxItem(true)]
	public partial class ManualPaymentMatchingView : Gtk.Bin
	{
		public ManualPaymentMatchingView()
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			notebookPayment.ShowTabs = false;

			#region Radio buttons

			radioBtnAllocateOrders.Active = true;
			radioBtnAllocateOrders.Toggled += RadioBtnAllocateOrdersOnToggled;
			radioBtnAllocatedOrders.Toggled += RadioBtnAllocatedOrdersOnToggled;
			radioBtnAllocatedOrders.Sensitive = ViewModel.HasPaymentItems;

			#endregion

			btnAddCounterparty.Clicked += (sender, args) => ViewModel.AddCounterpatyCommand.Execute();
			btnAddCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyIsNull, w => w.Sensitive)
				.InitializeFromSource();
			ybtnRevertPayment.Clicked += (sender, args) => ViewModel.RevertAllocatedSum.Execute();
			ybtnRevertPayment.Binding.AddBinding(ViewModel, vm => vm.CanRevertPay, w => w.Sensitive).InitializeFromSource();

			dateRangeFilter.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			dateRangeFilter.PeriodChangedByUser += (sender, e) => ViewModel.UpdateNodes();

			enumchecklistOrdersStatuses.EnumType = typeof(OrderStatus);
			enumchecklistOrdersStatuses.Binding
				.AddBinding(ViewModel, vm => vm.OrderStatuses, w => w.SelectedValuesList, new EnumsListConverter<OrderStatus>())
				.InitializeFromSource();

			enumchecklistOrderPaymentStatuses.EnumType = typeof(OrderPaymentStatus);
			enumchecklistOrderPaymentStatuses.Binding
				.AddBinding(ViewModel, vm => vm.OrderPaymentStatuses, w => w.SelectedValuesList, new EnumsListConverter<OrderPaymentStatus>())
				.InitializeFromSource();

			lblBalance.Binding
				.AddBinding(ViewModel, vm => vm.CurrentBalance, v => v.Text, new DecimalToStringConverter())
				.InitializeFromSource();

			lblAllocated.Binding
				.AddBinding(ViewModel, vm => vm.AllocatedSum, v => v.Text, new DecimalToStringConverter())
				.InitializeFromSource();

			ylabelWaitForPaymentValue.Binding
				.AddFuncBinding(ViewModel, vm => vm.CounterpartyWaitingForPaymentOrdersDebt > 0 ? vm.CounterpartyWaitingForPaymentOrdersDebt.ToString("N2") : "0.00", w => w.Text)
				.InitializeFromSource();

			ylabelCloseDocumentsValue.Binding
				.AddFuncBinding(ViewModel, vm => vm.CounterpartyClosingDocumentsOrdersDebt > 0 ? vm.CounterpartyClosingDocumentsOrdersDebt.ToString("N2") : "0.00", w => w.Text)
				.InitializeFromSource();

			ylabelOtherOrdersDebtValue.Binding
				.AddFuncBinding(ViewModel, vm => vm.CounterpartyOtherOrdersDebt > 0 ? vm.CounterpartyOtherOrdersDebt.ToString("N2") : "0.00", w => w.Text)
				.InitializeFromSource();

			textViewComment.Binding
				.AddBinding(ViewModel.Entity, vm => vm.Comment, v => v.Buffer.Text)
				.InitializeFromSource();

			//entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);

			/*entryCounterparty.Binding
				.AddBinding(ViewModel.Entity, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();

			entryCounterparty.ChangedByUser += (sender, e) =>
			{
				ViewModel.UpdateCMOCounterparty();
				ViewModel.UpdateNodes();
				ViewModel.GetLastBalance();
				ViewModel.UpdateSumToAllocate();
				ViewModel.UpdateCurrentBalance();
				ViewModel.GetCounterpartyDebt();
			};*/

			var searchView = new SearchView((SearchViewModel)ViewModel.Search);
			hboxSearch.Add(searchView);
			searchView.Show();

			hbox6.Sensitive = ViewModel.CanChangeCounterparty;

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ConfigureTrees();
		}

		private void ConfigureTrees()
		{
			ytreeviewOrdersAllocate.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingViewModelNode>.Create()
				.AddColumn("№ заказа")
					.AddTextRenderer(node => node.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа")
					.AddTextRenderer(node => node.OrderDate.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Сумма заказа, р.")
					.AddTextRenderer(node => $"{node.ActualOrderSum}")
					.XAlign(0.5f)
				.AddColumn("Прошлые оплаты, р.")
					.AddNumericRenderer(node => node.LastPayments)
					.Digits(2)
				.AddColumn("Текущая оплата, р.")
					.AddNumericRenderer(node => node.CurrentPayment).Editing().Digits(2)
					.Adjustment(new Adjustment(0, 0, 10000000, 1, 10, 10))
					.AddSetter((node, cell) => ViewModel.CurrentPaymentChangedByUser(cell))
				.AddColumn("Статус оплаты")
					.AddEnumRenderer(node => node.OrderPaymentStatus)
				.AddColumn("Рассчитать остаток?")
					.AddToggleRenderer(node => node.Calculate)
					.ToggledEvent(UseFine_Toggled)
				.AddColumn("")
				.Finish();

			ytreeviewOrdersAllocate.ItemsDataSource = ViewModel.ListNodes;
			ytreeviewOrdersAllocate.ButtonReleaseEvent += YtreeviewOrdersAllocate_ButtonReleaseEvent;

			yTreeViewAllocatedOrders.ColumnsConfig = FluentColumnsConfig<ManualPaymentMatchingViewModelAllocatedNode>.Create()
				.AddColumn("№ заказа")
					.AddNumericRenderer(node => node.OrderId)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.AddEnumRenderer(node => node.OrderStatus)
				.AddColumn("Дата заказа")
					.AddTextRenderer(node => node.OrderDate.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("Сумма заказа, р.")
					.AddNumericRenderer(node => node.OrderSum)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Полная сумма оплаты\n(в т.ч. с др платежей), р.")
					.AddNumericRenderer(node => node.AllAllocatedSum)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Распределенная сумма\n(с этого платежа), р.")
					.AddNumericRenderer(node => node.AllocatedSum)
					.Digits(2)
					.XAlign(0.5f)
				.AddColumn("Статус оплаты")
					.AddEnumRenderer(node => node.OrderPaymentStatus)
					.XAlign(0.5f)
				.AddColumn("Статус распределения")
					.AddEnumRenderer(node => node.PaymentItemStatus)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			yTreeViewAllocatedOrders.ItemsDataSource = ViewModel.ListAllocatedNodes;
			yTreeViewAllocatedOrders.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanRevertPayFromOrderPermission, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedAllocatedNode, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanChangeCounterparty))
			{
				hbox6.Sensitive = ViewModel.CanChangeCounterparty;
				return;
			}
		}

		#region Переключение вкладок

		private void RadioBtnAllocateOrdersOnToggled(object sender, EventArgs e)
		{
			if(radioBtnAllocateOrders.Active)
			{
				notebookPayment.CurrentPage = 0;
			}
		}

		private void RadioBtnAllocatedOrdersOnToggled(object sender, EventArgs e)
		{
			if(radioBtnAllocatedOrders.Active)
			{
				notebookPayment.CurrentPage = 1;
			}
		}

		#endregion

		private void UseFine_Toggled(object o, ToggledArgs args) =>
			//Вызываем через Gtk.Application.Invoke чтобы событие вызывалось уже после того как поле обновилось.
			Gtk.Application.Invoke((sender, eventArgs) => OnToggleClicked(this, EventArgs.Empty));

		private void OnToggleClicked(object sender, EventArgs e)
		{
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
			{
				return;
			}

			var node = selectedObj as ManualPaymentMatchingViewModelNode;

			if(node.Calculate)
			{
				ViewModel.Calculate(node);
			}
			else
			{
				ViewModel.ReCalculate(node);
			}
		}

		private void YtreeviewOrdersAllocate_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button == 3)
			{
				ConfigureMenu();
			}
		}

		private void ConfigureMenu()
		{
			var selectedObj = ytreeviewOrdersAllocate.GetSelectedObject();

			if(selectedObj == null)
			{
				return;
			}

			var order = ViewModel.UoW.GetById<Order>((selectedObj as ManualPaymentMatchingViewModelNode).Id);

			var menu = new Menu();

			var openOrder = new MenuItem($"Открыть заказ №{order.Id}");
			openOrder.Activated += (s, args) => ViewModel.OpenOrderCommand.Execute(order);
			openOrder.Visible = true;
			menu.Add(openOrder);

			menu.ShowAll();
			menu.Popup();
		}
	}
}
