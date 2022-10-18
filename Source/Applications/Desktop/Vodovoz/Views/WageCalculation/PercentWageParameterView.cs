using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PercentWageParameterView : WidgetViewBase<PercentWageParameterItemViewModel>
	{
		public PercentWageParameterView(PercentWageParameterItemViewModel itemViewModel) : base(itemViewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			comboPercentType.ItemsEnum = typeof(PercentWageTypes);
			comboPercentType.Binding.AddBinding(ViewModel.Entity, vm => vm.PercentWageType, w => w.SelectedItem).InitializeFromSource();
			comboPercentType.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryRouteListPercentWage.Binding.AddBinding(ViewModel.Entity, e => e.RouteListPercent, w => w.ValueAsDecimal).InitializeFromSource();
			entryRouteListPercentWage.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			entryRouteListPercentWage.Binding.AddBinding(ViewModel, vm => vm.RouteListPercentVisibility, w => w.Visible).InitializeFromSource();
			ylabelWageForRouteList.Binding.AddBinding(ViewModel, vm => vm.RouteListPercentVisibility, w => w.Visible).InitializeFromSource();

		}
	}
}
