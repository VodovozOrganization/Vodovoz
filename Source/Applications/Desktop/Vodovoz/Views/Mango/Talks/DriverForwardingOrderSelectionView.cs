using Gtk;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;
using VodovozBusiness.EntityRepositories.Nodes;

namespace Vodovoz.Views.Mango.Talks
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverForwardingOrderSelectionView : DialogViewBase<DriverForwardingOrderSelectionViewModel>
	{
		public DriverForwardingOrderSelectionView(DriverForwardingOrderSelectionViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ytreeviewOrders.CreateFluentColumnsConfig<DriverForwardingOrderNode>()
				.AddColumn("Заказ")
					.AddNumericRenderer(node => node.OrderId)
				.AddColumn("Дата доставки")
					.AddTextRenderer(node => node.DeliveryDateText)
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.Address)
					.WrapWidth(300).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Статус заказа")
					.AddTextRenderer(node => node.OrderStatusTitle)
				.AddColumn("Водитель")
					.AddTextRenderer(node => node.DriverName)
				.AddColumn("Доб. номер")
					.AddTextRenderer(node => node.DriverExtensionNumberText)
				.AddColumn("")
					.AddTextRenderer(node => node.ForwardingUnavailableReason)
				.Finish();

			ytreeviewOrders.SetItemsSource(ViewModel.Orders);

			ytreeviewOrders.Binding
				.AddBinding(ViewModel, vm => vm.SelectedOrder, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeviewOrders.RowActivated += OnOrdersRowActivated;

			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
		}

		private void OnOrdersRowActivated(object sender, RowActivatedArgs args)
		{
			ViewModel.ForwardCallCommand.Execute();
		}
	}
}
