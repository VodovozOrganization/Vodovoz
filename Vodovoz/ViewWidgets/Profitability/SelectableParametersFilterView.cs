using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets;
using Gamma.ColumnConfig;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Gamma.Binding;

namespace Vodovoz.ViewWidgets.Profitability
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelectableParametersFilterView : WidgetViewBase<SelectableParametersFilterViewModel>
	{
		public SelectableParametersFilterView(SelectableParametersFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			chkSelectAll.Toggled += (sender, e) => ViewModel.SelectUnselectAllCommand.Execute(chkSelectAll.Active);
			ConfigureTreeView();
		}

		private void ConfigureTreeView()
		{
			treeViewSelectableParameters.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("Выбрать")
					.AddToggleRenderer(x => x.Selected)
					.Editing()
				.AddColumn("Название")
					.AddTextRenderer(x => x.Title)
				.Finish();

			treeViewSelectableParameters.HeadersVisible = false;

			if(ViewModel.IsRecursiveParameters)
			{
				treeViewSelectableParameters.YTreeModel =
					new RecursiveTreeModel<SelectableParameter>(ViewModel.HighParents, x => x.Parent, x => x.Children);
			}
			else
			{
				treeViewSelectableParameters.ItemsDataSource = ViewModel.Parameters;
			}
		}
	}
}
