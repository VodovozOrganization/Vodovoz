using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalesPlanWageParameterView : WidgetViewBase<SalesPlanWageParameterItemViewModel>
	{
		public SalesPlanWageParameterView(SalesPlanWageParameterItemViewModel itemViewModel) : base(itemViewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			cmbSalesPlans.SetRenderTextFunc<SalesPlan>(x => x.Title);
			cmbSalesPlans.Binding.AddBinding(ViewModel, s => s.GetSalesPlans, w => w.ItemsList).InitializeFromSource();
			cmbSalesPlans.Binding.AddBinding(ViewModel.Entity, s => s.SalesPlan, w => w.SelectedItem).InitializeFromSource();
			cmbSalesPlans.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();
		}
	}
}