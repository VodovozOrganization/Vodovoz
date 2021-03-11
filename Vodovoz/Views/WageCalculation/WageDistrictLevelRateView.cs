using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WageDistrictLevelRateView : WidgetViewBase<WageDistrictLevelRateViewModel>
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
			ybuttonRemoveParameter.Binding.AddBinding(ViewModel, e => e.IsAdvancedParameterSelected, w => w.Sensitive).InitializeFromSource();
			ybuttonAddParameter.Binding.AddBinding(ViewModel, e => e.IsNodeSelected, w => w.Sensitive).InitializeFromSource();

			widgetcontainerview.Binding.AddBinding(ViewModel, s => s.AdvancedWidgetViewModel, w => w.WidgetViewModel).InitializeFromSource();
			ViewModel.WageRatesUpdate += () => { 
				treeViewWageRates?.YTreeModel?.EmitModelChanged();
				treeViewWageRates.ExpandAll();
			};

			ViewModel.WageRatesFill += () => {
				treeViewWageRates.YTreeModel = new RecursiveTreeConfig<IWageHierarchyNode>
				(x => x.Parent, x => x.Children)
				.CreateModel(ViewModel.Entity.ObservableWageRates);
			};
			treeViewWageRates.ColumnsConfig = FluentColumnsConfig<IWageHierarchyNode>.Create()
				.AddColumn("Название ставки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Name)
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

			treeViewWageRates.YTreeModel = new RecursiveTreeConfig<IWageHierarchyNode>
					(x => x.Parent, x => x.Children)
					.CreateModel(ViewModel.Entity.ObservableWageRates);
			treeViewWageRates.ExpandAll();
			treeViewWageRates.Selection.Changed += (sender, e) => ViewModel.SelectionChangedCommand.Execute(treeViewWageRates.GetSelectedObject<IWageHierarchyNode>());
		}

		protected void OnTreeViewWageRatesRowActivated(object o, RowActivatedArgs args)
			=> ViewModel.OpenAdvancedParametersCommand.Execute(treeViewWageRates.GetSelectedObject());

		protected void OnYbuttonRemoveParameterClicked(object sender, System.EventArgs e)
			=> ViewModel.DeleteAdvancedParametersCommand.Execute(treeViewWageRates.GetSelectedObject());

		protected void OnYbuttonAddParameterClicked(object sender, System.EventArgs e)
			=> ViewModel.AddNewParameterCommand.Execute(treeViewWageRates.GetSelectedObject());
	}
}
