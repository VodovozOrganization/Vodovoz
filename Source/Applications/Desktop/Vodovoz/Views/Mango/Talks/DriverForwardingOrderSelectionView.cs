using Gtk;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;
using VodovozBusiness.EntityRepositories.Nodes;

namespace Vodovoz.Views.Mango.Talks
{
	[System.ComponentModel.ToolboxItem(true)]
	[WindowSize(900, 300)]
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
				.AddColumn("Заказ").HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.OrderId)
				.AddColumn("Дата доставки").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DeliveryDateText)
				.AddColumn("Адрес").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Address)
					.WrapWidth(300).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Статус заказа").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.OrderStatusTitle)
				.AddColumn("Водитель").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DriverName)
				.AddColumn("Доб. номер").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.DriverExtensionNumberText)
				.AddColumn("Комментарий").HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.ForwardingUnavailableReason)
					.WrapWidth(200).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("")
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
