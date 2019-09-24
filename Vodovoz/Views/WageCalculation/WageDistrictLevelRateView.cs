using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageDistrictLevelRateView : EntityWidgetViewBase<WageDistrictLevelRateViewModel>
	{
		readonly bool editable;
		public WageDistrictLevelRateView(WageDistrictLevelRateViewModel viewModel, bool editable) : base(viewModel)
		{
			this.editable = editable;
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			btnFillRates.Binding.AddBinding(ViewModel, s => s.CanFillRates, w => w.Sensitive).InitializeFromSource();
			btnFillRates.Clicked += (sender, e) => ViewModel.CreateAndFillNewRatesCommand.Execute();

			treeViewWageRates.ColumnsConfig = FluentColumnsConfig<WageRate>.Create()
				.AddColumn("Название ставки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.WageRateType.GetEnumTitle())
				.AddColumn("Для водителя\nс экспедитором")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(r => r.ForDriverWithForwarder)
						.Digits(2)
						.XAlign(1f)
						.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
						.Editing(editable)
					.AddTextRenderer(r => r.GetUnitName, false)
				.AddColumn("Для водителя\nбез экспедитора")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(r => r.ForDriverWithoutForwarder)
						.Digits(2)
						.XAlign(1f)
						.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
						.Editing(editable)
					.AddTextRenderer(r => r.GetUnitName, false)
				.AddColumn("Для\nэкспедитора")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(r => r.ForForwarder)
						.Digits(2)
						.XAlign(1f)
						.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
						.Editing(editable)
					.AddTextRenderer(r => r.GetUnitName, false)
				.AddColumn("")
				.Finish();

			treeViewWageRates.ItemsDataSource = ViewModel.Entity.ObservableWageRates;
		}
	}
}
