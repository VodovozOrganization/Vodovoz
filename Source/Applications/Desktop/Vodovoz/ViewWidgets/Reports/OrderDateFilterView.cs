using System;
using QS.ViewModels;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewWidgets.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderDateFilterView : WidgetViewBase<OrderDateFilterViewModel>
	{
		public OrderDateFilterView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			yradiobuttonCreationDate.Binding
				.AddBinding(ViewModel, vm => vm.FilteringByCreationDate, w => w.Active)
				.InitializeFromSource();

			yradiobuttonDeliveryDate.Binding
				.AddBinding(ViewModel, vm => vm.FilteringByDeliveryDate, w => w.Active)
				.InitializeFromSource();

			yradiobuttonPaymentDate.Binding
				.AddBinding(ViewModel, vm => vm.FilteringByPaymentDate, w => w.Active)
				.InitializeFromSource();
		}
	}


}
