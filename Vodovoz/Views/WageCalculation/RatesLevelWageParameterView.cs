using Gamma.Binding;
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
	public partial class RatesLevelWageParameterView : WidgetViewBase<RatesLevelWageParameterItemViewModel>
	{
		public RatesLevelWageParameterView(RatesLevelWageParameterItemViewModel itemViewModel) : base(itemViewModel)
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
					ColumnsConfig = FluentColumnsConfig<IWageHierarchyNode>.Create()
						.AddColumn("Название ставки")
							.HeaderAlignment(0.5f)
							.AddTextRenderer(x => x.Name)
						.AddColumn("Водитель с\nэкспедитором")
							.AddNumericRenderer(r => r.ForDriverWithForwarder)
							.Digits(2)
							.XAlign(1f)
							.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
							.AddTextRenderer(r => r.GetUnitName, false)
						.AddColumn("Водитель без\nэкспедитора")
							.AddNumericRenderer(r => r.ForDriverWithoutForwarder)
							.Digits(2)
							.XAlign(1f)
							.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
							.AddTextRenderer(r => r.GetUnitName, false)
						.AddColumn("Экспедитор")
							.AddNumericRenderer(r => r.ForForwarder)
							.Digits(2)
							.XAlign(1f)
							.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
							.AddTextRenderer(r => r.GetUnitName, false)
						.AddColumn("")
						.Finish()
				};
				yTreeRatesInfo.YTreeModel = new RecursiveTreeConfig<IWageHierarchyNode>
					(x => x.Parent, x => x.Children)
					.CreateModel(levelRate.ObservableWageRates);
				yTreeRatesInfo.ExpandAll();
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