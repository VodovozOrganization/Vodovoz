using System;
using QS.ViewModels;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameter;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.ViewWidgets.AdvancedWageParameterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottlesCountAdvancedWageParameterWidget : TabViewBase<BottlesCountAdvancedWageParameterViewModel>
	{
		public BottlesCountAdvancedWageParameterWidget(BottlesCountAdvancedWageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yenumcomboboxLeftSing.ItemsEnum = typeof(ComparisonSings);
			yenumcomboboxRightSing.ItemsEnum = typeof(ComparisonSings);
			yspinbuttonLeftCount.Binding.AddBinding(ViewModel.Entity, e => e.BottlesFrom, w => w.Digits).InitializeFromSource();
		    validatedentryRightCount.Binding.AddBinding(ViewModel.Entity, e => e.BottlesTo, w => w.Digits).InitializeFromSource();
			yenumcomboboxLeftSing.Binding.AddBinding(ViewModel.Entity, e => e.LeftSing, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboboxRightSing.Binding.AddBinding(ViewModel.Entity, e => e.RightSing, w => w.SelectedItemOrNull).InitializeFromSource();
		}
	}
}
