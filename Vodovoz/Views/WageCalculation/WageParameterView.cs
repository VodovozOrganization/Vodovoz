using Gamma.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageParameterView : TabViewBase<WageParameterViewModel>
	{
		public WageParameterView(WageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			cmbWageCalcType.SetRenderTextFunc<WageCalculationType>(p => p.GetEnumTitle());
			cmbWageCalcType.Binding.AddBinding(ViewModel, e => e.AvailableWageCalcTypes, w => w.ItemsList).InitializeFromSource();
			cmbWageCalcType.Binding.AddBinding(ViewModel.Entity, e => e.WageCalcType, w => w.SelectedItem).InitializeFromSource();

			spinWageCalcRate.Binding.AddBinding(ViewModel.Entity, e => e.WageCalcRate, w => w.ValueAsDecimal).InitializeFromSource();
			spinWageCalcRate.Binding.AddBinding(ViewModel, e => e.WageCalcRateMaxValue, w => w.Adjustment.Upper).InitializeFromSource();
			spinWageCalcRate.Binding.AddBinding(ViewModel, e => e.IsWageCalcRateVisible, w => w.Visible).InitializeFromSource();
			lblUnit.Binding.AddBinding(ViewModel, s => s.WageCalcRateUnit, w => w.Text).InitializeFromSource();
			lblUnit.Binding.AddBinding(ViewModel, s => s.IsWageCalcRateVisible, w => w.Visible).InitializeFromSource();

			spinQuantityOfFullBottlesToSell.Binding.AddBinding(ViewModel.Entity, e => e.QuantityOfFullBottlesToSell, w => w.ValueAsInt).InitializeFromSource();
			spinQuantityOfFullBottlesToSell.Binding.AddBinding(ViewModel, e => e.AreQuantitiesForSalesPlanVisible, w => w.Visible).InitializeFromSource();
			lblPlanFullUnit.Binding.AddBinding(ViewModel, s => s.AreQuantitiesForSalesPlanVisible, w => w.Visible).InitializeFromSource();

			spinQuantityOfEmptyBottlesToTake.Binding.AddBinding(ViewModel.Entity, e => e.QuantityOfEmptyBottlesToTake, w => w.ValueAsInt).InitializeFromSource();
			spinQuantityOfEmptyBottlesToTake.Binding.AddBinding(ViewModel, e => e.AreQuantitiesForSalesPlanVisible, w => w.Visible).InitializeFromSource();
			lblPlanEmptyUnit.Binding.AddBinding(ViewModel, s => s.AreQuantitiesForSalesPlanVisible, w => w.Visible).InitializeFromSource();

			chkIsArchive.Binding.AddBinding(ViewModel.Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();

			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false);
		}
	}
}
