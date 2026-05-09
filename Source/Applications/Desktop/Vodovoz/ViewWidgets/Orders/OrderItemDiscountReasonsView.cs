using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Widgets.Orders;

namespace Vodovoz.ViewWidgets.Orders
{
	[ToolboxItem(true)]
	public partial class OrderItemDiscountReasonsView : WidgetViewBase<OrderItemDiscountReasonsViewModel>
	{
		public OrderItemDiscountReasonsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ytreeviewDiscountReasons.CreateFluentColumnsConfig<DiscountReason>()
				.AddColumn("Основание скидки")
					.MinWidth(50)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewDiscountReasons.HeadersVisible = true;
			ytreeviewDiscountReasons.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderItemDiscountReasons, w => w.ItemsDataSource)
				.AddBinding(ViewModel, vm => vm.SelectedDiscountReason, w => w.SelectedRow)
				.InitializeFromSource();

			speciallistcomboboxDiscountReason.SetRenderTextFunc<DiscountReason>(dr => dr.Name);
			speciallistcomboboxDiscountReason.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AvailableDiscountReasons, w => w.ItemsList)
				.AddBinding(vm => vm.NewDiscountReason, w => w.SelectedItem)
				.InitializeFromSource();

			ybuttonAdd.BindCommand(ViewModel.AddDiscountReasonCommand);
			ybuttonDelete.BindCommand(ViewModel.DeleteDiscountReasonCommand);
		}
	}
}
