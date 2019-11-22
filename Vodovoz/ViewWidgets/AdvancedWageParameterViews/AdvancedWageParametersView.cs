using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.ViewWidgets.AdvancedWageParameterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdvancedWageParametersView : WidgetViewBase<AdvancedWageParametersViewModel>
	{
		public AdvancedWageParametersView(AdvancedWageParametersViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yenumcombobox.ItemsEnum = typeof(AdvancedWageParameterType);
			yenumcombobox.Binding.AddBinding(ViewModel, vm => vm.ParameterType, w => w.SelectedItem).InitializeFromSource();
			widgetcontainerview.Binding.AddBinding(ViewModel, vm => vm.ParameterViewModel, w => w.WidgetViewModel).InitializeFromSource();
		}

		protected void OnYbuttonAddClicked(object sender, EventArgs e)
			=> ViewModel.AddCommand.Execute();

		protected void OnYbuttonCancelClicked(object sender, EventArgs e)
			=> ViewModel.CancelCreationCommand.Execute();
	}
}
