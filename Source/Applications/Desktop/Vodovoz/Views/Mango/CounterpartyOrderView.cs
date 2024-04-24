using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Views;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Dialogs.Mango;

namespace Vodovoz.Views.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyOrderView : ViewBase<CounterpartyOrderViewModel>
	{
		public CounterpartyOrderView(CounterpartyOrderViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}
		void Configure()
		{
			OrderYTreeView.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
			.AddColumn("Номер").HeaderAlignment(0.5f)
			.AddNumericRenderer(order => order.Id).XAlign(0.5f)
			.AddColumn("Дата").HeaderAlignment(0.5f)
			.AddTextRenderer(order => order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("dd.MM.yy") : String.Empty).XAlign(0.5f)
			.AddColumn("Статус").HeaderAlignment(0.5f)
			.AddTextRenderer(order => order.OrderStatus.GetEnumTitle()).XAlign(0.5f)
			.AddColumn("Адрес").HeaderAlignment(0.5f)
			.AddTextRenderer(order => order.DeliveryPoint != null ? order.DeliveryPoint.CompiledAddress : null)
			.Finish();

			OrderYTreeView.ButtonReleaseEvent += ButtonReleaseEvent_OrderYTreeView;
			OrderYTreeView.RowActivated += RowActivated_OrderYTreeView;
			OrderYTreeView.CursorChanged += CursorChanged_OrderYTreeView;
			CounterpartyYButton.Clicked += PressEvent_CounterpartyYButton;
			CounterpartyYButton.Label = ViewModel.Client.Name;
			CommTextView.Buffer.Text = ViewModel.Client.Comment;
			CommTextView.Editable = false;

			OrderYTreeView.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.LatestOrder, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedOrder, w => w.SelectedRow)
				.InitializeFromSource();

			specialListCmbDeliveryPoint.Binding.AddSource(ViewModel)
				.AddBinding( vm => vm.DeliveryPoints, w => w.ItemsList)
				.AddBinding( vm => vm.DeliveryPoint, w => w.SelectedItem)
				.AddBinding( vm => vm.IsDeliveryPointChoiceRequired, w => w.Sensitive)
				.InitializeFromSource();
		}
		#region Events
		private void ButtonReleaseEvent_OrderYTreeView(object sedner , ButtonReleaseEventArgs e)
		{
			var selectedOrder = OrderYTreeView.GetSelectedObject<Order>();

			if(e.Event.Button == 3 && selectedOrder != null) {
				Gtk.Menu popupMenu = new Menu();
				MenuItem item1 = new MenuItem("Повторить заказ");
				item1.ButtonReleaseEvent += delegate (object s, ButtonReleaseEventArgs _e) { ViewModel.RepeatOrder(selectedOrder.Id); };
				popupMenu.Add(item1);

				MenuItem item2 = new MenuItem("Перейти в заказ");
				item2.ButtonReleaseEvent += delegate (object s, ButtonReleaseEventArgs _e) { ViewModel.OpenMoreInformationAboutOrder(selectedOrder.Id); };
				popupMenu.Add(item2);

				MenuItem item3 = new MenuItem("Перейти в МЛ");
				if(
					selectedOrder.OrderStatus == OrderStatus.NewOrder ||
					selectedOrder.OrderStatus == OrderStatus.Accepted ||
					selectedOrder.OrderStatus == OrderStatus.OnLoading ||
					selectedOrder.OrderStatus == OrderStatus.OnTheWay ||
					selectedOrder.OrderStatus == OrderStatus.InTravelList ||
					selectedOrder.OrderStatus == OrderStatus.Closed ||
					selectedOrder.OrderStatus == OrderStatus.Shipped )
				{
					item3.ButtonReleaseEvent += delegate (object s, ButtonReleaseEventArgs _e) { ViewModel.OpenRoutedList(selectedOrder); };
					popupMenu.Add(item3);
				}

				if(selectedOrder.OrderStatus == OrderStatus.Canceled || selectedOrder.OrderStatus == OrderStatus.NotDelivered || selectedOrder.OrderStatus == OrderStatus.DeliveryCanceled) {
					MenuItem item4 = new MenuItem("Перейти в недовоз");
					item4.ButtonReleaseEvent += delegate (object s, ButtonReleaseEventArgs _e) { ViewModel.OpenUndelivery(selectedOrder); };
					popupMenu.Add(item4);
				}
				if(selectedOrder.OrderStatus == OrderStatus.NewOrder ||
					selectedOrder.OrderStatus == OrderStatus.WaitForPayment ||
					selectedOrder.OrderStatus == OrderStatus.Accepted ||
					selectedOrder.OrderStatus == OrderStatus.InTravelList) {

					MenuItem item5 = new MenuItem("Отменить");
					item5.ButtonReleaseEvent += delegate (object s, ButtonReleaseEventArgs _e) { ViewModel.CancelOrder(selectedOrder); };
					popupMenu.Add(item5);
				}

				MenuItem item6 = new MenuItem("Создать жалобу");
				item6.ButtonReleaseEvent += delegate (object s, ButtonReleaseEventArgs _e) { ViewModel.CreateComplaint(selectedOrder); };
				popupMenu.Add(item6);
				
				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}
		private void PressEvent_CounterpartyYButton(object sender , EventArgs e)
		{
			ViewModel.OpenMoreInformationAboutCounterparty();
		}

		private void CursorChanged_OrderYTreeView(object sender, EventArgs e)
		{
			var selectedRow = OrderYTreeView.GetSelectedObject<Order>();
			ViewModel.Order = selectedRow;
		}
		private void RowActivated_OrderYTreeView(object sender, EventArgs e)
		{
			var selectedRow = OrderYTreeView.GetSelectedObject<Order>();
			ViewModel.OpenMoreInformationAboutOrder(selectedRow.Id);
		}
		#endregion
	}
}
