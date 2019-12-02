using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.ViewWidgets.AdvancedWageParameterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryTimeAdvancedWagePrameterView : WidgetViewBase<DeliveryTimeAdvancedWageParameterViewModel>
	{
		public DeliveryTimeAdvancedWagePrameterView(DeliveryTimeAdvancedWageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			timerangepicker.Binding.AddBinding(ViewModel.Entity, e => e.StartTime , w => w.TimeStart).InitializeFromSource();
			timerangepicker.Binding.AddBinding(ViewModel.Entity, e => e.EndTime, w => w.TimeEnd).InitializeFromSource();
		}
	}
}
