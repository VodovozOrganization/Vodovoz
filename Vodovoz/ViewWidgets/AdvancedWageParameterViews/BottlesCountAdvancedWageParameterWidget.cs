using System.Linq;
using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;

namespace Vodovoz.ViewWidgets.AdvancedWageParameterViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottlesCountAdvancedWageParameterWidget : WidgetViewBase<BottlesCountAdvancedWageParameterViewModel>
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
			yspinbuttonLeftCount.Binding.AddBinding(ViewModel.Entity, e => e.BottlesFrom, w => w.ValueAsUint).InitializeFromSource();
		    yvalidatedentryRightCount.Binding.AddBinding(ViewModel.Entity, e => e.BottlesTo, w => w.Text, new UintToStringConverter()).InitializeFromSource();
			yvalidatedentryRightCount.Binding.AddBinding(ViewModel, vm => vm.CanSetRightCount, w => w.Sensitive).InitializeFromSource();
			yenumcomboboxLeftSing.Binding.AddBinding(ViewModel.Entity, e => e.LeftSing, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboboxRightSing.AddEnumToHideList(ViewModel.RightSingHideType.OfType<object>().ToArray());
			yenumcomboboxRightSing.Binding.AddBinding(ViewModel.Entity, e => e.RightSing, w => w.SelectedItemOrNull).InitializeFromSource();
		}
	}
}
