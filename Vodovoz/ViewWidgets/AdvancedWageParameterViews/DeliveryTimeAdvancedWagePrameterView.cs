using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.ViewWidgets.AdvancedWageParameterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryTimeAdvancedWagePrameterView : TabViewBase<DeliveryTimeAdvancedWageParameterViewModel>
	{
		public DeliveryTimeAdvancedWagePrameterView(DeliveryTimeAdvancedWageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytimeentryFrom.Binding.AddBinding(ViewModel.Entity, e => e.StartTime , w => w.Time).InitializeFromSource();
			ytimeentryTo.Binding.AddBinding(ViewModel.Entity, e => e.EndTime, w => w.Time).InitializeFromSource();
		}
	}
}
