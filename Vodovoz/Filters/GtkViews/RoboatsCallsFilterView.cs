using System;
namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RoboatsCallsFilterView : Gtk.Bin
	{
		public RoboatsCallsFilterView()
		{
			this.Build();

			//enumcomboStatus.ItemsEnum = typeof(OrderStatus);
			//enumcomboStatus.Binding.AddSource(ViewModel)
			//	.AddBinding(vm => vm.CanChangeStatus, w => w.Sensitive)
			//	.AddBinding(vm => vm.RestrictStatus, w => w.SelectedItemOrNull)
			//	.InitializeFromSource();

			//dateperiodOrders.Binding.AddSource(ViewModel)
				//.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				//.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				//.AddFuncBinding(vm => vm.CanChangeStartDate && vm.CanChangeEndDate, w => w.Sensitive)
				//.InitializeFromSource();
		}
	}
}
