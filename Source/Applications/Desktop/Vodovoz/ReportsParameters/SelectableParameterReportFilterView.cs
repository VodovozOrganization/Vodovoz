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
		private readonly SelectableParameterReportFilterViewModel _viewModel;

		public SelectableParameterReportFilterView(SelectableParameterReportFilterViewModel viewModel)
		{
			Build();
			_viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			_viewModel.PropertyChanged += ViewModel_PropertyChanged;
			ytreeviewParameterSets.Selection.Changed += Selection_Changed;
			search.TextChanged += Search_TextChanged;

			ConfigureBindings();

			ytreeviewParameterSets.ItemsDataSource = _viewModel.ReportFilter.ParameterSets;
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
			buttonInclude.Clicked += (sender, e) => _viewModel.SwitchToIncludeCommand.Execute();
			_viewModel.SwitchToIncludeCommand.CanExecuteChanged += (sender, e) => buttonInclude.Sensitive = _viewModel.SwitchToIncludeCommand.CanExecute();

			buttonExclude.Clicked += (sender, e) => _viewModel.SwitchToExcludeCommand.Execute();
			_viewModel.SwitchToExcludeCommand.CanExecuteChanged += (sender, e) => buttonExclude.Sensitive = _viewModel.SwitchToExcludeCommand.CanExecute();

			ytreeviewParameters.Binding.AddBinding(_viewModel, vm => vm.HasSelectedSet, w => w.Sensitive).InitializeFromSource();

			buttonSelectAll.Clicked += (sender, e) => _viewModel.SelectAllParametersCommand.Execute();
			_viewModel.SelectAllParametersCommand.CanExecuteChanged += (sender, e) => buttonSelectAll.Sensitive = _viewModel.SelectAllParametersCommand.CanExecute();

			buttonUnselect.Clicked += (sender, e) => _viewModel.UnselectAllParametersCommand.Execute();
			_viewModel.UnselectAllParametersCommand.CanExecuteChanged += (sender, e) => buttonUnselect.Sensitive = _viewModel.UnselectAllParametersCommand.CanExecute();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(_viewModel.CurrentParameterSet):
					UpdateCurrentParameterSetFromSource();
					break;
				case nameof(_viewModel.HasSelectedSet):
					UpdateSearchSensitivity();
					break;
				case nameof(_viewModel.SearchValue):
					UpdateSearchText();
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
			if(selectedParameterSet != null && selectedParameterSet == _viewModel.CurrentParameterSet) {
				return;
			}

			_viewModel.CurrentParameterSet = selectedParameterSet;
			if(_viewModel.CurrentParameterSet != null) {
				_viewModel.CurrentParameterSet.Parameters.ListContentChanged -= CurrentParameterSet_ListContentChanged;
				_viewModel.CurrentParameterSet.Parameters.ListContentChanged += CurrentParameterSet_ListContentChanged;
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
			if(selectedParameterSet != null && selectedParameterSet == _viewModel.CurrentParameterSet) {
				return;
			}

			if(_viewModel.CurrentParameterSet == null)
			{
				return;
			}
			var selectedIter = ytreeviewParameterSets.YTreeModel.IterFromNode(_viewModel.CurrentParameterSet);
			if(selectedIter.UserData == IntPtr.Zero) {
				return;
			}
			ytreeviewParameterSets.Selection.SelectIter(selectedIter);
		}

		private void RefreshButtons()
		{
			if(_viewModel.CurrentParameterSet?.FilterType == SelectableFilterType.Exclude) {
				buttonExclude.Active = true;
			} else {
				buttonInclude.Active = true;
			}
		}
		
		private void UpdateSearchText()
		{
			search.Text = _viewModel.SearchValue;
			RefreshParametersSource();
		}

		private void RefreshParametersSource()
		{
			if(_viewModel?.CurrentParameterSet == null) {
				ytreeviewParameters.ItemsDataSource = null;
				return;
			}

			var source = GetCurrentParameters();

			if(_viewModel.CurrentParameterSet.Parameters.Any(x => x.Children.Any())) {
				var recursiveModel = new RecursiveTreeModel<SelectableParameter>(source, x => x.Parent, x => x.Children);
				ytreeviewParameters.YTreeModel = recursiveModel;
			} else {
				ytreeviewParameters.ItemsDataSource = source;
			}
		}

		private void UpdateSearchSensitivity()
		{
			search.Sensitive = _viewModel.HasSelectedSet;
		}

		void Search_TextChanged(object sender, EventArgs e)
		{
			_viewModel.SilentUpdateSearchValue(search.Text);
			RefreshParametersSource();
		}

		private IList<SelectableParameter> GetCurrentParameters()
		{
			if(_viewModel?.CurrentParameterSet?.Parameters == null) {
				return new List<SelectableParameter>();
			}

			_viewModel.CurrentParameterSet.FilterParameters(search.Text);
			return _viewModel.CurrentParameterSet.OutputParameters;
		}
	}
}
