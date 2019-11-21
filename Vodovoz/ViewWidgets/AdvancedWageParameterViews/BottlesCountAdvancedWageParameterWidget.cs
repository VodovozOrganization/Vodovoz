using System;
using QS.ViewModels;
using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
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
		    yvalidatedentryRightCount.Binding.AddBinding(ViewModel.Entity, e => e.BottlesTo, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			yvalidatedentryRightCount.Binding.AddBinding(ViewModel, vm => vm.CanSetRightCount, w => w.Sensitive).InitializeFromSource();
			yenumcomboboxLeftSing.Binding.AddBinding(ViewModel.Entity, e => e.LeftSing, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboboxRightSing.Binding.AddBinding(ViewModel.Entity, e => e.RightSing, w => w.SelectedItemOrNull).InitializeFromSource();
		}
	}
}
