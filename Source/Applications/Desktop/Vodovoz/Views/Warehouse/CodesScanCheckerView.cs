using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.Views.Warehouse
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CodesScanCheckerView :  Gtk.Bin//WidgetViewBase<CodesScanCheckerViewModel>
	{
		public CodesScanCheckerView()
		{
			Build();
		}
		
		// protected override void ConfigureWidget()
		// {
		// 	yentryCode.Binding.AddBinding(ViewModel, vm => vm.Code, w => w.Text).InitializeFromSource();
		// 	ylabelInfo.Binding.AddBinding(ViewModel, vm=>vm.ProgressInfo, w=>w.Text).InitializeFromSource();
		// 	
		// 	
		// 	ybuttonAccept.BindCommand(ViewModel.AcceptCommand);
		// }
	}
}
