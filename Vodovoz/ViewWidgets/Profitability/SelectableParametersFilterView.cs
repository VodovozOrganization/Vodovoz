using System;
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
		
		public void UpdateViewModel(SelectableParametersFilterViewModel viewModel)
		{
			ViewModel.Parameters.ListContentChanged -= OnParametersListContentChanged;
			ViewModel = viewModel;
			SetTitle();
			chkSelectAll.Active = ViewModel.IsSelectAll;
			treeViewSelectableParameters.YTreeModel = null;
			treeViewSelectableParameters.ItemsDataSource = null;
			SetParametersItemsSource();
		}

		private void Configure()
		{
			SetTitle();
			//Передаем инвертированное значение, т.к. оно еще не поменялось: при установке в true, Active еще false и наоборот
			chkSelectAll.Pressed += (sender, e) => ViewModel.SelectUnselectAllCommand.Execute(!chkSelectAll.Active);
			ConfigureTreeView();
		}

		private void SetTitle()
		{
			lblTitle.LabelProp = ViewModel.Title;
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
			SetParametersItemsSource();
		}

		private void SetParametersItemsSource()
		{
			if(ViewModel.IsRecursiveParameters)
			{
				treeViewSelectableParameters.YTreeModel =
					new RecursiveTreeModel<SelectableParameter>(ViewModel.Parameters, x => x.Parent, x => x.Children);
			}
			else
			{
				treeViewSelectableParameters.ItemsDataSource = ViewModel.Parameters;
			}
			
			ViewModel.Parameters.ListContentChanged += OnParametersListContentChanged;
		}

		private void OnParametersListContentChanged(object sender, EventArgs e)
		{
			treeViewSelectableParameters.QueueDraw();
		}
	}
}
