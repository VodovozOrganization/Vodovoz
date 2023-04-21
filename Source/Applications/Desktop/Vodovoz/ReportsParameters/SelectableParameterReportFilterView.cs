using System;
using Vodovoz.ViewModels.Reports;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Gamma.ColumnConfig;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelectableParameterReportFilterView : Gtk.Bin
	{
		private readonly SelectableParameterReportFilterViewModel viewModel;

		public SelectableParameterReportFilterView(SelectableParameterReportFilterViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			viewModel.PropertyChanged += ViewModel_PropertyChanged;
			ytreeviewParameterSets.Selection.Changed += Selection_Changed;
			search.TextChanged += Search_TextChanged;

			ConfigureBindings();

			ytreeviewParameterSets.ItemsDataSource = viewModel.ReportFilter.ParameterSets;
			ytreeviewParameterSets.ColumnsConfig = FluentColumnsConfig<SelectableParameterSet>.Create()
				.AddColumn("Параметр").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewParameters.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Title)
				.Finish();

			ytreeviewParameters.HeadersVisible = false;
			ytreeviewParameterSets.HeadersVisible = false;
		}

		private void ConfigureBindings()
		{
			buttonInclude.Clicked += (sender, e) => viewModel.SwitchToIncludeCommand.Execute();
			viewModel.SwitchToIncludeCommand.CanExecuteChanged += (sender, e) => buttonInclude.Sensitive = viewModel.SwitchToIncludeCommand.CanExecute();

			buttonExclude.Clicked += (sender, e) => viewModel.SwitchToExcludeCommand.Execute();
			viewModel.SwitchToExcludeCommand.CanExecuteChanged += (sender, e) => buttonExclude.Sensitive = viewModel.SwitchToExcludeCommand.CanExecute();

			ytreeviewParameters.Binding.AddBinding(viewModel, vm => vm.HasSelectedSet, w => w.Sensitive).InitializeFromSource();

			buttonSelectAll.Clicked += (sender, e) => viewModel.SelectAllParametersCommand.Execute();
			viewModel.SelectAllParametersCommand.CanExecuteChanged += (sender, e) => buttonSelectAll.Sensitive = viewModel.SelectAllParametersCommand.CanExecute();

			buttonUnselect.Clicked += (sender, e) => viewModel.UnselectAllParametersCommand.Execute();
			viewModel.UnselectAllParametersCommand.CanExecuteChanged += (sender, e) => buttonUnselect.Sensitive = viewModel.UnselectAllParametersCommand.CanExecute();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(viewModel.CurrentParameterSet):
					UpdateCurrentParameterSetFromSource();
					break;
				case nameof(viewModel.HasSelectedSet):
					UpdateSearchSensitivity();
					break;
				default:
					break;
			}
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			UpdateCurrentParameterSetToSource();
		}

		private void UpdateCurrentParameterSetToSource()
		{
			SelectableParameterSet selectedParameterSet = ytreeviewParameterSets.GetSelectedObject() as SelectableParameterSet;
			if(selectedParameterSet != null && selectedParameterSet == viewModel.CurrentParameterSet) {
				return;
			}

			viewModel.CurrentParameterSet = selectedParameterSet;
			if(viewModel.CurrentParameterSet != null) {
				viewModel.CurrentParameterSet.Parameters.ListContentChanged -= CurrentParameterSet_ListContentChanged;
				viewModel.CurrentParameterSet.Parameters.ListContentChanged += CurrentParameterSet_ListContentChanged;
			}
		}

		void CurrentParameterSet_ListContentChanged(object sender, EventArgs e)
		{
			ytreeviewParameters.QueueDraw();
		}

		private void UpdateCurrentParameterSetFromSource()
		{
			RefreshParametersSource();
			RefreshButtons();

			SelectableParameterSet selectedParameterSet = ytreeviewParameterSets.GetSelectedObject() as SelectableParameterSet;
			if(selectedParameterSet != null && selectedParameterSet == viewModel.CurrentParameterSet) {
				return;
			}

			if(viewModel.CurrentParameterSet == null)
			{
				return;
			}
			var selectedIter = ytreeviewParameterSets.YTreeModel.IterFromNode(viewModel.CurrentParameterSet);
			if(selectedIter.UserData == IntPtr.Zero) {
				return;
			}
			ytreeviewParameterSets.Selection.SelectIter(selectedIter);
		}

		private void RefreshButtons()
		{
			if(viewModel.CurrentParameterSet?.FilterType == SelectableFilterType.Exclude) {
				buttonExclude.Active = true;
			} else {
				buttonInclude.Active = true;
			}
		}

		private void RefreshParametersSource()
		{
			if(viewModel?.CurrentParameterSet == null) {
				ytreeviewParameters.ItemsDataSource = null;
				return;
			}

			var source = GetCurrentParameters();

			if(viewModel.CurrentParameterSet.Parameters.Any(x => x.Children.Any())) {
				var recursiveModel = new RecursiveTreeModel<SelectableParameter>(source, x => x.Parent, x => x.Children);
				ytreeviewParameters.YTreeModel = recursiveModel;
			} else {
				ytreeviewParameters.ItemsDataSource = source;
			}
		}

		private void UpdateSearchSensitivity()
		{
			search.Sensitive = viewModel.HasSelectedSet;
		}

		void Search_TextChanged(object sender, EventArgs e)
		{
			RefreshParametersSource();
		}

		private IList<SelectableParameter> GetCurrentParameters()
		{
			if(viewModel?.CurrentParameterSet?.Parameters == null) {
				return new List<SelectableParameter>();
			}

			viewModel.CurrentParameterSet.FilterParameters(search.Text);
			return viewModel.CurrentParameterSet.OutputParameters;
		}

	}
}
