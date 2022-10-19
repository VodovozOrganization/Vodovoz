using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FixedWageParameterView : WidgetViewBase<FixedWageParameterItemViewModel>
	{
		public FixedWageParameterView(FixedWageParameterItemViewModel itemViewModel) : base(itemViewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			comboFixedType.ItemsEnum = typeof(FixedWageTypes);
			comboFixedType.Binding.AddBinding(ViewModel.Entity, vm => vm.FixedWageType, w => w.SelectedItem).InitializeFromSource();
			comboFixedType.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryRouteListFixedWage.Binding.AddBinding(ViewModel.Entity, e => e.RouteListFixedWage, w => w.ValueAsDecimal).InitializeFromSource();
			entryRouteListFixedWage.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

		}
	}
}
