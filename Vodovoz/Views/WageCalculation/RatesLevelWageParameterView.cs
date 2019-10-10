using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RatesLevelWageParameterView : EntityWidgetViewBase<RatesLevelWageParameterViewModel>
	{
		public RatesLevelWageParameterView(RatesLevelWageParameterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			cmbLevels.SetRenderTextFunc<WageDistrictLevelRates>(x => x.Name);
			cmbLevels.Binding.AddBinding(ViewModel, s => s.WageLevels, w => w.ItemsList).InitializeFromSource();
			cmbLevels.Binding.AddBinding(ViewModel.Entity, s => s.WageDistrictLevelRates, w => w.SelectedItem).InitializeFromSource();
			cmbLevels.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.LevelChanged += (sender, e) => GenerateTabs();
			GenerateTabs();
		}

		Notebook nbDistricts;
		void GenerateTabs()
		{
			if(nbDistricts != null)
				nbDistricts.Destroy();
			nbDistricts = new Notebook();

			if(ViewModel.Entity.WageDistrictLevelRates?.LevelRates == null)
				return;

			foreach(var levelRate in ViewModel.Entity.WageDistrictLevelRates.LevelRates) {
				yTreeView yTreeRatesInfo = new yTreeView {
					CanFocus = true,
					Name = nameof(yTreeRatesInfo),
					ItemsDataSource = levelRate.ObservableWageRates,
					ColumnsConfig = FluentColumnsConfig<WageRate>.Create()
						.AddColumn("Название ставки")
							.HeaderAlignment(0.5f)
							.AddTextRenderer(x => x.WageRateType.GetEnumTitle())
						.AddColumn("Водитель с\nэкспедитором")
							.HeaderAlignment(0.5f)
							.AddTextRenderer(r => r.GetForDriverWithForwarderString, false)
						.AddColumn("Водитель без\nэкспедитора")
							.HeaderAlignment(0.5f)
							.AddTextRenderer(r => r.GetForDriverWithoutForwarderString, false)
						.AddColumn("Экспедитор")
							.HeaderAlignment(0.5f)
							.AddTextRenderer(r => r.GetForForwarderString, false)
						.AddColumn("")
						.Finish()
				};
				VBox vbx = new VBox {
					yTreeRatesInfo
				};
				Box.BoxChild viewBox = (Box.BoxChild)vbx[yTreeRatesInfo];
				viewBox.Fill = true;
				viewBox.Expand = true;
				var scrolledWindow = new ScrolledWindow {
					vbx
				};

				Label tabLabel = new Label {
					UseMarkup = true,
					Markup = levelRate.WageDistrict.Name
				};

				nbDistricts.AppendPage(scrolledWindow, tabLabel);
			}
			hbxNotebooksWithDistricts.Add(nbDistricts);
			hbxNotebooksWithDistricts.ShowAll();
		}
	}
}